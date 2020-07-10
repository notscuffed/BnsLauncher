#pragma once

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif // !WIN32_LEAN_AND_MEAN
#include <Windows.h>
#include <vector>

struct ippatch {
    // All fields are stored in network order
    ULONG from_ip;
    ULONG to_ip;
    USHORT from_port;
    USHORT to_port;
};

extern std::vector<ippatch> g_ippatches;

// Ip and port in network order
void patch_ip(ULONG& ip, USHORT& port);
void load_iprewriter_patches();
