using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.IO;

namespace Pinevoke {
	class Generator {
		int Indent;
		StringBuilder CsCode;

		public Generator(string DllName, CharSet CSet = CharSet.Ansi, string Namespace = "System") {
			CsCode = new StringBuilder();
			Line("using System;");
			Line("using System.Runtime.InteropServices;");
			Line();
			Line("namespace " + Namespace + " {");
			Indent++;

			BeginImportClass("kernel32", CharSet.Ansi);
			GenerateDllImport("LoadLibrary", "IntPtr", new string[] { "string" });
			GenerateDllImport("GetProcAddress", "IntPtr", new string[] { "IntPtr", "string" });
			EndImportClass();

			BeginImportClass(DllName, CSet);
		}

		public void Line(string Str = "") {
			if (Indent > 0)
				CsCode.Append(new string('\t', Indent));
			CsCode.AppendLine(Str);
		}

		public void GenerateDllImport(string OriginalName, string Name, string ReturnType, string[] ParamTypes,
		   CallingConvention CConv = CallingConvention.Cdecl, bool SetLastError = true) {
			string EntryPoint = "";
			if (OriginalName != Name)
				EntryPoint = " EntryPoint = \"" + OriginalName + "\",";

			Line(string.Format("[DllImport(DllName, CharSet = CSet, CallingConvention = CallingConvention.{0},{1} SetLastError = {2})]",
				CConv, EntryPoint, SetLastError.ToString().ToLower()));

			List<string> Params = new List<string>();
			for (int i = 0; i < ParamTypes.Length; i++)
				if (ParamTypes[i] != "void")
					Params.Add(ParamTypes[i] + " " + ((char)('A' + i)));
			Line(string.Format("public static extern {0} {1}({2});", ReturnType, Name, string.Join(", ", Params)));
			Line();
		}

		public void GenerateDllImport(string Name, string ReturnType, string[] ParamTypes,
			CallingConvention CConv = CallingConvention.Cdecl, bool SetLastError = true) {
			GenerateDllImport(Name, Name, ReturnType, ParamTypes, CConv, SetLastError);
		}

		public void GenerateVarImport(string OriginalName, string Name, string Type) {
			Line("static IntPtr Internal" + Name + " = " + GenerateCall("kernel32", "GetProcAddress") + "(LibPtr, \"" + OriginalName + "\");");
			Line(string.Format("public static {0} {1} {{", Type, Name));
			Indent++;
			Line("get {");
			Indent++;
			Line(Types.ReadType(Type, "Internal" + Name));
			Indent--;
			Line("}");
			Line("set {");
			Indent++;
			Line(Types.WriteType(Type, "Internal" + Name));
			Indent--;
			Line("}");
			Indent--;
			Line("}");
			Line();
		}

		public string GenerateCall(string Class, string Name) {
			string Ret = "";
			if (CurrentClassName != Class)
				Ret += Class + ".";
			Ret += Name;
			return Ret;
		}

		string CurrentClassName;
		public void BeginImportClass(string DllName, CharSet CSet) {
			DllName = Path.GetFileNameWithoutExtension(DllName);
			Line("static partial class " + DllName + " {");
			CurrentClassName = DllName;
			Indent++;
			Line("const string DllName = \"" + Path.GetFileNameWithoutExtension(DllName) + "\";");
			Line("const CharSet CSet = CharSet." + CSet + ";");
			Line("static IntPtr LibPtr = " + GenerateCall("kernel32", "LoadLibrary") + "(DllName);");
			Line();

			/*Line("static " + DllName + "() {");
			Indent++;
			Line(string.Format("LibPtr = {0}(DllName);", GenerateCall("kernel32", "LoadLibrary")));
			Indent--;
			Line("}");
			Line();*/
		}

		public void EndImportClass() {
			CurrentClassName = "";
			Indent--;
			Line("}");
			Line();
		}

		public string Finalize() {
			EndImportClass();
			Indent--;
			Line("}");
			return CsCode.ToString();
		}
	}
}