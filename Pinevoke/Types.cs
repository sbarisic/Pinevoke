using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Pinevoke {
	static class Types {
		static Dictionary<string, string> TypeConversion;

		static Types() {
			TypeConversion = new Dictionary<string, string>() {
				{"unsigned char", "byte"},
				{"short", "short"},
				{"unsigned short", "ushort"},
				{"int", "int"},
				{"unsigned int", "uint"},
				{"long", "int"},
				{"unsigned long", "uint"},
				{"char", "char"},
				{"wchar_t", "char"},
				{"char *", "string"},
				{"wchar_t *", "string"},
				{"float", "float"},
				{"double", "double"},

				// --
				{"_", "IntPtr"}
			};
		}

		public static string ReadType(string Type, string InternalName) {
			if (Type == "string")
				return "return " + WrapType("IntPtr", "string", "Marshal.ReadIntPtr(" + InternalName + ")") + ";";
			throw new NotImplementedException();
		}

		public static string WriteType(string Type, string InternalName) {
			if (Type == "string")
				return "Marshal.WriteIntPtr(" + InternalName + ", " + WrapType("string", "IntPtr", "value") + ");";
			throw new NotImplementedException();
		}

		public static string ConvertType(string CppType, bool Internal = false) {
			// Constructors and destructors
			if (CppType == "void" || CppType.Length == 0)
				return "void";

			if (TypeConversion.ContainsKey(CppType))
				return TypeConversion[CppType];
			if (TypeConversion.ContainsValue(CppType))
				return CppType;

			if (CppType.Contains('*') || CppType.Contains('&')) {
				if (Internal)
					return "IntPtr";
				return Parser.ToTokens(CppType)[1].Text;
			}

			throw new Exception("Unknown type " + CppType);
		}

		public static CallingConvention ConvertCConv(string CConv) {
			switch (CConv) {
				case "__cdecl":
					return CallingConvention.Cdecl;
				case "__thiscall":
					return CallingConvention.ThisCall;
				default:
					throw new NotImplementedException();
			}
		}

		public static string WrapType(string FromType, string ToType, string ParamName) {
			string Ret = ParamName;

			if (FromType == "IntPtr" && ToType == "string")
				Ret = "Marshal.PtrToStringAnsi";
			if (FromType == "string" && ToType == "IntPtr")
				Ret = "Marshal.StringToHGlobalAnsi";
			if ((FromType.Contains('&') || FromType.Contains('*')) && ToType != "string") 
				Ret = "(IntPtr)";

			if (Ret != ParamName && !string.IsNullOrEmpty(ParamName))
				Ret += "(" + ParamName + ")";
			return Ret;
		}

		public static string CustomMarshal(string ClassName, string TypeName, bool ReturnType, string Rest) {
			string Ret = "";
			string CustomMarshalFormat = "[";
			if (ReturnType)
				CustomMarshalFormat += "return: ";
			CustomMarshalFormat += "MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof({0}_{1}_Marshal))]";

			if (!string.IsNullOrEmpty(ClassName) && TypeName == "string")
				Ret = string.Format(CustomMarshalFormat, ClassName, TypeName);

			return Ret + Rest;
		}
	}
}