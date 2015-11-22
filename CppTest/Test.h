#pragma once

class EXPORT TestClass {
public:
	static int SizeOf() { return sizeof(TestClass); }
	// ^ Requirement, because nobody thought of actually
	// embedding the god damn class size in some metadata/whatever in the dll

	const char* Title;

	TestClass();
	~TestClass();

	void SetTitle(const char* Title);
	const char* GetTitle();
	void PrintTitle();
};

EXPORT const char* SomeString;
EXPORT const char* GetSomeString();
EXPORT int Add(int A, int* B);