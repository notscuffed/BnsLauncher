#pragma once

#include <phnt_windows.h>

bool fill_bytes(LPVOID address, size_t size, int value);
bool patch_gameguard();
