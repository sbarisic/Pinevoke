using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinevoke {
	enum Keyword : int {
		Unsigned,
		Const,
		Void,
		Char,
		Short,
		Int,
		Long,
		Float,
		Double,

		__cdecl,
		__thiscall,

		_Public,
	}

	enum Symbols : int {
		LParen,
		RParen,
		Comma,
		Ptr,
		Ref
	}

	class Parser {
		LexerBehavior LB;
		LexerSettings LS;

		public Parser() {
			LB = LexerBehavior.SkipComments | LexerBehavior.SkipWhiteSpaces | LexerBehavior.Default;
			LS = LexerSettings.Default;

			LS.Keywords = new Dictionary<string, int>();
			string[] KeywordNames = Enum.GetNames(typeof(Keyword));
			for (int i = 0; i < KeywordNames.Length; i++)
				LS.Keywords.Add(KeywordNames[i].ToLower(), (int)(Keyword)Enum.Parse(typeof(Keyword), KeywordNames[i]));

			LS.Keywords.Add("public:", (int)Keyword._Public);

			LS.Symbols = new Dictionary<string, int>();
			LS.Symbols.Add("(", (int)Symbols.LParen);
			LS.Symbols.Add(")", (int)Symbols.RParen);
			LS.Symbols.Add(",", (int)Symbols.Comma);
			LS.Symbols.Add("*", (int)Symbols.Ptr);
			LS.Symbols.Add("&", (int)Symbols.Ref);
		}

		public void Parse(string Mangled, string Unmangled) {
			Console.WriteLine("{0} => {1}", Mangled, Unmangled);

			Token[] Tokens = new Lexer(Unmangled, LB, LS).ToArray();
			for (int i = 0; i < Tokens.Length; i++)
				Console.WriteLine(Tokens[i]);
			Console.WriteLine();
		}
	}
}