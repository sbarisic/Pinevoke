using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pinevoke {
	static class Utils {
		public static T[] Sub<T>(this T[] Arr, int Len) {
			T[] Ret = new T[Len];
			Array.Copy(Arr, Ret, Len);
			return Ret;
		}
		
		public static T[] Append<T>(this T[] Arr, T[] Arr2) {
			T[] Ret = new T[Arr.Length + Arr2.Length];
			Array.Copy(Arr, Ret, Arr.Length);
			Array.Copy(Arr2, 0, Ret, Arr.Length, Arr2.Length);
			return Ret;
		}
	}
}
