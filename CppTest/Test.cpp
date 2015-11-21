#include "stdafx.h"
#include "Test.h"

EXPORT const char* SomeString;

const char* GetSomeString() {
	return SomeString;
}

int Add(int A, int* B) {
	return A + *B;
}