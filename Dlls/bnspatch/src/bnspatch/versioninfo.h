#pragma once
#include <phnt_windows.h>
#include <phnt.h>

EXTERN_C int GetModuleVersionInfo(HMODULE hModule, PCWSTR pwszSubBlock, LPCVOID* ppv);
