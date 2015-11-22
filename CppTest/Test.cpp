#include "stdafx.h"
#include "Test.h"

TestClass::TestClass() {
}

TestClass::~TestClass() {
}

void TestClass::SetTitle(const char* Title) {
	this->Title = Title;
}

const char* TestClass::GetTitle() {
	return this->Title;
}

void TestClass::PrintTitle() {
	printf("Title: %s\n", this->Title);
}

const char* GetSomeString() {
	return SomeString;
}

int Add(int A, int* B) {
	return A + *B;
}