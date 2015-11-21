using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace CsTest {
	unsafe class Program {
		static void Main(string[] args) {
			Console.Title = "C# Test";

			int i = 2;
			Console.WriteLine("Add: {0}", CppTest.Add(40, new IntPtr(&i)));
			CppTest.SomeString = "Hello World!";
			Console.WriteLine(CppTest.GetSomeString());

			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}