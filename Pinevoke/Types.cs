using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Pinevoke {
	static class Types {
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
			if (CppType == "char *")
				return "string";
			if (CppType.Contains('*'))
				return "IntPtr";
			return CppType;
		}

		public static CallingConvention ConvertCConv(string CConv) {
			switch (CConv) {
				case "__cdecl":
					return CallingConvention.Cdecl;
				default:
					throw new NotImplementedException();
			}
		}
	}
}