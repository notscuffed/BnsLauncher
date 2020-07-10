#pragma once
#include "xmlreader.h"

#ifdef _M_X64
#define thiscall_(name, thisarg, ...) name(thisarg, ## __VA_ARGS__) 
#else
#include <cstdint>
#define thiscall_(name, thisarg, ...) __fastcall name(thisarg, intptr_t, ## __VA_ARGS__) 
#endif

extern v13::XmlDoc* (__thiscall* g_pfnReadFromFile13)(v13::XmlReader const*, const wchar_t*);
v13::XmlDoc* thiscall_(ReadFromFile13_hook, v13::XmlReader const* thisptr, const wchar_t* xmlFileNameForLogging);

extern v14::XmlDoc* (__thiscall* g_pfnReadFromFile14)(v14::XmlReader const*, const wchar_t*, class v14::XmlPieceReader*);
v14::XmlDoc* thiscall_(ReadFromFile14_hook, v14::XmlReader const* thisptr, const wchar_t* xmlFileNameForLogging, class v14::XmlPieceReader* xmlPieceReader);

extern v13::XmlDoc* (__thiscall* g_pfnReadFromBuffer13)(v13::XmlReader const*, unsigned char const*, unsigned int, const wchar_t*);
v13::XmlDoc* thiscall_(ReadFromBuffer13_hook, v13::XmlReader const* thisptr, unsigned char const* mem, unsigned int size, const wchar_t* file);

extern v14::XmlDoc* (__thiscall* g_pfnReadFromBuffer14)(v14::XmlReader const*, unsigned char const*, unsigned int, const wchar_t*, class v14::XmlPieceReader*);
v14::XmlDoc* thiscall_(ReadFromBuffer14_hook, v14::XmlReader const* thisptr, unsigned char const* mem, unsigned int size, const wchar_t* xmlFileNameForLogging, class v14::XmlPieceReader* xmlPieceReader);
