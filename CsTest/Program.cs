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

			CppTest.Animal Chicken = new CppTest.Animal();
			Chicken.SetName("Chicken");

			CppTest.Animal Dog = new CppTest.Animal();
			Dog.SetName("Dog");

			CppTest.Farmer Farmer = new CppTest.Farmer();
			Farmer.SayAnimalName(Chicken);
			Farmer.SayAnimalName(Dog);

			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}