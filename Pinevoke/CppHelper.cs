using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Pinevoke {
	public static unsafe class CppHelper {
		static List<string> ExportedSymbols;

		static CppHelper() {
			ExportedSymbols = new List<string>();
		}

		static bool EnumSymbols(ref SYMBOL_INFO Symbol, uint Size, IntPtr Userdata) {
			string Name;
			fixed (byte* NamePtr = Symbol.Name)
				Name = Marshal.PtrToStringAnsi(new IntPtr(NamePtr));

			if ((Symbol.Flags & 0x200) != 0)
				ExportedSymbols.Add(Name);
			return true;
		}

		public static string[] GetExports(string Dll) {
			Console.WriteLine("Enumerating exports for " + Dll);

			if (!File.Exists(Dll))
				throw new FileNotFoundException("File not found", Dll);

			string PdbPath = Path.GetFileNameWithoutExtension(Dll) + ".pdb";
			if (File.Exists(PdbPath)) {
#if DEBUG
				File.Delete(PdbPath);
#else
			throw new Exception(PdbPath + " found, remove it");
#endif
			}

			ulong DllBase = Dbghelp.SymLoadModuleEx(Program.CurProcess, IntPtr.Zero, Dll, null, 0, 0, IntPtr.Zero, 0);
			if (DllBase == 0)
				throw new Win32Exception();

			ExportedSymbols.Clear();
			Dbghelp.SymEnumSymbols(Program.CurProcess, DllBase, null, EnumSymbols, IntPtr.Zero);
			return ExportedSymbols.ToArray();
		}

		public static string Demangle(string Name) {
			return Dbghelp.UnDecorateSymbolName(Name);
		}
	}
}
