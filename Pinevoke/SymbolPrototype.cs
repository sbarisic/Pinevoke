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
		public string TypeString;
		public bool Unsigned;
		public int Pointer;
		public bool PrimitiveType;

		public CppType(bool Unsigned, int Pointer, string TypeString) {
			TypeString = NormalizeTypeName(TypeString, out PrimitiveType);

			if (TypeString == "char" && Unsigned) {
				Unsigned = false;
				TypeString = "byte";
			}
			
			this.Unsigned = Unsigned;
			this.Pointer = Pointer;
			this.TypeString = TypeString;
		}

		public override string ToString() {
			string Name = "";

			if (Unsigned)
				Name += "unsigned ";
			Name += TypeString;

			if (Pointer > 0)
				Name += " " + new string('*', Pointer);

			return Name;
		}

		static CSharpCodeProvider Compiler = new CSharpCodeProvider();
		static string NormalizeTypeName(string TypeName, out bool PrimitiveType) {
			if (TypeName == "short")
				TypeName = "Int16";
			else if (TypeName == "long")
				TypeName = "Int64";
			else if (TypeName == "int")
				TypeName = "Int32";
			else if (TypeName == "byte")
				TypeName = "Byte";

			try {
				// HAAAAAAAAAAAAAX
				int StartIdx = 0;
				if (TypeName.StartsWith("__"))
					StartIdx += 2;

				string TN = char.ToUpper(TypeName[StartIdx]) + TypeName.Substring(StartIdx + 1);
				string Ret = Compiler.GetTypeOutput(new CodeTypeReference(Type.GetType("System." + TN)));
				PrimitiveType = true;
				return Ret;
			} catch (Exception) {
			}

			PrimitiveType = false;
			return TypeName;
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

			TypeName = Tokens[StartIdx];
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

			string[] Tokens = Tokenizer.Tokenize(Demangled);
			Scope = SplitByScope(ParseName(Tokens));
			Name = Scope.LastOrDefault();
			CallingConvention = GetCallingConvention();

			if (Scope.Length > 1)
				ClassName = Scope[Scope.Length - 2];

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
