using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinevoke {
	static class Tokenizer {
		static void PushToken(StringBuilder SB, List<string> Tokens) {
			if (SB.Length > 0) {
				Tokens.Add(SB.ToString());
				SB.Clear();
			}
		}

		public static string[] Tokenize(string Name) {
			List<string> Tokens = new List<string>();
			StringBuilder Temp = new StringBuilder();

			// Ghetto condense symbol string splittin'
			string[] Symbols = { "(", ")", "*", "&", "," };

			foreach (var S in Symbols)
				Name = Name.Replace(S, " " + S + " ");

			for (int i = 0; i < Name.Length; i++) {
				if (char.IsWhiteSpace(Name[i]))
					PushToken(Temp, Tokens);
				else
					Temp.Append(Name[i]);
			}

			PushToken(Temp, Tokens);

			// Fuck the const keyword and accessibility specifiers (whatever they're called), hopefully
			// nothing else ends with :
			return Tokens.Where((_) => !_.EndsWith(":") && _ != "const").ToArray();
		}
	}
}
