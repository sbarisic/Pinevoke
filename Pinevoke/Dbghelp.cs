using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Pinevoke {
	delegate bool EnumSymbolsProc(ref SYMBOL_INFO Symbol, uint SymbolSize, IntPtr UserContext);

	[StructLayout(LayoutKind.Sequential)]
	unsafe struct SYMBOL_INFO {
		public uint SizeOfStruct;
		public uint TypeIndex;
		public fixed ulong Reserved[2];
		public uint Index;
		public uint Size;
		public ulong ModBase;
		public uint Flags;
		public ulong Value;
		public ulong Address;
		public uint Register;
		public uint Scope;
		public uint Tag;
		public uint NameLen;
		public uint MaxNameLen;
		public fixed byte Name[1];
	}

	static class Dbghelp {
		const string DllName = "dbghelp";
		const CharSet CSet = CharSet.Ansi;

		[DllImport(DllName, SetLastError = true, CharSet = CSet)]
		public static extern uint SymSetOptions(uint Flags);

		[DllImport(DllName, SetLastError = true, CharSet = CSet)]
		public static extern bool SymInitialize(IntPtr hProcess, string UserSearchPath = null, bool fInvadeProcess = false);

		[DllImport(DllName, SetLastError = true, CharSet = CSet)]
		public static extern bool SymCleanup(IntPtr hProcess);

		[DllImport(DllName, SetLastError = true, CharSet = CSet)]
		public static extern ulong SymLoadModuleEx(IntPtr Proc, IntPtr File, string ImgName, string ModName, long Base,
			int DllSize, IntPtr Data, int Flags);

		[DllImport(DllName, SetLastError = true, CharSet = CSet)]
		public static extern bool SymEnumSymbols(IntPtr Proc, ulong BaseOfDll, string Mask, EnumSymbolsProc Callback, IntPtr Userdata);

		[DllImport(DllName, SetLastError = true, CharSet = CSet)]
		public static extern int UnDecorateSymbolName(string DecoratedName, StringBuilder UnDecoratedName,
			int UndecoratedLength, int Flags);

		public static string UnDecorateSymbolName(string DecoratedName, int Flags = 0) {
			StringBuilder Sb = new StringBuilder(2048);
			UnDecorateSymbolName(DecoratedName, Sb, Sb.Capacity, Flags);
			return Sb.ToString().Trim();
		}
	}
}