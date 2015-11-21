using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;

namespace Pinevoke {
	enum ID : int {
		Invalid = -1,
		None = 0,
		Type,
		CallingConvention,

		LParen,
		RParen,
		Comma,
	}

	static class Extensions {
		public static T[] Sub<T>(this T[] Arr, int Start, int Len) {
			T[] Ret = new T[Len];
			for (int i = 0; i < Len; i++)
				Ret[i] = Arr[Start + i];
			return Ret;
		}

		public static ID GetID(this Token T) {
			return (ID)T.Id;
		}

		public static bool IsID(this Token T, ID K) {
			return (T.Type == TokenType.Keyword || T.Type == TokenType.Symbol) && T.GetID() == K;
		}

		public static Type GetNetType(this Token[] Tokens) {
			return typeof(void);
		}
	}

	class Parser {
		LexerBehavior LB;
		LexerSettings LS;

		int Indent;
		StringBuilder CsCode;

		void Line(string Str = "") {
			if (Indent > 0)
				CsCode.Append(new string('\t', Indent));
			CsCode.AppendLine(Str);
		}

		void GenerateDllImport(string OriginalName, string Name, string ReturnType, string[] ParamTypes,
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

		void GenerateDllImport(string Name, string ReturnType, string[] ParamTypes,
			CallingConvention CConv = CallingConvention.Cdecl, bool SetLastError = true) {
			GenerateDllImport(Name, Name, ReturnType, ParamTypes, CConv, SetLastError);
		}

		void GenerateVarImport(string OriginalName, string Name, string Type) {
			Line("static IntPtr Internal" + Name + " = " + GenerateCall("kernel32", "GetProcAddress") + "(LibPtr, \"" + OriginalName + "\");");
			Line(string.Format("public static {0} {1} {{", Type, Name));
			Indent++;
			Line("get {");
			Indent++;
			Line(ReadType(Type, "Internal" + Name));
			Indent--;
			Line("}");
			Line("set {");
			Indent++;
			Line(WriteType(Type, "Internal" + Name));
			Indent--;
			Line("}");
			Indent--;
			Line("}");
			Line();
		}

		string GenerateCall(string Class, string Name) {
			string Ret = "";
			if (CurrentClassName != Class)
				Ret += Class + ".";
			Ret += Name;
			return Ret;
		}

		string CurrentClassName;
		void BeginImportClass(string DllName, CharSet CSet) {
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

		void EndImportClass() {
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

		public Parser(string DllName, CharSet CSet = CharSet.Ansi, string Namespace = "System") {
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

			LB = LexerBehavior.SkipComments | LexerBehavior.SkipWhiteSpaces | LexerBehavior.Default;
			LS = LexerSettings.Default;

			LS.Keywords = new Dictionary<string, int>();
			LS.Keywords.Add("unsigned", (int)ID.Type);
			LS.Keywords.Add("void", (int)ID.Type);
			LS.Keywords.Add("char", (int)ID.Type);
			LS.Keywords.Add("short", (int)ID.Type);
			LS.Keywords.Add("int", (int)ID.Type);
			LS.Keywords.Add("long", (int)ID.Type);
			LS.Keywords.Add("float", (int)ID.Type);
			LS.Keywords.Add("double", (int)ID.Type);

			LS.Keywords.Add("__cdecl", (int)ID.CallingConvention);

			LS.Keywords.Add("public:", (int)ID.Invalid);
			LS.Keywords.Add("const", (int)ID.Invalid);

			LS.Symbols = new Dictionary<string, int>();
			LS.Symbols.Add("*", (int)ID.Type);
			LS.Symbols.Add("&", (int)ID.Type);
			LS.Symbols.Add("(", (int)ID.LParen);
			LS.Symbols.Add(")", (int)ID.RParen);
			LS.Symbols.Add(",", (int)ID.Comma);
		}

		public void Parse(string Mangled, string Unmangled) {
			Console.WriteLine("{0} => {1}", Mangled, Unmangled);

			Token[] Tokens = ToTokens(Unmangled);

			if (Unmangled.EndsWith(")")) { // Function
				int ReturnTypeLen = 0;
				int NameIdx = 0;

				for (int i = 0; i < Tokens.Length; i++) {
					if (Tokens[i].IsID(ID.CallingConvention))
						ReturnTypeLen = i;
					if (Tokens[i].IsID(ID.LParen)) {
						NameIdx = i - 1;
						break;
					}
				}

				string ReturnType = ConvertType(string.Join(" ", (IEnumerable<Token>)Tokens.Sub(0, ReturnTypeLen)));
				string CConv = Tokens[ReturnTypeLen].Text;
				string Name = Tokens[NameIdx].Text;
				string[] ParamTypes = GetParams(Unmangled);

				GenerateDllImport(Mangled, Name, ReturnType, ParamTypes, ConvertCConv(CConv));

			} else { // Something else
				string VariableType = ConvertType(string.Join(" ", (IEnumerable<Token>)Tokens.Sub(0, Tokens.Length - 1)));
				string Name = Tokens[Tokens.Length - 1].Text;
				GenerateVarImport(Mangled, Name, VariableType);
			}
		}

		string ReadType(string Type, string InternalName) {
			if (Type == "string")
				return "return Marshal.PtrToStringAnsi(Marshal.ReadIntPtr(" + InternalName + "));";
			throw new NotImplementedException();
		}

		string WriteType(string Type, string InternalName) {
			if (Type == "string")
				return "Marshal.WriteIntPtr(" + InternalName + ", Marshal.StringToHGlobalAnsi(value));";
			throw new NotImplementedException();
		}

		string ConvertType(string CppType) {
			if (CppType == "char *")
				return "string";
			if (CppType.Contains('*'))
				return "IntPtr";
			return CppType;
		}

		CallingConvention ConvertCConv(string CConv) {
			switch (CConv) {
				case "__cdecl":
					return CallingConvention.Cdecl;
				default:
					throw new NotImplementedException();
			}
		}

		Token[] ToTokens(string Txt) {
			List<Token> Tokens = new List<Token>();
			Token[] RawTokens = new Lexer(Txt, LB, LS).ToArray();
			for (int i = 0; i < RawTokens.Length; i++)
				if (!RawTokens[i].IsID(ID.Invalid))
					Tokens.Add(RawTokens[i]);
			return Tokens.ToArray();
		}

		Token[] GetReturnType(string Unmangled) {
			return ToTokens(Unmangled.Substring(0, Unmangled.IndexOf('(')));
		}

		string[] GetParams(string Unmangled) {
			int FirstLParen = Unmangled.IndexOf('(');
			string[] ParamTypes = Unmangled.Substring(FirstLParen + 1, Unmangled.IndexOf(')') - FirstLParen - 1).Split(',');
			List<string> Params = new List<string>();
			for (int i = 0; i < ParamTypes.Length; i++)
				Params.Add(ConvertType(string.Join(" ", (IEnumerable<Token>)ToTokens(ParamTypes[i]))));
			return Params.ToArray();
		}
	}
}