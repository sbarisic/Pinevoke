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
		NamespaceSeparator,
		Destructor,
		Class,
		Struct,
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
		static LexerBehavior LB;
		static LexerSettings LS;

		public Parser() {
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
			LS.Keywords.Add("__stdcall", (int)ID.CallingConvention);
			LS.Keywords.Add("__fastcall", (int)ID.CallingConvention);
			LS.Keywords.Add("__thiscall", (int)ID.CallingConvention);
			LS.Keywords.Add("__vectorcall", (int)ID.CallingConvention);

			LS.Keywords.Add("class", (int)ID.Class);
			LS.Keywords.Add("struct", (int)ID.Struct);

			LS.Keywords.Add("public", (int)ID.Invalid);
			LS.Keywords.Add("const", (int)ID.Invalid);

			LS.Symbols = new Dictionary<string, int>();
			LS.Symbols.Add("*", (int)ID.Type);
			LS.Symbols.Add("&", (int)ID.Type);
			LS.Symbols.Add("(", (int)ID.LParen);
			LS.Symbols.Add(")", (int)ID.RParen);
			LS.Symbols.Add(",", (int)ID.Comma);
			LS.Symbols.Add("~", (int)ID.Destructor);
			LS.Symbols.Add("::", (int)ID.NamespaceSeparator);

			LS.Symbols.Add(":", (int)ID.Invalid);
		}

		public void Parse(string Mangled, string Unmangled, Generator Gen) {
			if (Unmangled.Contains("::operator=") || Unmangled.Contains("__autoclassinit"))
				return;

			Console.WriteLine("{0}\n{1}\n", Mangled, Unmangled);
			Token[] Tokens = ToTokens(Unmangled);

			if (Unmangled.StartsWith("public:")) { // Class member
				bool IsStatic = Tokens[0].Text == "static";
				if (IsStatic)
					Tokens = Tokens.Sub(1, Tokens.Length - 1);

				int ReturnTypeLen = 0;
				int NameIdx = 0;
				int ClassNameIdx = 0;
				bool Destructor = false;

				for (int i = 0; i < Tokens.Length; i++) {
					if (Tokens[i].IsID(ID.CallingConvention))
						ReturnTypeLen = i;
					if (Tokens[i].IsID(ID.NamespaceSeparator))
						ClassNameIdx = i - 1;
					if (Tokens[i].IsID(ID.LParen))
						NameIdx = i - 1;
					if (Tokens[i].IsID(ID.Destructor))
						Destructor = true;
				}

				string ReturnType = Types.ConvertType(string.Join(" ", (IEnumerable<Token>)Tokens.Sub(0, ReturnTypeLen)));
				string CConv = Tokens[ReturnTypeLen].Text;
				string ClassName = Tokens[ClassNameIdx].Text;
				string Name = Tokens[NameIdx].Text;
				if (Name == ClassName && !Destructor)
					Name = Function.Constructor;
				else if (Destructor)
					Name = Function.Destructor;
				string[] OriginalParamTypes;
				string[] ParamTypes = GetParams(Unmangled, out OriginalParamTypes);
				Gen.Add(ClassName, new Function(Mangled, Name, ReturnType, ParamTypes, OriginalParamTypes, Types.ConvertCConv(CConv)));
			} else if (Unmangled.EndsWith(")")) { // Function
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

				string ReturnType = Types.ConvertType(string.Join(" ", (IEnumerable<Token>)Tokens.Sub(0, ReturnTypeLen)));
				string CConv = Tokens[ReturnTypeLen].Text;
				string Name = Tokens[NameIdx].Text;
				string[] OriginalParamTypes;
				string[] ParamTypes = GetParams(Unmangled, out OriginalParamTypes);

				Gen.Add(new Function(Mangled, Name, ReturnType, ParamTypes, OriginalParamTypes, Types.ConvertCConv(CConv)));

			} else { // Something else
				string VariableType = Types.ConvertType(string.Join(" ", (IEnumerable<Token>)Tokens.Sub(0, Tokens.Length - 1)));
				string Name = Tokens[Tokens.Length - 1].Text;

				Gen.Add(new Variable(Mangled, Name, VariableType));
			}
		}

		public static Token[] ToTokens(string Txt) {
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

		string[] GetParams(string Unmangled, out string[] Original) {
			int FirstLParen = Unmangled.IndexOf('(');
			string[] ParamTypes = Unmangled.Substring(FirstLParen + 1, Unmangled.IndexOf(')') - FirstLParen - 1).Split(',');
			List<string> Params = new List<string>();
			List<string> OriginalParams = new List<string>();
			for (int i = 0; i < ParamTypes.Length; i++) {
				string TypeName = string.Join(" ", (IEnumerable<Token>)ToTokens(ParamTypes[i]));
				Params.Add(Types.ConvertType(TypeName));
				OriginalParams.Add(TypeName);
			}
			Original = OriginalParams.ToArray();
			return Params.ToArray();
		}
	}
}