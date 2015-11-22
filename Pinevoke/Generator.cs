using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace Pinevoke {
	class Generatable {
		public virtual string Generate() {
			throw new NotImplementedException();
		}
	}

	class Function : Generatable {
		public const string Constructor = "__ctor";
		public const string Destructor = "__dtor";
		public const string This = "__this";

		public string OriginalName;
		public string Name;
		public string ReturnType;
		public string[] ParamTypes;
		public CallingConvention CConv;
		public bool SetLastError;
		public string Modifiers;
		public bool FwdToNative;
		public string ClassName;

		public Function(string OriginalName, string Name, string ReturnType, string[] ParamTypes,
			CallingConvention CConv = CallingConvention.Cdecl, bool SetLastError = true, string Modifiers = "static extern") {
			this.OriginalName = OriginalName;
			this.Name = Name;
			this.ReturnType = ReturnType;
			this.ParamTypes = ParamTypes;
			this.CConv = CConv;
			this.SetLastError = SetLastError;
			this.Modifiers = Modifiers;
		}

		public Function(string Name, string ReturnType, string[] ParamTypes,
			CallingConvention CConv = CallingConvention.Cdecl, bool SetLastError = true)
			: this(Name, Name, ReturnType, ParamTypes, CConv, SetLastError) {
		}

		public override string Generate() {
			StringBuilder SB = new StringBuilder();
			string EntryPoint = "";
			if (OriginalName != Name)
				EntryPoint = " EntryPoint = \"" + OriginalName + "\",";

			if (FwdToNative) {
				if (CConv == CallingConvention.ThisCall)
					Modifiers = "";
				else
					Modifiers = "static";
			}

			if (!FwdToNative)
				SB.AppendFormat("[DllImport(__DllName, CharSet = __CSet, CallingConvention = CallingConvention.{0},{1} SetLastError = {2})]\n",
					CConv, EntryPoint, SetLastError.ToString().ToLower());

			string Mods = "";
			if (Modifiers.Length > 0)
				Mods += " " + Modifiers;

			List<string> Params = new List<string>();
			int ParamCount = 0;
			if (!FwdToNative && CConv == CallingConvention.ThisCall)
				Params.Add("IntPtr " + This);
			for (int i = 0; i < ParamTypes.Length; i++)
				if (ParamTypes[i] != "void") {
					ParamCount++;
					Params.Add(ParamTypes[i] + " " + ((char)('A' + i)));
				}

			if (FwdToNative && Name == Constructor)
				SB.AppendFormat("public {0}()", ClassName);
			else if (FwdToNative && Name == Destructor)
				SB.AppendFormat("~{0}()", ClassName);
			else
				SB.AppendFormat("public{3} {0} {1}({2})", ReturnType, Name, string.Join(", ", Params), Mods);

			if (FwdToNative) {
				SB.Append(" {\n\t");
				if (ReturnType != "void")
					SB.Append("return ");

				string ThisPtr = "";
				if (!Modifiers.Contains("static")) {
					ThisPtr = This;
					if (ParamCount > 0)
						ThisPtr += ", ";
				}

				Params.Clear();
				for (int i = 0; i < ParamCount; i++)
					Params.Add(((char)('A' + i)).ToString());

				if (Name == Constructor)
					SB.AppendFormat("{0} = Marshal.AllocHGlobal(Native.SizeOf());\n\t", This);
				SB.AppendFormat("Native.{0}({1}{2});\n", Name, ThisPtr, string.Join(", ", Params));
				if (Name == Destructor)
					SB.AppendFormat("\tMarshal.FreeHGlobal({0});\n", This);
				SB.Append("}");
			} else
				SB.Append(";");

			return SB.ToString();
		}
	}

	class Variable : Generatable {
		public string OriginalName;
		public string Name;
		public string Type;

		public Variable(string OriginalName, string Name, string Type) {
			this.OriginalName = OriginalName;
			this.Name = Name;
			this.Type = Type;
		}

		public override string Generate() {
			StringBuilder SB = new StringBuilder();
			SB.AppendLine("static IntPtr __Internal" + Name + " = kernel32.GetProcAddress(__LibPtr, \"" + OriginalName + "\");");
			SB.AppendFormat("public static {0} {1} {{\n", Type, Name);
			SB.AppendLine("\tget { " + Types.ReadType(Type, "__Internal" + Name) + " }");
			SB.AppendLine("\tset { " + Types.WriteType(Type, "__Internal" + Name) + " }");
			SB.Append("}");
			return SB.ToString();
		}
	}

	class Class : Generatable {
		public string Name;
		public CharSet CSet;
		List<Function> Functions;

		public Class(string Name, CharSet CSet) {
			this.CSet = CSet;
			this.Name = Name;
			Functions = new List<Function>();
		}

		public void Add(Function F) {
			Functions.Add(F);
		}

		public override string Generate() {
			StringBuilder SB = new StringBuilder();
			SB.AppendFormat("public class {0} {{\n", Name);
			SB.AppendFormat("\tconst CharSet __CSet = CharSet.{0};\n", CSet);
			SB.AppendLine("\tstatic class Native {");
			for (int i = 0; i < Functions.Count; i++)
				SB.AppendLine(Functions[i].Generate());
			SB.AppendLine("\t}");

			SB.AppendFormat("\n\tIntPtr __this;\n", Name);

			for (int i = 0; i < Functions.Count; i++) {
				Functions[i].FwdToNative = true;
				Functions[i].ClassName = Name;
				SB.AppendLine(Functions[i].Generate());
			}

			SB.AppendLine("}");
			return SB.ToString();
		}
	}

	class Generator {
		int Indent;
		StringBuilder CsCode;
		string DllName;
		CharSet CSet;

		List<Function> Functions;
		List<Variable> Variables;
		List<Class> Classes;

		public Generator(string DllName, CharSet CSet = CharSet.Ansi) {
			CsCode = new StringBuilder();
			Functions = new List<Function>();
			Variables = new List<Variable>();
			Classes = new List<Class>();
			this.DllName = Path.GetFileNameWithoutExtension(DllName);
			this.CSet = CSet;
		}

		public void Add(string ClassName, Function F) {
			GetOrCreate(ClassName).Add(F);
		}

		public void Add(Function F) {
			Functions.Add(F);
		}

		public void Add(Variable V) {
			Variables.Add(V);
		}

		public string Finalize() {
			Append("using System;");
			Append("using System.Runtime.InteropServices;");
			Append();

			BeginStaticClass("kernel32");
			Append(new Function("LoadLibrary", "IntPtr", new[] { "string" }, CallingConvention.Winapi).Generate());
			Append(new Function("GetProcAddress", "IntPtr", new[] { "IntPtr", "string" }, CallingConvention.Winapi).Generate());
			EndStaticClass();
			Append();

			BeginStaticClass(DllName, CSet);
			Append();

			foreach (var V in Variables)
				Append(V.Generate());
			foreach (var F in Functions)
				Append(F.Generate());

			Append();
			foreach (var C in Classes)
				Append(C.Generate());

			EndStaticClass();
			return CsCode.ToString();
		}

		Class GetOrCreate(string Name) {
			for (int i = 0; i < Classes.Count; i++)
				if (Classes[i].Name == Name)
					return Classes[i];
			Class C = new Class(Name, CSet);
			Classes.Add(C);
			return C;
		}

		void Line(string Str = "") {
			Str = Str.Replace("\r", "");
			if (Indent > 0)
				CsCode.Append(new string('\t', Indent));
			CsCode.AppendLine(Str);
		}

		void Append(string Str = "") {
			string[] Lines = Str.Split('\n');
			for (int i = 0; i < Lines.Length; i++)
				Line(Lines[i]);
		}

		void BeginStaticClass(string Name, CharSet CSet = CharSet.Ansi) {
			Append("public static partial class " + Name + " {");
			Indent++;
			Append("const string __DllName = \"" + Name + "\";");
			Append("const CharSet __CSet = CharSet." + CSet + ";");
			if (Name != "kernel32")
				Append("static IntPtr __LibPtr = kernel32.LoadLibrary(__DllName);");
		}

		void EndStaticClass() {
			Indent--;
			Append("}");
		}
	}
}