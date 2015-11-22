using System;
using System.Runtime.InteropServices;

public static partial class kernel32 {
	const string __DllName = "kernel32";
	const CharSet __CSet = CharSet.Ansi;
	[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
	public static extern IntPtr LoadLibrary(string A);
	[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
	public static extern IntPtr GetProcAddress(IntPtr A, string B);
}

public static partial class CppTest {
	const string __DllName = "CppTest";
	const CharSet __CSet = CharSet.Ansi;
	static IntPtr __LibPtr = kernel32.LoadLibrary(__DllName);

	static IntPtr __InternalSomeString = kernel32.GetProcAddress(__LibPtr, "?SomeString@@3PBDB");
	public static string SomeString {
		get {
			return Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(__InternalSomeString));
		}
		set {
			Marshal.WriteIntPtr(__InternalSomeString, Marshal.StringToHGlobalAnsi(value));
		}
	}
	[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?GetSomeString@@YAPBDXZ", SetLastError = true)]
	public static extern string GetSomeString();
	[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?Add@@YAHHPAH@Z", SetLastError = true)]
	public static extern int Add(int A, IntPtr B);

	public class TestClass {
		const CharSet __CSet = CharSet.Ansi;
		static class Native {
			[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?SizeOf@TestClass@@SAHXZ", SetLastError = true)]
			public static extern int SizeOf();
			[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.ThisCall, EntryPoint = "?PrintTitle@TestClass@@QAEXXZ", SetLastError = true)]
			public static extern void PrintTitle(IntPtr __this);
			[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.ThisCall, EntryPoint = "?SetTitle@TestClass@@QAEXPBD@Z", SetLastError = true)]
			public static extern void SetTitle(IntPtr __this, string A);
			[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.ThisCall, EntryPoint = "?GetTitle@TestClass@@QAEPBDXZ", SetLastError = true)]
			public static extern string GetTitle(IntPtr __this);
			[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.ThisCall, EntryPoint = "??1TestClass@@QAE@XZ", SetLastError = true)]
			public static extern void __dtor(IntPtr __this);
			[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.ThisCall, EntryPoint = "??0TestClass@@QAE@XZ", SetLastError = true)]
			public static extern void __ctor(IntPtr __this);
		}

		IntPtr __this;
		public static int SizeOf() {
			return Native.SizeOf();
		}
		public void PrintTitle() {
			Native.PrintTitle(__this);
		}
		public void SetTitle(string A) {
			Native.SetTitle(__this, A);
		}
		public string GetTitle() {
			return Native.GetTitle(__this);
		}
		~TestClass() {
			Native.__dtor(__this);
			Marshal.FreeHGlobal(__this);
		}
		public TestClass() {
			__this = Marshal.AllocHGlobal(Native.SizeOf());
			Native.__ctor(__this);
		}
	}

}
