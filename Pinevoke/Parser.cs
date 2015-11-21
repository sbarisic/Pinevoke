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

			LS.Keywords.Add("public:", (int)ID.Invalid);
			LS.Keywords.Add("const", (int)ID.Invalid);

			LS.Symbols = new Dictionary<string, int>();
			LS.Symbols.Add("*", (int)ID.Type);
			LS.Symbols.Add("&", (int)ID.Type);
			LS.Symbols.Add("(", (int)ID.LParen);
			LS.Symbols.Add(")", (int)ID.RParen);
			LS.Symbols.Add(",", (int)ID.Comma);
		}

		public void Parse(string Mangled, string Unmangled, Generator Gen) {
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

				string ReturnType = Types.ConvertType(string.Join(" ", (IEnumerable<Token>)Tokens.Sub(0, ReturnTypeLen)));
				string CConv = Tokens[ReturnTypeLen].Text;
				string Name = Tokens[NameIdx].Text;
				string[] ParamTypes = GetParams(Unmangled);

				Gen.GenerateDllImport(Mangled, Name, ReturnType, ParamTypes, Types.ConvertCConv(CConv));

			} else { // Something else
				string VariableType = Types.ConvertType(string.Join(" ", (IEnumerable<Token>)Tokens.Sub(0, Tokens.Length - 1)));
				string Name = Tokens[Tokens.Length - 1].Text;
				Gen.GenerateVarImport(Mangled, Name, VariableType);
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
				Params.Add(Types.ConvertType(string.Join(" ", (IEnumerable<Token>)ToTokens(ParamTypes[i]))));
			return Params.ToArray();
		}
	}
}