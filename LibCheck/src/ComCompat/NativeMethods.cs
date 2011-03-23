using System;
using System.Runtime.InteropServices;
using System.Security;

/// <summary>
/// PInvoke methods.
/// </summary>
namespace ComComparer 
{
	internal class NativeMethods
	{
		[DllImport("oleaut32.dll", CharSet=CharSet.Unicode, PreserveSig=false)]
		internal static extern UCOMITypeLib LoadTypeLibEx(string szFile, REGKIND regkind);
	}

	// REGKIND enumeration used by LoadTypeLibEx
	internal enum REGKIND
	{
		REGKIND_DEFAULT = 0,
		REGKIND_REGISTER = 1,
		REGKIND_NONE = 2
	}

	// Various type information structures

	internal struct VARDESC
	{
		public int memid;
		public IntPtr lpstrSchema;
		public DESCUNION descUnion;
		public ELEMDESC elemdescVar;
		public short wVarFlags;
		public VARKIND varkind;
	}

	internal struct ARRAYDESC
	{
		internal TYPEDESC tdescElem;
		internal ushort cDims;
		internal IntPtr rgbounds;
	}

	internal struct SAFEARRAYBOUND 
	{
		internal ulong cElements;
		internal long lLbound;
	}

	internal struct PARAMDESCEX 
	{
		internal IntPtr cByte;
		[MarshalAs(UnmanagedType.Struct)] internal object varDefaultValue;
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct DESCUNION
	{
		[FieldOffset(0)] public int oInst;
		[FieldOffset(0)] public IntPtr lpvarValue;
	}
	internal enum VARKIND
	{
		VAR_PERINSTANCE,
		VAR_STATIC,
		VAR_CONST,
		VAR_DISPATCH
	}

	class DummyClass
	{
		private void WarningSilencer()
		{
			// Use "unused" fields to silence warnings.
			// The fields are really used by unmanaged code

			VARDESC v = new VARDESC();
			v.elemdescVar = new ELEMDESC();
			v.lpstrSchema = IntPtr.Zero;
			v.memid = 0;
			v.varkind = 0;
			v.wVarFlags = 0;
			v.descUnion = new DESCUNION();

			ARRAYDESC a = new ARRAYDESC();
			a.cDims = 0;
			a.rgbounds = IntPtr.Zero;
			a.tdescElem = new TYPEDESC();

			SAFEARRAYBOUND sb = new SAFEARRAYBOUND();
			sb.cElements = 0;
			sb.lLbound = 0;

			PARAMDESCEX p = new PARAMDESCEX();
			p.cByte = IntPtr.Zero;
			p.varDefaultValue = null;
		}
	}
}