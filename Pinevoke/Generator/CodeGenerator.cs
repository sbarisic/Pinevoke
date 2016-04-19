using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Pinevoke {
	public class KVTree<K, V> : Dictionary<K, KVTree<K, V>> {
		public HashSet<V> Leaves;

		public KVTree() : base() {
			Leaves = new HashSet<V>();
		}

		public bool Contains(params K[] Keys) {
			KVTree<K, V> RootNode = this;

			for (int i = 0; i < Keys.Length; i++) {
				if (!RootNode.Contains(Keys))
					return false;
				RootNode = RootNode[Keys[i]];
			}
			return true;
		}

		public KVTree<K, V> Get(params K[] Keys) {
			KVTree<K, V> RootNode = this;
			for (int i = 0; i < Keys.Length; i++) {
				if (!RootNode.ContainsKey(Keys[i]))
					RootNode.Add(Keys[i], new KVTree<K, V>());
				RootNode = RootNode[Keys[i]];
			}
			return RootNode;
		}

		public bool ContainsLeaves() {
			return Leaves.Count > 0;
		}

		public void ForEachLeaf(Action<V, Tuple<K, KVTree<K, V>>[]> OnLeaf, Tuple<K, KVTree<K, V>>[] Root) {
			foreach (var L in Leaves)
				OnLeaf(L, Root);

			foreach (var K in Keys)
				this[K].ForEachLeaf(OnLeaf, Root.Append(new[] { new Tuple<K, KVTree<K, V>>(K, this[K]) }));
		}

		public void ForEachLeaf(Action<V, Tuple<K, KVTree<K, V>>[]> OnLeaf) {
			ForEachLeaf(OnLeaf, new Tuple<K, KVTree<K, V>>[] { });
		}
	}

	class CodeGenerator {
		IEnumerable<SymbolPrototype> Exports;
		KVTree<string, SymbolPrototype> ExportsTree;
		string LibName;

		public CodeGenerator(IEnumerable<SymbolPrototype> Exports, string LibName) {
			ExportsTree = new KVTree<string, SymbolPrototype>();
			this.Exports = Exports;
			this.LibName = LibName;
		}

		public string Generate() {
			foreach (var E in Exports) {
				string[] ClassPath = { LibName };
				ClassPath = ClassPath.Append(E.Scope);
				if (E.ClassName != null)
					ClassPath = ClassPath.Append(new[] { E.ClassName });

				var Leaf = ExportsTree.Get(ClassPath);
				Leaf.Leaves.Add(E);
			}

			ExportsTree.ForEachLeaf((Exp, KV) => {
				for (int i = 0; i < KV.Length; i++) {
					if (KV[i].Item2.ContainsLeaves())
						Console.ForegroundColor = ConsoleColor.Green;
					else
						Console.ForegroundColor = ConsoleColor.White;
					Console.Write("{0}", KV[i].Item1);
					Console.ResetColor();
					Console.Write(".");
				}

				Console.ForegroundColor = ConsoleColor.Blue;
				Console.WriteLine(Exp.Name);
			});

			return "";
		}
	}
}