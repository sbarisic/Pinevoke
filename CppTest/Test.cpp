#include "stdafx.h"
#include "Test.h"

Animal::Animal() {
}

void Animal::SetName(const char* Name) {
	this->Name = strcpy((char*)calloc(strlen(Name) + 1, sizeof(char)), Name);
}

const char* Animal::GetName() {
	return this->Name;
}

Farmer::Farmer() {
}

void Farmer::SayAnimalName(Animal* A) {
	printf("Animal name is: %s\n", A->GetName());
}

// ----

const char* GetSomeString() {
	return SomeString;
}

int Add(int A, int* B) {
	return A + *B;
}