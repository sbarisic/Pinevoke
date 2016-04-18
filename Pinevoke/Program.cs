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
		public static IntPtr CurProcess;
		static List<string> ExportedSymbols;

		[STAThread]
		static void Main(string[] Args) {
			Console.Title = "Pinevoke";
			CurProcess = Process.GetCurrentProcess().Handle;
			ExportedSymbols = new List<string>();

			if (Args.Length != 1 || !File.Exists(Args[0])) {
				Console.WriteLine("Usage:\n\tpinevoke library.dll");
				return;
			}

			if (!Dbghelp.SymInitialize(CurProcess))
				throw new Exception("Failed to SymInitialize");

			Dbghelp.SymSetOptions(0x4000); // Publics only
			Generate(Args[0]);
			Dbghelp.SymCleanup(CurProcess);

			if (Debugger.IsAttached)
				Console.ReadLine();
		}

		static void Generate(string DllPath) {
			string[] ExportStrings = CppHelper.GetExports(DllPath);
			SymbolPrototype[] Exports = ExportStrings.Select((S) => new SymbolPrototype(S)).ToArray();

			for (int i = 0; i < Exports.Length; i++)
				Console.WriteLine(Exports[i]);
		}
	}
}