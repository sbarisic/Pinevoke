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
				{"double", "double"}
			};
		}

		public static string ReadType(string Type, string InternalName) {
			if (Type == "string")
				return "return Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(" + InternalName + "));";
			throw new NotImplementedException();
		}

		public static string WriteType(string Type, string InternalName) {
			if (Type == "string")
				return "Marshal.WriteIntPtr(" + InternalName + ", Marshal.StringToHGlobalAnsi(value));";
			throw new NotImplementedException();
		}

		public static string ConvertType(string CppType) {
			// Constructors and destructors
			if (CppType == "void" || CppType.Length == 0)
				return "void";

			if (TypeConversion.ContainsKey(CppType))
				return TypeConversion[CppType];
			if (CppType.Contains('*'))
				return "IntPtr";
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
	}
}