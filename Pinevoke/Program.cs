using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pinevoke {
	unsafe class Program {
		static IntPtr CurProcess;
		static List<string> ExportedSymbols;

		[STAThread]
		static void Main(string[] Args) {
			Console.Title = "Pinevoke";
			CurProcess = Process.GetCurrentProcess().Handle;
			ExportedSymbols = new List<string>();

			if (!Dbghelp.SymInitialize(CurProcess))
				throw new Exception("Failed to SymInitialize");
			Dbghelp.SymSetOptions(0x4000); // Publics only

			if (Args.Length == 1 && File.Exists(Args[0]))
				Generate(Args[0]);
			else
				Console.WriteLine("Usage:\n\tpinevoke.exe some.dll");

			Dbghelp.SymCleanup(CurProcess);
			Console.ReadLine();
		}

		static bool EnumSymbols(ref SYMBOL_INFO Symbol, uint Size, IntPtr Userdata) {
			string Name;
			fixed (byte* NamePtr = Symbol.Name)
				Name = Marshal.PtrToStringAnsi(new IntPtr(NamePtr));

			if ((Symbol.Flags & 0x200) != 0)
				ExportedSymbols.Add(Name);
			return true;
		}

		static string[] GetExports(string Dll) {
			if (!File.Exists(Dll))
				throw new FileNotFoundException("File not found", Dll);

			string PdbPath = Path.GetFileNameWithoutExtension(Dll) + ".pdb";
			if (File.Exists(PdbPath))
				//throw new Exception(PdbPath + " found, remove it");
				File.Delete(PdbPath);

			ulong DllBase = Dbghelp.SymLoadModuleEx(CurProcess, IntPtr.Zero, Dll, null, 0, 0, IntPtr.Zero, 0);
			if (DllBase == 0)
				//throw new Exception("Failed to load module " + Dll);
				throw new Win32Exception();

			ExportedSymbols.Clear();
			Dbghelp.SymEnumSymbols(CurProcess, DllBase, null, EnumSymbols, IntPtr.Zero);
			return ExportedSymbols.ToArray();
		}

		static string Demangle(string Name) {
			return Dbghelp.UnDecorateSymbolName(Name);
		}

		static void Generate(string DllPath) {
			string[] Exports = GetExports(DllPath);

			Parser P = new Parser();
			for (int i = 0; i < Exports.Length; i++) {
				string Mangled = Exports[i];
				string Unmangled = Demangle(Mangled);

				Console.WriteLine(Mangled);
				P.Parse(Unmangled);
			}
		}
	}
}