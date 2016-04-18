#include "stdafx.h"
#include "Test.h"

int __stdcall Add(int A, int* B) {
	return A + *B;
}