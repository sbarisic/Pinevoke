#include "stdafx.h"
#include "Test.h"

namespace A {
	namespace B {
		namespace C {
			EXPORT void string_test(char* Str, wchar_t* Str2) {
				printf("String: %s\n", Str);
				wprintf(L"WString: %s\n", Str2);
			}
		}
	}
	namespace D {
		EXPORT void test() {
			printf("Test called!\n");
		}

		namespace E {
			EXPORT void test2() {
				printf("Test2 called!\n");
			}
		}
	}
}