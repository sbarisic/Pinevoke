using System;
using System.Runtime.InteropServices;

using Animal_string_Marshal = StringMarshal;

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

	public class Animal {
		const CharSet __CSet = CharSet.Ansi;
		static class Native {
			[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.ThisCall, EntryPoint = "??0Animal@@QAE@XZ", SetLastError = true)]
			public static extern void __ctor(IntPtr __this);
			[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.ThisCall, EntryPoint = "?GetName@Animal@@QAEPBDXZ", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Animal_string_Marshal))]
			public static extern string GetName(IntPtr __this);
			[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?SizeOf@Animal@@SAHXZ", SetLastError = true)]
			public static extern int SizeOf();
			[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.ThisCall, EntryPoint = "?SetName@Animal@@QAEXPBD@Z", SetLastError = true)]
			public static extern void SetName(IntPtr __this, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(Animal_string_Marshal))]string A);
		}

		IntPtr __this;
		public static implicit operator IntPtr(Animal A) {
			return A.__this;
		}
		public Animal() {
			__this = Marshal.AllocHGlobal(Native.SizeOf());
			Native.__ctor(__this);
		}
		public string GetName() {
			return (Native.GetName(__this));
		}
		public static int SizeOf() {
			return (Native.SizeOf());
		}
		public void SetName(string A) {
			Native.SetName(__this, A);
		}
	}

	public class Farmer {
		const CharSet __CSet = CharSet.Ansi;
		static class Native {
			[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.ThisCall, EntryPoint = "?SayAnimalName@Farmer@@QAEXPAVAnimal@@@Z", SetLastError = true)]
			public static extern void SayAnimalName(IntPtr __this, IntPtr A);
			[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.ThisCall, EntryPoint = "??0Farmer@@QAE@XZ", SetLastError = true)]
			public static extern void __ctor(IntPtr __this);
			[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.Cdecl, EntryPoint = "?SizeOf@Farmer@@SAHXZ", SetLastError = true)]
			public static extern int SizeOf();
		}

		IntPtr __this;
		public static implicit operator IntPtr(Farmer A) {
			return A.__this;
		}
		public void SayAnimalName(Animal A) {
			Native.SayAnimalName(__this, (IntPtr)(A));
		}
		public Farmer() {
			__this = Marshal.AllocHGlobal(Native.SizeOf());
			Native.__ctor(__this);
		}
		public static int SizeOf() {
			return (Native.SizeOf());
		}
	}

}
