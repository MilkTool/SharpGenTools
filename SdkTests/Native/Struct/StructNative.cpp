#include "StructNative.h"
#include <cstdio>

STRUCTLIB_FUNC(SimpleStruct) GetSimpleStruct()
{
	return { 10, 3 };
}


STRUCTLIB_FUNC(StructWithArray) PassThroughArray(StructWithArray param)
{
	return param;
}


STRUCTLIB_FUNC(TestUnion) PassThroughUnion(TestUnion param)
{
	return param;
}

STRUCTLIB_FUNC(UnionWithArray) PassThroughUnion2(UnionWithArray param)
{
	return param;
}

STRUCTLIB_FUNC(BitField) PassThroughBitfield(BitField param)
{
	return param;
}

STRUCTLIB_FUNC(AsciiTest) PassThroughAscii(AsciiTest param)
{
	return param;
}

STRUCTLIB_FUNC(Utf16Test) PassThroughUtf(Utf16Test param)
{
	return param;
}

STRUCTLIB_FUNC(NestedTest) PassThroughNested(NestedTest param)
{
	return param;
}

STRUCTLIB_FUNC(bool) VerifyReservedBits(BitField2 param)
{
	return param.reservedBits == 20;
}
