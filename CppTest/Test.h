#pragma once

#define TEST_FNC void __stdcall Fnc(__int8 A, __int16 B, __int32 C, \
	__int64 D, const char E, unsigned char* F, signed long long G, unsigned long long*** H)

EXPORT TEST_FNC;

class EXPORT TestClass {
public:
	TestClass() {
	}

	~TestClass() {
	}

	void Crap() {
	}
};