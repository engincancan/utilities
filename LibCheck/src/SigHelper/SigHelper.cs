using System;
using System.Reflection;

namespace SigHelper {

	public sealed class SigHelper {

		//** Fields
		internal static TTable CsTTable = new TTable(@"reffiles\Translations.txt");

		//** Methods
		public static void Load(string file) { CsTTable = new TTable(file); }

/*		public static string CsTranslate(string word) {
			string result = CsTTable.Translate(word);
			if (result.StartsWith("System.")) {
				string temp = result.Substring(7);
				if (temp.IndexOf(".") == -1)
					return temp;
			}
			return result;
		}
*/
		public static string CsTranslate(string word) {
			string temp = null;
			if (word.StartsWith("System.") && (temp = word.Substring(7)).IndexOf(".") == -1)
				return CsTTable.Translate(temp);
			else
				return CsTTable.Translate(word);
		}

		// Simplistic parser to translate generic style keywords into C# style syntax.
		public static string CsParse(string phrase) { return CsParse(phrase, false); }
		public static string CsParse(string phrase, bool trim) {
			string result = String.Empty;
			string bucket = String.Empty;
			bool inWord = false;
			for (int i = 0; i < phrase.Length; i++)
			{
				char c = phrase[i];
				if (Char.IsLetterOrDigit(c) || c == '.') {
					if (!inWord) {
						inWord = true;
						bucket = c.ToString();
					}
					else
						bucket += c;
				}
				else {
					if (inWord) {
						inWord = false;
						result += CsTranslate(bucket);
					}
					if (!trim || (c != '&' && c != '*'))
						result += c;
				}
			}
			if (inWord)
				result += CsTranslate(bucket);

			return result;
		}

		// Generic ToString handlers
		public static string ToGenString(MemberInfo member) {
			if (member is Type)
				return GenTypeInfo.ToString((Type)member);
			if (member is FieldInfo)
				return GenFieldInfo.ToString((FieldInfo)member);
			if (member is ConstructorInfo)
				return GenConstructorInfo.ToString((ConstructorInfo)member);
			if (member is MethodInfo)
				return GenMethodInfo.ToString((MethodInfo)member);
			if (member is PropertyInfo)
				return GenPropertyInfo.ToString((PropertyInfo)member);
			if (member is EventInfo)
				return GenEventInfo.ToString((EventInfo)member);

			return GenMemberInfo.ToString(member);
		}

		public static string ToGenString(ParameterInfo parameter) {
			return GenParameterInfo.ToString(parameter);
		}

		// C# ToString handlers
		public static string ToCsString(MemberInfo member) {
			if (member is Type)
				return CsTypeInfo.ToString((Type)member);
			if (member is FieldInfo)
				return CsFieldInfo.ToString((FieldInfo)member);
			if (member is ConstructorInfo)
				return CsConstructorInfo.ToString((ConstructorInfo)member);
			if (member is MethodInfo)
				return CsMethodInfo.ToString((MethodInfo)member);
			if (member is PropertyInfo)
				return CsPropertyInfo.ToString((PropertyInfo)member);
			if (member is EventInfo)
				return CsEventInfo.ToString((EventInfo)member);

			return GenMemberInfo.ToString(member);
		}

		public static string ToCsString(ParameterInfo parameter) {
			return CsParameterInfo.ToString(parameter);
		}

		// C# ToHtml handlers
		public static string ToCsHtml(MemberInfo member) {
			if (member is Type)
				return CsTypeInfo.ToHtml((Type)member);
			if (member is FieldInfo)
				return CsFieldInfo.ToHtml((FieldInfo)member);
			if (member is ConstructorInfo)
				return CsConstructorInfo.ToHtml((ConstructorInfo)member);
			if (member is MethodInfo)
				return CsMethodInfo.ToHtml((MethodInfo)member);
			if (member is PropertyInfo)
				return CsPropertyInfo.ToHtml((PropertyInfo)member);
			if (member is EventInfo)
				return CsEventInfo.ToHtml((EventInfo)member);

			return GenMemberInfo.ToHtml(member);
		}

		public static string ToCsHtml(ParameterInfo parameter) {
			return CsParameterInfo.ToHtml(parameter);
		}
	}
}