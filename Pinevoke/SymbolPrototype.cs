using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Microsoft.CSharp;
using System.CodeDom;
using System.ComponentModel;

namespace Pinevoke {
	class CppType {
		static Dictionary<string, Tuple<string, string>> PrimitiveTypes = new Dictionary<string, Tuple<string, string>>() {
			{ "void", new Tuple<string,string>("Void", "Void") },
			{ "char", new Tuple<string,string>("Char", "Byte") },
			{ "bool", new Tuple<string,string>("Boolean", "Boolean") },
			{ "short", new Tuple<string,string>("Int16", "UInt16") },
			{ "int", new Tuple<string,string>("Int32", "UInt32") },
			{ "long", new Tuple<string,string>("Int32", "UInt32") },
			{ "long long", new Tuple<string,string>("Int64", "UInt64") },
			{ "__int64", new Tuple<string,string>("Int64", "UInt64") },
			{ "wchar_t", new Tuple<string,string>("Char", "UInt16") },
			{ "float", new Tuple<string,string>("Single", "Single") },
			{ "double", new Tuple<string,string>("Double", "Double") },
			{ "long double", new Tuple<string,string>("Double", "Double") },
		};

		public string TypeString;
		public int Pointer;
		public bool PrimitiveType;
		public UnmanagedType? MarshalAsUnmanagedType;

		public CppType(bool Unsigned, int Pointer, string TypeString) {
			PrimitiveType = false;
			MarshalAsUnmanagedType = null;

			if (PrimitiveTypes.ContainsKey(TypeString)) {
				PrimitiveType = true;

				if (!Unsigned) {
					if (TypeString == "wchar_t") {
						if (Pointer > 0) {
							MarshalAsUnmanagedType = UnmanagedType.LPWStr;
						} else {
							MarshalAsUnmanagedType = UnmanagedType.I2;
						}
					} else if (TypeString == "char") {
						if (Pointer > 0) {
							MarshalAsUnmanagedType = UnmanagedType.LPStr;
						} else {
							MarshalAsUnmanagedType = UnmanagedType.I1;
						}
					}
				}

				Tuple<string, string> TS = PrimitiveTypes[TypeString];
				if (Unsigned)
					TypeString = TS.Item2;
				else
					TypeString = TS.Item1;
			}

			TypeString = TypeString.Replace("::", ".");

			this.Pointer = Pointer;
			this.TypeString = TypeString;
		}

		public override string ToString() {
			string Name = "";
			if (MarshalAsUnmanagedType != null)
				Name += "[" + MarshalAsUnmanagedType + "] ";

			Name += TypeString;
			if (Pointer > 0)
				Name += " " + new string('*', Pointer);
			return Name;
		}

		public static CppType ParseType(string[] Tokens, int StartIdx) {
			bool Unsigned = false;
			int Pointer = 0;

			string StartTok = Tokens[StartIdx].ToLower();
			string TypeName = "";

			if (StartTok == "unsigned") {
				Unsigned = true;
				StartIdx++;
			} else if (StartTok == "signed") {
				Unsigned = false;
				StartIdx++;
			} else if (StartTok == "class" || StartTok == "struct") {
				StartIdx++;
			}

			if (Tokens[StartIdx] == "long") {
				string Tok2 = Tokens[StartIdx + 1];
				if (!(Tok2 == "," || Tok2 == ")")) {
					StartIdx++;
					TypeName += "long ";
				}
			}

			TypeName += Tokens[StartIdx];
			string T = "";
			while ((T = Tokens[++StartIdx]).Length > 0 && (T == "*" || T == "&"))
				Pointer++;

			return new CppType(Unsigned, Pointer, TypeName);
		}
	}

	enum SymbolType {
		Function, Operator, Constructor, Destructor
	}

	class SymbolPrototype {
		public string Demangled;
		public SymbolType SymbolType;
		public CallingConvention CallingConvention;

		public CppType ReturnType;
		public CppType[] ParamTypes;
		public string Name, ClassName;
		public string[] Scope;

		public SymbolPrototype(string MangledName) {
			Demangled = CppHelper.Demangle(MangledName);
			if (Demangled == MangledName)
				throw new Exception("Could not demangle " + MangledName);

			string[] Tokens = Tokenizer.Tokenize(Demangled);
			Scope = SplitByScope(ParseName(Tokens));
			Name = Scope.LastOrDefault();
			Scope = Scope.Sub(Scope.Length - 1);
			CallingConvention = GetCallingConvention();

			if (Scope.Length > 0 && CallingConvention == CallingConvention.ThisCall)
				ClassName = Scope[Scope.Length - 1];
			else
				ClassName = null;

			if (Demangled.Contains("operator"))
				SymbolType = SymbolType.Operator;
			else if (CallingConvention == CallingConvention.ThisCall && ClassName == Name)
				SymbolType = SymbolType.Constructor;
			else if (CallingConvention == CallingConvention.ThisCall && ("~" + ClassName) == Name)
				SymbolType = SymbolType.Destructor;
			else
				SymbolType = SymbolType.Function;

			if (SymbolType == SymbolType.Operator || SymbolType == SymbolType.Function)
				ReturnType = CppType.ParseType(Tokens, 0);
			else
				ReturnType = new CppType(false, 0, "void");

			ParamTypes = ParseParamTypes(Tokens);
		}

		public override string ToString() {
			return string.Format("{0} {1} {2}({3})", ReturnType, CallingConvention, Name,
				string.Join(", ", ParamTypes.Select((_) => _.ToString()))) + "\n" + Demangled + "\n";
		}

		CallingConvention GetCallingConvention() {
			if (Demangled.Contains("__cdecl"))
				return CallingConvention.Cdecl;
			else if (Demangled.Contains("__stdcall"))
				return CallingConvention.StdCall;
			else if (Demangled.Contains("__fastcall"))
				return CallingConvention.FastCall;
			else if (Demangled.Contains("__thiscall"))
				return CallingConvention.ThisCall;

			throw new Exception("Unknown calling convention in function " + ToString());
		}

		string ParseName(string[] Tokens) {
			int LeftParen = 1;
			for (int i = 0; i < Tokens.Length; i++)
				if (Tokens[i] == "(") {
					LeftParen = i;
					break;
				}

			return Tokens[LeftParen - 1];
		}

		string[] SplitByScope(string Name) {
			return Name.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
		}

		CppType[] ParseParamTypes(string[] Tokens) {
			List<CppType> Types = new List<CppType>();

			for (int i = 0; i < Tokens.Length; i++)
				if (Tokens[i] == "(" || Tokens[i] == ",")
					Types.Add(CppType.ParseType(Tokens, i + 1));

			return Types.ToArray();
		}
	}
}
