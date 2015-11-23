using System;
using System.Runtime.InteropServices;

class Animal_string_Marshal : ICustomMarshaler {
	public void CleanUpManagedData(object ManagedObj) {
	}

	public void CleanUpNativeData(IntPtr NativeData) {
		Marshal.FreeHGlobal(NativeData);
	}

	public int GetNativeDataSize() {
		return -1;
	}

	public IntPtr MarshalManagedToNative(object ManagedObj) {
		return Marshal.StringToHGlobalAnsi(ManagedObj.ToString());
	}

	public object MarshalNativeToManaged(IntPtr NativeData) {
		return Marshal.PtrToStringAnsi(NativeData);
	}

	internal static Animal_string_Marshal Singleton;
	public static ICustomMarshaler GetInstance(string Cookie) {
		if (Singleton == null)
			Singleton = new Animal_string_Marshal();
		return Singleton;
	}
}