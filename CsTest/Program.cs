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

			CppTest.SomeString = "dcfgvbhj";
			Console.WriteLine("Some String: {0}", CppTest.GetSomeString());


			CppTest.TestClass Test = new CppTest.TestClass();
			Test.SetInt(42);
			Test.PrintInt();
			Console.WriteLine("Int from C#: {0}", Test.GetInt());

			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}