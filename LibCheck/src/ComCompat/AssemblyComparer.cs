using System;
using System.Collections;
using System.Runtime.InteropServices;

/// <summary>
/// Checks that one assembly is compatible with another from COM's perspective.
/// </summary>
namespace ComComparer 
{
	public class AssemblyComparer
	{
		private UCOMITypeLib oldTlb, newTlb;
		private int oldTypeCount, newTypeCount;
		private ArrayList compatErrors, compatWarnings, compatOtherDiffs;
		private Hashtable newTypes;
		private string currentTypeName = "";

		const int MEMBERID_NIL = -1;
		const int MAX_NAMES = 64;

		/// <summary>
		/// Constructs a new AssemblyComparer instance from two type library filenames.
		/// </summary>
		/// <exception cref="COMException">Throws COMException if one of the type libraries can't be loaded.</exception>
		/// <param name="newAssemblyFile">The new assembly, which is meant to be compatible with the old assembly.</param>
		/// <param name="oldAssemblyFile">The old assembly.</param>
		public AssemblyComparer(string oldAssemblyFile, string newAssemblyFile)
		{
			Exporter exporter = new Exporter();
			oldTlb = exporter.Export(oldAssemblyFile, null);
			newTlb = exporter.Export(newAssemblyFile, null);

			oldTypeCount = oldTlb.GetTypeInfoCount();
			newTypeCount = newTlb.GetTypeInfoCount();

			newTypes = new Hashtable(newTypeCount);

			UCOMITypeInfo newTypeInfo = null;

			for (int i = 0; i < newTypeCount; i++)
			{
				newTlb.GetTypeInfo(i, out newTypeInfo);

				// Use the TypeInfo name as the hashtable key since non-interface GUIDs are
				// allowed to change
				newTypes.Add(Marshal.GetTypeInfoName(newTypeInfo), newTypeInfo);
			}
		}

		/// <summary>
		/// The errors as a result of comparison.
		/// </summary>
		public ArrayList Errors { get { return compatErrors; } }

		/// <summary>
		/// The warnings as a result of comparison.
		/// </summary>
		public ArrayList Warnings { get { return compatWarnings; } }

		/// <summary>
		/// The other diffs as a result of comparison.
		/// </summary>
		public ArrayList OtherDiffs { get { return compatOtherDiffs; } }

		/// <summary>
		/// Checks the compatibility of the type libraries passed to the constructor.
		/// </summary>
		public void CheckCompatibility()
		{
			UCOMITypeInfo oldTypeInfo = null;
			UCOMITypeInfo newTypeInfo = null;
			TYPEATTR newAttr, oldAttr;
			TYPELIBATTR newLibAttr, oldLibAttr;
			IntPtr newPtr = IntPtr.Zero;
			IntPtr oldPtr = IntPtr.Zero;
			string currentTypeKind;

			// Initialize counting-related variables
			compatErrors = new ArrayList();
			compatWarnings = new ArrayList();
			compatOtherDiffs = new ArrayList();

			// First compare the info about the libraries themselves
			try
			{
				// Extract TYPEATTR from old TypeLib
				oldTlb.GetLibAttr(out oldPtr);
				oldLibAttr = (TYPELIBATTR)Marshal.PtrToStructure(oldPtr, typeof(TYPELIBATTR));

				// Extract TYPEATTR from new TypeLib
				newTlb.GetLibAttr(out newPtr);
				newLibAttr = (TYPELIBATTR)Marshal.PtrToStructure(newPtr, typeof(TYPELIBATTR));

				CompareTLibAttrs(oldLibAttr, newLibAttr);
			}
			finally
			{
				if (oldPtr != IntPtr.Zero) oldTlb.ReleaseTLibAttr(oldPtr);
				if (newPtr != IntPtr.Zero) newTlb.ReleaseTLibAttr(newPtr);
			}

			oldPtr = IntPtr.Zero;
			newPtr = IntPtr.Zero;

			// Ensure that each type in the old type library is in the new type library
			for (int i = 0; i < oldTypeCount; i++)
			{
				try
				{
					// Extract old TypeInfo from old type library
					oldTlb.GetTypeInfo(i, out oldTypeInfo);

					// Extract TYPEATTR from old TypeInfo
					oldTypeInfo.GetTypeAttr(out oldPtr);
					oldAttr = (TYPEATTR)Marshal.PtrToStructure(oldPtr, typeof(TYPEATTR));

					// Extract the old type name
					currentTypeName = Marshal.GetTypeInfoName(oldTypeInfo);

					switch (oldAttr.typekind)
					{
						case TYPEKIND.TKIND_ENUM: currentTypeKind = "an enum"; break;
						case TYPEKIND.TKIND_RECORD: currentTypeKind = "a struct"; break;
						case TYPEKIND.TKIND_UNION: currentTypeKind = "a union"; break;
						case TYPEKIND.TKIND_ALIAS: currentTypeKind = "an alias"; break;
						case TYPEKIND.TKIND_MODULE: currentTypeKind = "a module"; break;
						case TYPEKIND.TKIND_INTERFACE: currentTypeKind = "an interface"; break;
						case TYPEKIND.TKIND_DISPATCH: currentTypeKind = "an interface"; break;
						case TYPEKIND.TKIND_COCLASS: currentTypeKind = "a coclass"; break;
						default:
							throw new ApplicationException("Unknown TYPEKIND encountered: " + oldAttr.typekind);
					}

					// Check if the same-named type is in the new type library
					if (newTypes.Contains(currentTypeName))
					{
						// Extract new TypeInfo from the hashtable
						newTypeInfo = (UCOMITypeInfo)newTypes[currentTypeName];

						// Extract TYPEATTR from new TypeInfo
						newTypeInfo.GetTypeAttr(out newPtr);
						newAttr = (TYPEATTR)Marshal.PtrToStructure(newPtr, typeof(TYPEATTR));

						// Comparison Step 1: Compare TYPEATTRs
						if (CompareTypeAttrs(oldAttr, newAttr, oldTypeInfo, newTypeInfo))
						{
							// We know that the typekind values are equal at this point,
							// so do further comparison of types.

							// Comparison Step 2: Check different things depending on the kind of type
							switch (oldAttr.typekind)
							{
								case TYPEKIND.TKIND_ENUM:
								case TYPEKIND.TKIND_RECORD:
								case TYPEKIND.TKIND_UNION:
								case TYPEKIND.TKIND_ALIAS:
									CheckTypeDef(oldTypeInfo, newTypeInfo, oldAttr, newAttr);
									break;
								case TYPEKIND.TKIND_MODULE:
									throw new ApplicationException("Tool error: Exported type libraries should not contain modules.");
								case TYPEKIND.TKIND_INTERFACE:
									// These are interfaces that don't derive from IDispatch
									CheckInterface(oldTypeInfo, newTypeInfo, oldAttr, newAttr);
									break;
								case TYPEKIND.TKIND_DISPATCH:
									// These are interfaces that derive from IDispatch or dispinterfaces
									CheckInterface(oldTypeInfo, newTypeInfo, oldAttr, newAttr);
									break;
								case TYPEKIND.TKIND_COCLASS:
									CheckCoclass(oldTypeInfo, newTypeInfo, oldAttr, newAttr);
									break;
								default:
									throw new ApplicationException("Unknown TYPEKIND encountered: " + oldAttr.typekind);
							}
						}
					}
					else
					{
						ReportCompatError("The new type library is missing " + currentTypeKind + " named '" + currentTypeName + "'.");
					}
				}
				finally
				{
					if (oldPtr != IntPtr.Zero) oldTypeInfo.ReleaseTypeAttr(oldPtr);
					if (newPtr != IntPtr.Zero) newTypeInfo.ReleaseTypeAttr(newPtr);
				}
			}
		}

		/// <summary>
		/// Compares TYPELIBATTRs. Reports different LIBIDs and different version numbers
		/// (which should be expected) as "other differences."
		/// </summary>
		/// <param name="oldAttr">The old TYPELIBATTR</param>
		/// <param name="newAttr">The new TYPELIBATTR</param>
		/// <returns></returns>
		private void CompareTLibAttrs(TYPELIBATTR oldAttr, TYPELIBATTR newAttr)
		{
			if (oldAttr.guid != newAttr.guid)
				ReportCompatOtherDiff("The type libraries have different LIBIDs.", oldAttr.guid.ToString(), newAttr.guid.ToString());

			if (oldAttr.lcid != newAttr.lcid)
				ReportCompatError("The type libraries have different Locale IDs.", oldAttr.lcid.ToString(), newAttr.lcid.ToString());

			if (oldAttr.syskind != newAttr.syskind)
				ReportCompatOtherDiff("The type libraries list different target platforms.", oldAttr.syskind.ToString(), newAttr.syskind.ToString());

			if (oldAttr.wLibFlags != newAttr.wLibFlags)
				ReportCompatOtherDiff("The type libraries have different library flags.", oldAttr.wLibFlags.ToString(), newAttr.wLibFlags.ToString());

			// If the new type library has a higher version, treat give a different message
			if (oldAttr.wMajorVerNum > newAttr.wMajorVerNum || 
				(oldAttr.wMajorVerNum == newAttr.wMajorVerNum && oldAttr.wMinorVerNum > newAttr.wMinorVerNum))
			{
				ReportCompatOtherDiff("The newer type library has an older version number.  Are you sure you listed them in the correct order?", oldAttr.wMajorVerNum.ToString() + "." + oldAttr.wMinorVerNum.ToString(), newAttr.wMajorVerNum.ToString() + "." + newAttr.wMinorVerNum.ToString());
			}
			else if (oldAttr.wMajorVerNum != newAttr.wMajorVerNum || oldAttr.wMinorVerNum != newAttr.wMinorVerNum)
			{
				ReportCompatOtherDiff("The new type library has a newer version number, which is expected.", oldAttr.wMajorVerNum.ToString() + "." + oldAttr.wMinorVerNum.ToString(), newAttr.wMajorVerNum.ToString() + "." + newAttr.wMinorVerNum.ToString());
			}
		}

		/// <summary>
		/// Ensures that the new TYPEATTR is compatible with the old TYPEATTR.
		/// This means that they are identical except that the GUID can change for non-interfaces (but produces
		/// a warning), and the number of implemented interfaces (cImplTypes) is ignored because it's 
		/// treated specially for coclasses.
		/// Returns true if type kinds are equal (even if other incompatibilities exist), false otherwise. 
		/// </summary>
		/// <param name="oldAttr">TYPEATTR corresponding to the old TypeInfo</param>
		/// <param name="newAttr">TYPEATTR corresponding to the new TypeInfo</param>
		/// <param name="oldInfo">The old TypeInfo</param>
		/// <param name="newInfo">The new TypeInfo</param>
		/// <returns></returns>
		private bool CompareTypeAttrs(TYPEATTR oldAttr, TYPEATTR newAttr, UCOMITypeInfo oldInfo, UCOMITypeInfo newInfo)
		{
			// This comparison ignores cImplTypes because it's handled specially for coclasses.

			if (oldAttr.typekind != newAttr.typekind)
			{
				ReportCompatError("'" + currentTypeName + "' is a different kind of type.  Aborting further comparison of this type.", oldAttr.typekind.ToString(), newAttr.typekind.ToString());
				return false;
			}

			// Special-case GUIDs because it can be okay for them to change
			if (!oldAttr.guid.Equals(newAttr.guid))
			{
				// Report an error if the type is an interface
				if (oldAttr.typekind == TYPEKIND.TKIND_INTERFACE || oldAttr.typekind == TYPEKIND.TKIND_DISPATCH)
				{
					ReportCompatError("Interface '" + currentTypeName + "' has a different IID.", oldAttr.guid.ToString(), newAttr.guid.ToString());
				}
				else
				{
					ReportCompatWarning("'" + currentTypeName + "' has a new GUID, which may not be compatible.", oldAttr.guid.ToString(), newAttr.guid.ToString());
				}
			}
			if (oldAttr.cbAlignment != newAttr.cbAlignment)
				ReportCompatError("The type '" + currentTypeName + "' has a different alignment.", oldAttr.cbAlignment.ToString(), newAttr.cbAlignment.ToString());

			if (oldAttr.cbSizeInstance != newAttr.cbSizeInstance)
				ReportCompatError("The type '" + currentTypeName + "' has a different size.", oldAttr.cbSizeInstance.ToString() + " bytes", newAttr.cbSizeInstance.ToString() + " bytes");

			if (oldAttr.cbSizeVft != newAttr.cbSizeVft)
				ReportCompatError("The type '" + currentTypeName + "' has a different-sized v-table.", oldAttr.cbSizeVft.ToString(), newAttr.cbSizeVft.ToString());

			if (oldAttr.cFuncs != newAttr.cFuncs)
				ReportCompatError("The type '" + currentTypeName + "' has a different number of functions.", oldAttr.cFuncs.ToString() + ", counting base interface methods", newAttr.cFuncs.ToString() + ", counting base interface methods");

			if (oldAttr.cVars != newAttr.cVars)
				ReportCompatError("The type '" + currentTypeName + "' has a different number of variables/data members.", oldAttr.cVars.ToString(), newAttr.cVars.ToString());

			if (oldAttr.idldescType.wIDLFlags != newAttr.idldescType.wIDLFlags)
				ReportCompatError("The type '" + currentTypeName + "' has different IDL attributes.", oldAttr.idldescType.wIDLFlags.ToString(), newAttr.idldescType.wIDLFlags.ToString());

			if (oldAttr.lcid != newAttr.lcid)
				ReportCompatError("The type '" + currentTypeName + "' has a different LCID.", oldAttr.lcid.ToString(), newAttr.lcid.ToString());

			if (oldAttr.memidConstructor != newAttr.memidConstructor)
				ReportCompatError("The type '" + currentTypeName + "' has a different constructor ID.", oldAttr.memidConstructor.ToString(), newAttr.memidConstructor.ToString());

			if (oldAttr.memidDestructor != newAttr.memidDestructor)
				ReportCompatError("The type '" + currentTypeName + "' has a different destructor ID.", oldAttr.memidDestructor.ToString(), newAttr.memidDestructor.ToString());

			CompareTypeDescs(oldAttr.tdescAlias, newAttr.tdescAlias, oldInfo, newInfo, "", "", "");

			// If version number changes, only report as error if not an interface.
			// It shouldn't really matter for any type, but it also should only occur for interfaces
			// in exported type libraries because the exporter only generates 0.0 for class interfaces
			// and 1.0 for everything else (such as real interfaces).
			if (oldAttr.wMajorVerNum != newAttr.wMajorVerNum || oldAttr.wMinorVerNum != newAttr.wMinorVerNum)
			{
				if (newAttr.typekind == TYPEKIND.TKIND_INTERFACE || newAttr.typekind == TYPEKIND.TKIND_DISPATCH)
					ReportCompatOtherDiff("The interface '" + currentTypeName + "' has a different version number, which should not break compatibility.  This simply indicates that it's a class interface in one type library, but a real interface in the other type library.", oldAttr.wMajorVerNum.ToString() + "." + oldAttr.wMinorVerNum.ToString(), newAttr.wMajorVerNum.ToString() + "." + newAttr.wMinorVerNum.ToString());
				else
					ReportCompatError("The type '" + currentTypeName + "' has a different version number.", oldAttr.wMajorVerNum.ToString() + "." + oldAttr.wMinorVerNum.ToString(), newAttr.wMajorVerNum.ToString() + "." + newAttr.wMinorVerNum.ToString());
			}

			// Again special case interfaces because replacing a class interface
			// with a real interface can remove the [hidden] and [nonextensible] attributes,
			// which should still be compatible.
			if (oldAttr.wTypeFlags != newAttr.wTypeFlags)
			{
				if ((newAttr.typekind == TYPEKIND.TKIND_INTERFACE || newAttr.typekind == TYPEKIND.TKIND_DISPATCH) &&
					((oldAttr.wTypeFlags & TYPEFLAGS.TYPEFLAG_FHIDDEN & TYPEFLAGS.TYPEFLAG_FNONEXTENSIBLE) == (newAttr.wTypeFlags & TYPEFLAGS.TYPEFLAG_FHIDDEN & TYPEFLAGS.TYPEFLAG_FNONEXTENSIBLE)))
					ReportCompatOtherDiff("The interface '" + currentTypeName + "' has type flags that differ in a way that should not break compatibility.  This simply indicates that it's a class interface in one type library, but a real interface in the other type library.", oldAttr.wTypeFlags.ToString(), newAttr.wTypeFlags.ToString());
				else
					ReportCompatError("The type '" + currentTypeName + "' has different type flags.", oldAttr.wTypeFlags.ToString(), newAttr.wTypeFlags.ToString());
			}

			return true;
		}

		/// <summary>
		/// Ensures that the new TYPEDESC is compatible with the old TYPEDESC.
		/// </summary>
		/// <param name="oldDesc">TYPEDESC corresponding to the old TypeInfo</param>
		/// <param name="newDesc">TYPEDESC corresponding to the new TypeInfo</param>
		/// <param name="oldInfo">The old TypeInfo</param>
		/// <param name="newInfo">The new TypeInfo</param>
		/// <param name="memberName">The member name corresponding to the TYPEDESC, or an empty string</param>
		/// <param name="paramName">The parameter name corresponding to the TYPEDESC, or an empty string</param>
		/// <param name="currentMemberType">The type of the current member, if applicable.</param>
		private void CompareTypeDescs(TYPEDESC oldDesc, TYPEDESC newDesc, UCOMITypeInfo oldInfo, UCOMITypeInfo newInfo, string memberName, string paramName, string currentMemberType)
		{
			string messagePrefix = "";

			if (memberName != "" && paramName != "")
				messagePrefix = "Parameter '" + paramName + "' of " + currentMemberType + " '" + currentTypeName + "." + memberName + "' ";
			else if (memberName == "")
				messagePrefix = "Type '" + currentTypeName + "' ";
			else
				messagePrefix = "The " + currentMemberType + " '" + currentTypeName + "." + memberName + "' ";

			if (oldDesc.vt != newDesc.vt)
			{
				ReportCompatError(messagePrefix + " has a different kind of type.  Aborting further checking of TYPEDESC.", ((VarEnum)oldDesc.vt).ToString(), ((VarEnum)newDesc.vt).ToString());
				// Don't check the TYPEDESCs any further since their type differs.
				return;
			}

			// Check the TYPEDESC field's lpValue entry only if the VT type ==
			// VT_SAFEARRAY, VT_PTR, VT_CARRAY, or VT_USERDEFINED.

			// When VT_USERDEFINED, lpValue is an HREFTYPE that can be used to get a TypeInfo for the UDT
			if (oldDesc.vt == (short)VarEnum.VT_USERDEFINED)
			{
				UCOMITypeInfo oldUdtInfo = null;
				UCOMITypeInfo newUdtInfo = null;
				string oldName = null;
				string newName = null;
			
				try
				{
					oldInfo.GetRefTypeInfo((int)oldDesc.lpValue, out oldUdtInfo);
					oldName = Marshal.GetTypeInfoName(oldUdtInfo);

					newInfo.GetRefTypeInfo((int)newDesc.lpValue, out newUdtInfo);
					newName = Marshal.GetTypeInfoName(newUdtInfo);
				}
				catch (ArgumentException)
				{
					throw new ApplicationException(messagePrefix + " is VT_USERDEFINED, but its TypeInfo could not be extracted in one of the type libraries.");
				}

				// Just compare the names of the TypeInfos.  The rest should be taken care of
				// during the normal scan of all types.
				CompareNamesCaseSensitive(messagePrefix + "has a different VT_USERDEFINED type.", oldName, newName);
			}
				// When VT_SAFEARRAY or VT_PTR, lpValue is a pointer to a TYPEDESC that specifies the element type.
			else if (oldDesc.vt == (short)VarEnum.VT_SAFEARRAY || oldDesc.vt == (short)VarEnum.VT_PTR)
			{
				try
				{
					TYPEDESC tdescNew = (TYPEDESC)Marshal.PtrToStructure(newDesc.lpValue, typeof(TYPEDESC));
					TYPEDESC tdescOld = (TYPEDESC)Marshal.PtrToStructure(oldDesc.lpValue, typeof(TYPEDESC));

					if (tdescOld.vt != tdescNew.vt)
						ReportCompatError(messagePrefix + "has a different pointer or element type.", ((VarEnum)tdescOld.vt).ToString(), ((VarEnum)tdescNew.vt).ToString());
				}
				catch (NullReferenceException)
				{
					throw new ApplicationException(messagePrefix + "has a TYPEDESC.vt value of VT_PTR or VT_SAFEARRAY, but its TYPEDESC.lpValue value doesn't point to a valid TYPEDESC in one of the type libraries.");
				}
			}
				// When VT_CARRAY, lpValue is a pointer to an ARRAYDESC that describes the array.
			else if (oldDesc.vt == (short)VarEnum.VT_CARRAY)
			{
				try
				{
					ARRAYDESC adescNew = (ARRAYDESC)Marshal.PtrToStructure(newDesc.lpValue, typeof(ARRAYDESC));
					ARRAYDESC adescOld = (ARRAYDESC)Marshal.PtrToStructure(oldDesc.lpValue, typeof(ARRAYDESC));

					if (adescOld.cDims != adescNew.cDims)
						ReportCompatError(messagePrefix + "(a C-style array) has a different number of dimensions.", adescOld.cDims.ToString(), adescNew.cDims.ToString());

					// Recursively check the TYPEDESC embedded in this ARRAYDESC
					CompareTypeDescs(adescOld.tdescElem, adescNew.tdescElem, oldInfo, newInfo, memberName, paramName + "'s array element type", currentMemberType);
				}
				catch (NullReferenceException)
				{
					throw new ApplicationException(messagePrefix + "has a TYPEDESC.vt value of VT_CARRAY, but its TYPEDESC.lpValue value doesn't point to a valid ARRAYDESC in one of the type libraries.");
				}
			}
		}

		/// <summary>
		/// Ensures that the new VARDESC is compatible with the old VARDESC.
		/// </summary>
		/// <param name="oldDesc">VARDESC corresponding to the old TypeInfo</param>
		/// <param name="newDesc">VARDESC corresponding to the new TypeInfo</param>
		/// <param name="oldInfo">The old TypeInfo</param>
		/// <param name="newInfo">The new TypeInfo</param>
		/// <param name="fieldName">The name of the field corresponding to the VARDESC</param>
		private void CompareVarDescs(VARDESC oldDesc, VARDESC newDesc, UCOMITypeInfo oldInfo, UCOMITypeInfo newInfo, string fieldName)
		{
			// Compare IDs
			if (oldDesc.memid != newDesc.memid)
			{
				ReportCompatError("Field '" + fieldName + "' of '" + currentTypeName + "' has a different MEMBERID.", oldDesc.memid.ToString(), newDesc.memid.ToString());
			}

			CompareElemDescs(oldDesc.elemdescVar, newDesc.elemdescVar, oldInfo, newInfo, fieldName, "", "field");

			// Compare flags
			if (oldDesc.wVarFlags != newDesc.wVarFlags)
			{
				ReportCompatError("Field '" + fieldName + "' of '" + currentTypeName + "' has different flags.", ((VARFLAGS)oldDesc.wVarFlags).ToString(), ((VARFLAGS)newDesc.wVarFlags).ToString());
			}

			// Compare VARKINDs
			if (oldDesc.varkind != newDesc.varkind)
			{
				ReportCompatError("Field '" + fieldName + "' of '" + currentTypeName + "' has different VARKINDs.", oldDesc.varkind.ToString(), newDesc.varkind.ToString());
			}

			if (oldDesc.varkind == VARKIND.VAR_PERINSTANCE)
			{
				// The variable is a field or member of the type at a fixed offset within each instance of the type.
				if (oldDesc.descUnion.oInst != newDesc.descUnion.oInst)
				{
					ReportCompatError("Field '" + fieldName + "' in '" + currentTypeName + "' has a different offset.", oldDesc.descUnion.oInst.ToString(), newDesc.descUnion.oInst.ToString());
				}
			}
			else if (oldDesc.varkind == VARKIND.VAR_CONST)
			{
				object oldConst = Marshal.GetObjectForNativeVariant(oldDesc.descUnion.lpvarValue);
				object newConst = Marshal.GetObjectForNativeVariant(newDesc.descUnion.lpvarValue);
			
				if (oldConst.GetType() != newConst.GetType())
					ReportCompatError("Constant '" + fieldName + "' in '" + currentTypeName + "' has a different type.", oldConst.GetType().ToString(), newConst.GetType().ToString());

				if (oldConst.GetType() != typeof(Int32))
					ReportCompatError("Constant '" + fieldName + "' in '" + currentTypeName + "' has an unexpected type (in both type libraries): " + oldConst.GetType().ToString());

				if ((int)oldConst != (int)newConst)
					ReportCompatError("Constant '" + fieldName + "' in '" + currentTypeName + "' has a different value.", oldConst.ToString(), newConst.ToString());
			}
			else
			{
				throw new ApplicationException("Unexpected VARKIND for field '" + fieldName + "' in '" + currentTypeName + "': " + oldDesc.varkind);
			}
		}

		/// <summary>
		/// Ensures that the new ELEMDESC is compatible with the old ELEMDESC.
		/// </summary>
		/// <param name="oldDesc">ELEMDESC corresponding to the old TypeInfo</param>
		/// <param name="newDesc">ELEMDESC corresponding to the new TypeInfo</param>
		/// <param name="oldInfo">The old TypeInfo</param>
		/// <param name="newInfo">The new TypeInfo</param>
		/// <param name="memberName">The member name corresponding to the ELEMDESC, or an empty string</param>
		/// <param name="paramName">The parameter name corresponding to the ELEMDESC, or an empty string</param>
		/// <param name="currentMemberType">The type of the current member, if applicable.</param>
		private void CompareElemDescs(ELEMDESC oldDesc, ELEMDESC newDesc, UCOMITypeInfo oldInfo, UCOMITypeInfo newInfo, string memberName, string paramName, string currentMemberType)
		{
			// This comparison ignores ELEMDESC.desc.paramdesc.wParamFlags because it's the same bits as ELEMDESC.desc.idldesc.wIDLFlags

			if (oldDesc.desc.idldesc.wIDLFlags != newDesc.desc.idldesc.wIDLFlags)
			{
				if (paramName == "")
					ReportCompatError("The " + currentMemberType + " '" + currentTypeName + "." + memberName + "' has a different idldesc.wIDLFlags value.", oldDesc.desc.idldesc.wIDLFlags.ToString(), newDesc.desc.idldesc.wIDLFlags.ToString());
				else
					ReportCompatError("Parameter '" + paramName + "' of " + currentMemberType + " '" + currentTypeName + "." + memberName + "' has a different idldesc.wIDLFlags value.", oldDesc.desc.idldesc.wIDLFlags.ToString(), newDesc.desc.idldesc.wIDLFlags.ToString());
			}

			// If the PARAMFLAG_FHASDEFAULT flag is set, then the lpVarValue is valid,
			// and points to a PARAMDESCEX structure
			if ((oldDesc.desc.paramdesc.wParamFlags & PARAMFLAG.PARAMFLAG_FHASDEFAULT) > 0)
			{
				try
				{
					PARAMDESCEX pdescNew = (PARAMDESCEX)Marshal.PtrToStructure(newDesc.desc.paramdesc.lpVarValue, typeof(PARAMDESCEX));
					PARAMDESCEX pdescOld = (PARAMDESCEX)Marshal.PtrToStructure(oldDesc.desc.paramdesc.lpVarValue, typeof(PARAMDESCEX));

					int cByteOld = (int)Marshal.GetObjectForNativeVariant(pdescOld.cByte);
					int cByteNew = (int)Marshal.GetObjectForNativeVariant(pdescNew.cByte);

					if (cByteOld != cByteNew)
					{
						ReportCompatError("Parameter '" + paramName + "' of " + currentMemberType + " '" + currentTypeName + "." + memberName + "' has a different-sized PARAMDESCEX structure.", cByteOld.ToString(), cByteNew.ToString());
					}
					else
					{
						for (int i = 0; i < cByteNew; i++)
						{
							if (Marshal.ReadByte(pdescNew.varDefaultValue, i) != Marshal.ReadByte(pdescOld.varDefaultValue, i))
							{
								ReportCompatError("The default value for parameter '" + paramName + "' of " + currentMemberType + " '" + currentTypeName + "." + memberName + "' differs at byte #" + (i+1), Marshal.ReadByte(pdescOld.varDefaultValue, i).ToString(), Marshal.ReadByte(pdescNew.varDefaultValue, i).ToString());
							}
						}
					}
				}
				catch (NullReferenceException)
				{
					throw new ApplicationException("In one of the type libraries, parameter '" + paramName + "' of " + currentMemberType + " '" + currentTypeName + "." + memberName + "' has a PARAMFLAG_FHASDEFAULT set, but its paramdesc.lpVarValue value doesn't point to a valid PARAMDESCEX.");
				}
			}

			CompareTypeDescs(oldDesc.tdesc, newDesc.tdesc, oldInfo, newInfo, memberName, paramName, currentMemberType);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="oldDesc">FUNCDESC corresponding to the old TypeInfo</param>
		/// <param name="newDesc">FUNCDESC corresponding to the new TypeInfo</param>
		/// <param name="oldInfo">The old TypeInfo</param>
		/// <param name="newInfo">The new TypeInfo</param>
		/// <param name="funcName">The name of the function corresponding to the FUNCDESC</param>
		private void CompareFuncDescs(FUNCDESC oldDesc, FUNCDESC newDesc, UCOMITypeInfo oldInfo, UCOMITypeInfo newInfo, string funcName)
		{
			// This function ignores the lprgelemdescParam field, because it is examined
			// when looking at the function's parameters in the CompareFunctions method.

			// This function also ignores the lprgscode field, because it is not relevant.

			if (oldDesc.callconv != newDesc.callconv)
				ReportCompatError("Function '" + currentTypeName + "." + funcName + "' has a different calling convention.", oldDesc.callconv.ToString(), newDesc.callconv.ToString());

			if (oldDesc.cParams != newDesc.cParams)
				ReportCompatError("Function '" + currentTypeName + "." + funcName + "' has a different number of parameters.", oldDesc.cParams.ToString(), newDesc.cParams.ToString());

			if (oldDesc.cParamsOpt != newDesc.cParamsOpt)
				ReportCompatError("Function '" + currentTypeName + "." + funcName + "' has a different number of optional parameters.", oldDesc.cParamsOpt.ToString(), newDesc.cParamsOpt.ToString());

			if (oldDesc.cScodes != newDesc.cScodes)
				ReportCompatError("Function '" + currentTypeName + "." + funcName + "' has a different cScodes value.", oldDesc.cScodes.ToString(), newDesc.cScodes.ToString());

			// Compare the ELEMDESCs for the return type
			CompareElemDescs(oldDesc.elemdescFunc, newDesc.elemdescFunc, oldInfo, newInfo, funcName, "<return type>", "function");

			if (oldDesc.funckind != newDesc.funckind)
				ReportCompatError("Function '" + currentTypeName + "." + funcName + "' has a different funckind value.", oldDesc.funckind.ToString(), newDesc.funckind.ToString());

			if (oldDesc.invkind != newDesc.invkind)
				ReportCompatError("Function '" + currentTypeName + "." + funcName + "' has a different invkind value.", oldDesc.invkind.ToString(), newDesc.invkind.ToString());
	
			if (oldDesc.memid != newDesc.memid)
				ReportCompatError("Function '" + currentTypeName + "." + funcName + "' has a different memid value.", oldDesc.memid.ToString(), newDesc.memid.ToString());
		
			if (oldDesc.oVft != newDesc.oVft)
				ReportCompatError("Function '" + currentTypeName + "." + funcName + "' has a different v-table offset.", oldDesc.oVft.ToString(), newDesc.oVft.ToString());
		
			if (oldDesc.wFuncFlags != newDesc.wFuncFlags)
				ReportCompatError("Function '" + currentTypeName + "." + funcName + "' has a different wFuncFlags value.", oldDesc.wFuncFlags.ToString(), newDesc.wFuncFlags.ToString());
		}

		/// <summary>
		/// Ensures that a new coclass is compatible with the old coclass.
		/// </summary>
		/// <param name="oldTypeInfo">The TypeInfo for the old coclass</param>
		/// <param name="newTypeInfo">The TypeInfo for the new coclass</param>
		/// <param name="oldAttr">TYPEATTR corrsponding to the old TypeInfo</param>
		/// <param name="newAttr">TYPEATTR corrsponding to the new TypeInfo</param>
		private void CheckCoclass(UCOMITypeInfo oldTypeInfo, UCOMITypeInfo newTypeInfo, TYPEATTR oldAttr, TYPEATTR newAttr)
		{
			Hashtable newListedInterfaces = new Hashtable(6);

			// Store all the listed interfaces from the new coclass in newListedInterfaces
			for (int i = 0; i < newAttr.cImplTypes; i++)
			{
				// Get type information for the listed interface
				int href = 0;
				UCOMITypeInfo listedTypeInfo = null;
				newTypeInfo.GetRefTypeOfImplType(i, out href);
				newTypeInfo.GetRefTypeInfo(href, out listedTypeInfo);

				// Retrieve the implemented interface flags (source, default)
				int flags;
				newTypeInfo.GetImplTypeFlags(i, out flags);

				// Use its name as the hashtable key
				newListedInterfaces.Add(Marshal.GetTypeInfoName(listedTypeInfo), flags);
			}

			// Check the interfaces from the old coclass
			for (int i = 0; i < oldAttr.cImplTypes; i++)
			{
				// Get type information for the listed interface
				int href = 0;
				UCOMITypeInfo listedTypeInfo = null;
				oldTypeInfo.GetRefTypeOfImplType(i, out href);
				oldTypeInfo.GetRefTypeInfo(href, out listedTypeInfo);

				// Check that it's still listed in the new coclass
				if (!newListedInterfaces.Contains(Marshal.GetTypeInfoName(listedTypeInfo)))
				{
					ReportCompatError("The coclass '" + currentTypeName + "' no longer implements the interface '" + Marshal.GetTypeInfoName(listedTypeInfo) + "'.");
				}
				else
				{
					// Retrieve the implemented interface flags (source, default) and compare them
					int flags;
					oldTypeInfo.GetImplTypeFlags(i, out flags);

					if (flags != (int)newListedInterfaces[Marshal.GetTypeInfoName(listedTypeInfo)])
					{
						ReportCompatError("The coclass '" + currentTypeName + "' lists the interface '" + Marshal.GetTypeInfoName(listedTypeInfo) + 
							"' with different flags (such as [default] or [source]).", ((IMPLTYPEFLAGS)flags).ToString(), ((IMPLTYPEFLAGS)newListedInterfaces[Marshal.GetTypeInfoName(listedTypeInfo)]).ToString());
					}
				}
			}
		}

		/// <summary>
		/// Ensures that a new typedef (struct, enum, union, alias) is compatible with 
		/// the old typedef.
		/// </summary>
		/// <param name="oldTypeInfo">The TypeInfo for the old typedef</param>
		/// <param name="newTypeInfo">The TypeInfo for the new typedef</param>
		/// <param name="oldAttr">TYPEATTR corrsponding to the old TypeInfo</param>
		/// <param name="newAttr">TYPEATTR corrsponding to the new TypeInfo</param>
		private void CheckTypeDef(UCOMITypeInfo oldTypeInfo, UCOMITypeInfo newTypeInfo, TYPEATTR oldAttr, TYPEATTR newAttr)
		{
			for (int i = 0; i < oldAttr.cVars; i++)
			{
				CheckField(oldTypeInfo, newTypeInfo, oldAttr, newAttr, i);
			}
		}

		/// <summary>
		/// Ensures that a new interface is compatible with the old interface.
		/// </summary>
		/// <param name="oldTypeInfo">The TypeInfo for the old interface</param>
		/// <param name="newTypeInfo">The TypeInfo for the new interface</param>
		/// <param name="oldAttr">TYPEATTR corrsponding to the old TypeInfo</param>
		/// <param name="newAttr">TYPEATTR corrsponding to the new TypeInfo</param>
		private void CheckInterface(UCOMITypeInfo oldTypeInfo, UCOMITypeInfo newTypeInfo, TYPEATTR oldAttr, TYPEATTR newAttr)
		{
			if (oldAttr.cImplTypes != 1)
				throw new ApplicationException("Only expected cImplTypes==1 on interface '" + currentTypeName + "'.");

			// Check that interface is derived from the same interface
			UCOMITypeInfo typeInfo1 = null;
			UCOMITypeInfo typeInfo2 = null;

			int href = 0;
			oldTypeInfo.GetRefTypeOfImplType(0, out href);
			oldTypeInfo.GetRefTypeInfo(href, out typeInfo1);

			newTypeInfo.GetRefTypeOfImplType(0, out href);
			newTypeInfo.GetRefTypeInfo(href, out typeInfo2);

			string oldBaseName = Marshal.GetTypeInfoName(typeInfo1);
			string newBaseName = Marshal.GetTypeInfoName(typeInfo2);

			if (oldBaseName != newBaseName)
				ReportCompatError("Interface '" + currentTypeName + "' derives from a different interface.", oldBaseName, newBaseName);

			/* Note: Here we could treat dispinterfaces specially by allowing the order of their
			 * methods to change:
			 * 
			 * if (newAttr.typekind == TYPEKIND.TKIND_DISPATCH && (newAttr.wTypeFlags & TYPEFLAGS.TYPEFLAG_FDUAL) == 0)
			 * {
			 *     // Enforce that the same members are all there, but potentially in a different order.
			 * }
			 * else
			 * {
			 *     // Do what we currently do for all interfaces.
			 * }
			 */

			// Check if the number of functions/fields are equal, although don't bother reporting an error
			// because one was already reported when checking the TYPEATTRs
			int numFuncs, numFields;
			if (oldAttr.cFuncs != newAttr.cFuncs)
				numFuncs = Math.Min(oldAttr.cFuncs, newAttr.cFuncs);
			else
				numFuncs = oldAttr.cFuncs;

			if (oldAttr.cVars != newAttr.cVars)
				numFields = Math.Min(oldAttr.cVars, newAttr.cVars);
			else
				numFields = oldAttr.cVars;

			// Check that all functions are the same
			for (int i = 0; i < numFuncs; i++)
			{
				CheckMember(oldTypeInfo, newTypeInfo, oldAttr, newAttr, i);
			}

			// Check that all fields are the same (for dispinterface properties)
			for (int i = 0; i < numFields; i++)
			{
				CheckField(oldTypeInfo, newTypeInfo, oldAttr, newAttr, i);
			}
		}

		/// <summary>
		/// Ensures that a new field is compatible with the old field.
		/// </summary>
		/// <param name="oldTypeInfo">The TypeInfo for the old field</param>
		/// <param name="newTypeInfo">The TypeInfo for the new field</param>
		/// <param name="oldAttr">TYPEATTR corrsponding to the old TypeInfo</param>
		/// <param name="newAttr">TYPEATTR corrsponding to the new TypeInfo</param>
		/// <param name="fieldNumber">The number of the current field</param>
		private void CheckField(UCOMITypeInfo oldTypeInfo, UCOMITypeInfo newTypeInfo, TYPEATTR oldAttr, TYPEATTR newAttr, int fieldNumber)
		{
			IntPtr ptrOld = IntPtr.Zero;
			IntPtr ptrNew = IntPtr.Zero;
			VARDESC descOld, descNew;

			try
			{
				oldTypeInfo.GetVarDesc(fieldNumber, out ptrOld);
				descOld = (VARDESC)Marshal.PtrToStructure(ptrOld, typeof(VARDESC));

				newTypeInfo.GetVarDesc(fieldNumber, out ptrNew);
				descNew = (VARDESC)Marshal.PtrToStructure(ptrNew, typeof(VARDESC));

				// Check that the names match.
				string newName, oldName, doc, help;
				int helpID;
				newTypeInfo.GetDocumentation(descNew.memid, out newName, out doc, out helpID, out help);
				oldTypeInfo.GetDocumentation(descOld.memid, out oldName, out doc, out helpID, out help);

				if (!CompareNamesCaseSensitive("Field #" + fieldNumber + " of type '" + currentTypeName + "' has a different name.  Aborting check for any other differences with this field.", oldName, newName))
				{
					return;
				}

				// At this point, we know that the type names match.  Compare the VARDESCs.
				CompareVarDescs(descOld, descNew, oldTypeInfo, newTypeInfo, oldName);
			}
			finally
			{
				if (ptrOld != IntPtr.Zero) oldTypeInfo.ReleaseFuncDesc(ptrOld);
				if (ptrNew != IntPtr.Zero) newTypeInfo.ReleaseFuncDesc(ptrNew);
			}
		}

		/// <summary>
		/// Ensures that a new member (function or property accessor) is compatible 
		/// with the old member.
		/// </summary>
		/// <param name="oldTypeInfo">The TypeInfo for the old member</param>
		/// <param name="newTypeInfo">The TypeInfo for the new member</param>
		/// <param name="oldAttr">TYPEATTR corrsponding to the old TypeInfo</param>
		/// <param name="newAttr">TYPEATTR corrsponding to the new TypeInfo</param>
		/// <param name="funcNumber">The number of the current member</param>
		private void CheckMember(UCOMITypeInfo oldTypeInfo, UCOMITypeInfo newTypeInfo, TYPEATTR oldAttr, TYPEATTR newAttr, int funcNumber)
		{
			IntPtr ptrOld = IntPtr.Zero;
			IntPtr ptrNew = IntPtr.Zero;
			FUNCDESC descOld, descNew;
			ELEMDESC edescOld, edescNew;
			int numNewNames, numOldNames;
			string [] newNames = new String[MAX_NAMES];
			string [] oldNames = new String[MAX_NAMES];
			string currentMemberType = "";

			try
			{
				oldTypeInfo.GetFuncDesc(funcNumber, out ptrOld);
				descOld = (FUNCDESC)Marshal.PtrToStructure(ptrOld, typeof(FUNCDESC));

				newTypeInfo.GetFuncDesc(funcNumber, out ptrNew);
				descNew = (FUNCDESC)Marshal.PtrToStructure(ptrNew, typeof(FUNCDESC));

				// Get the names of the members and their parameters
				newTypeInfo.GetNames(descNew.memid, newNames, MAX_NAMES, out numNewNames);
				oldTypeInfo.GetNames(descOld.memid, oldNames, MAX_NAMES, out numOldNames);

				// Check that the member names match
				if (!CompareNamesCaseSensitive("Member #" + funcNumber + " of type '" + currentTypeName + "' has a different name.  Aborting check for any other differences with this member.", oldNames[0], newNames[0]))
				{
					return;
				}

				// Determine member type, just using the old one 
				if ((descOld.invkind & INVOKEKIND.INVOKE_PROPERTYGET) > 0)
				{
					currentMemberType = "property get accessor";
				}
				else if ((descOld.invkind & INVOKEKIND.INVOKE_PROPERTYPUT) > 0)
				{
					currentMemberType = "property put accessor";
				}
				else if ((descOld.invkind & INVOKEKIND.INVOKE_PROPERTYPUTREF) > 0)
				{
					currentMemberType = "property putref accessor";
				}
				else
				{
					currentMemberType = "function";
				}

				// At this point, we know that the names match.  Compare the FUNCDESCs.
				CompareFuncDescs(descOld, descNew, oldTypeInfo, newTypeInfo, oldNames[0]);

				// Compare parameters

				if (numNewNames != numOldNames)
				{
					ReportCompatError("Function '" + currentTypeName + "." + newNames[0] + "' has a different number of parameters.", (numOldNames-1).ToString(), (numNewNames-1).ToString());
				}

				for (int i = 1; i <= descOld.cParams; i++)
				{
					// Compare names (case-insensitive - COM only cares about parameter name casing for
					// IDispatchEx, but our default implementation is based on the managed casing anyway.
					// Someone could write an implementation of IReflect that depends on case in an 
					// exported type library, but that would be bogus.)
					if (String.Compare(oldNames[i], newNames[i], true) != 0)
					{
						ReportCompatError("Parameter #" + (i) + " of " + currentMemberType + " '" + currentTypeName + "." + newNames[0] + "' differs by more than just case.", oldNames[i], newNames[i]);
					}

					// Compare ELEMDESCs

					if (descNew.lprgelemdescParam != IntPtr.Zero && descOld.lprgelemdescParam != IntPtr.Zero)
					{
						// Note: Casting IntPtr to int should only be done on 32-bit platforms
						IntPtr elemPtrNew = new IntPtr((int)descNew.lprgelemdescParam + ((i-1) * Marshal.SizeOf(typeof(ELEMDESC))));
						edescNew = (ELEMDESC)Marshal.PtrToStructure(elemPtrNew, typeof(ELEMDESC));

						IntPtr elemPtrOld = new IntPtr((int)descOld.lprgelemdescParam + ((i-1) * Marshal.SizeOf(typeof(ELEMDESC))));
						edescOld = (ELEMDESC)Marshal.PtrToStructure(elemPtrOld, typeof(ELEMDESC));

						CompareElemDescs(edescOld, edescNew, oldTypeInfo, newTypeInfo, oldNames[0], oldNames[i], currentMemberType);
					}
					else if (descNew.lprgelemdescParam != IntPtr.Zero && descOld.lprgelemdescParam == IntPtr.Zero)
					{
						ReportCompatError("Parameter '" + oldNames[i] + "' of " + currentMemberType + " '" + currentTypeName + "." + oldNames[0] + "' has a null ELEMDESC in the old type library only.");
					}
					else if (descNew.lprgelemdescParam == IntPtr.Zero && descOld.lprgelemdescParam != IntPtr.Zero)
					{
						ReportCompatError("Parameter '" + oldNames[i] + "' of " + currentMemberType + " '" + currentTypeName + "." + oldNames[0] + "' has a null ELEMDESC in the new type library only.");
					}
				}
			}
			finally
			{
				if (ptrOld != IntPtr.Zero) oldTypeInfo.ReleaseFuncDesc(ptrOld);
				if (ptrNew != IntPtr.Zero) newTypeInfo.ReleaseFuncDesc(ptrNew);
			}
		}

		/// <summary>
		/// Does a case-sensitive comparison of two names.  If they differ,
		/// checks if they only differ by case.  If so, suggests the use of a 
		/// names file to fix the problem.
		/// Returns true if names are equal, false otherwise.
		/// </summary>
		/// <param name="messageIfDifferent">Message to print if names differ</param>
		/// <param name="oldName">Old name</param>
		/// <param name="newName">New name</param>
		private bool CompareNamesCaseSensitive(string messageIfDifferent, string oldName, string newName)
		{
			if (oldName != newName)
			{
				// We know that the names are different, but check if they only
				// differ by case.  If so, still report an error but suggest the
				// use of a type library exporter "names file"
				if (String.Compare(oldName, newName, true) != 0)
				{
					ReportCompatError(messageIfDifferent, oldName, newName);
				}
				else
				{
					ReportCompatError(messageIfDifferent + "  Since the name only differs by case, consider using a names file with the type library exporter to correct this.", oldName, newName);
				}
				return false;
			}
			return true;
		}

		/// <summary>
		/// Prints a compatibility error.
		/// </summary>
		/// <param name="message">The message to print</param>
		public void ReportCompatError(string message)
		{
			compatErrors.Add("Error: " + message);
		}

		/// <summary>
		/// Records and prints a compatibility error.
		/// </summary>
		/// <param name="message">The message to print</param>
		/// <param name="oldValue">The old value of the incompatible item</param>
		/// <param name="newValue">The new value of the incompatible item</param>
		private void ReportCompatError(string message, string oldValue, string newValue)
		{
//			compatErrors.Add("Error: " + message + Environment.NewLine + 
//				"           (Old: " + oldValue + ")" + Environment.NewLine +
//				"           (New: " + newValue + ")");
			compatErrors.Add(message + Environment.NewLine + 
				"<br>(Old: " + oldValue + ")" + Environment.NewLine +
				"<br>(New: " + newValue + ")");
		}

		/// <summary>
		/// Records and conditionally prints a compatibility warning.
		/// </summary>
		/// <param name="message">The message to print</param>
		/// <param name="oldValue">The old value of the potentially incompatible item</param>
		/// <param name="newValue">The new value of the potentially incompatible item</param>
		private void ReportCompatWarning(string message, string oldValue, string newValue)
		{
//			compatWarnings.Add("Warning: " + message + Environment.NewLine + 
//				"           (Old: " + oldValue + ")" + Environment.NewLine +
//				"           (New: " + newValue + ")");
			compatWarnings.Add(message + Environment.NewLine + 
				"<br>(Old: " + oldValue + ")" + Environment.NewLine +
				"<br>(New: " + newValue + ")");
		}

		/// <summary>
		/// Records and conditionally prints minor differences that should not
		/// affect compatibility.
		/// </summary>
		/// <param name="message">The message to print</param>
		/// <param name="oldValue">The old value of the differing item</param>
		/// <param name="newValue">The new value of the differing item</param>
		private void ReportCompatOtherDiff(string message, string oldValue, string newValue)
		{
//			compatOtherDiffs.Add("Other Diff: " + message + Environment.NewLine + 
//				"           (Old: " + oldValue + ")" + Environment.NewLine +
//				"           (New: " + newValue + ")");
			compatOtherDiffs.Add(message + Environment.NewLine + 
				"<br>(Old: " + oldValue + ")" + Environment.NewLine +
				"<br>(New: " + newValue + ")");
		}

		/// <summary>
		/// Prints a summary of types in both type libraries.
		/// </summary>
		public void PrintSummary()
		{
			Console.WriteLine("");
			Console.WriteLine("{0} error(s), {1} warning(s), {2} other difference(s)", compatErrors.Count, compatWarnings.Count, compatOtherDiffs.Count);
		}
	}
}
