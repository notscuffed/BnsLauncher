#include "antigg.h"

bool fill_bytes(LPVOID address, size_t size, int value)
{
    DWORD oldProtection;

    if (!VirtualProtect(address, size, PAGE_EXECUTE_READWRITE, &oldProtection))
        return false;

    memset(address, value, size);

    VirtualProtect(address, size, oldProtection, &oldProtection);
    return true;
}

bool patch_gameguard()
{
    return fill_bytes((LPVOID)0x00C98108, 5, 0x90)
        && fill_bytes((LPVOID)0x005D5548, 5, 0x90);
}
