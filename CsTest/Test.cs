using System;
using System.Runtime.InteropServices;

namespace System {
	static partial class kernel32 {
		const string DllName = "kernel32";
		const CharSet CSet = CharSet.Ansi;
		static IntPtr LibPtr = LoadLibrary(DllName);
		
		[DllImport(DllName, CharSet = CSet, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern IntPtr LoadLibrary(string A);
		
		[DllImport(DllName, CharSet = CSet, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
		public static extern IntPtr GetProcAddress(IntPtr A, string B);
		
	}
	
	static partial class CppTest {
		const string DllName = "CppTest";
		const CharSet CSet = CharSet.Ansi;
		static IntPtr LibPtr = kernel32.LoadLibrary(DllName);
		
		[DllImport(DllName, CharSet = CSet, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetSomeString@@YAPBDXZ", SetLastError = true)]
		public static extern string GetSomeString();
		
		[DllImport(DllName, CharSet = CSet, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?Add@@YAHHPAH@Z", SetLastError = true)]
		public static extern int Add(int A, IntPtr B);
		
		static IntPtr InternalSomeString = kernel32.GetProcAddress(LibPtr, "?SomeString@@3PBDB");
		public static string SomeString {
			get {
				return Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(InternalSomeString));
			}
			set {
				Marshal.WriteIntPtr(InternalSomeString, Marshal.StringToHGlobalAnsi(value));
			}
		}
		
	}
	
}
