#pragma once

class EXPORT Animal {
public:
	static int SizeOf() { return sizeof(Animal); }
	Animal();

	const char* Name;

	void SetName(const char* Name);
	const char* GetName();
};

class EXPORT Farmer {
public:
	static int SizeOf() { return sizeof(Farmer); }
	Farmer();

	void SayAnimalName(Animal& A);
	//void SayAnimalName(Animal* A);
};

EXPORT const char* SomeString;
EXPORT const char* GetSomeString();
EXPORT int Add(int A, int* B);