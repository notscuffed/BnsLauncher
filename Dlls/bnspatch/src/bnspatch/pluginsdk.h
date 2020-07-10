#pragma once
#include <Windows.h>

struct PluginInfo
{
    PCWSTR Name;
    PCWSTR Version;
    PCWSTR Description;
    VOID(__cdecl* Init)(VOID);
};

struct DetoursData
{
    LONG(WINAPI* TransactionBegin)(VOID);
    LONG(WINAPI* TransactionAbort)(VOID);
    LONG(WINAPI* TransactionCommit)(VOID);
    LONG(WINAPI* UpdateThread)(HANDLE hThread);
    LONG(WINAPI* Attach)(PVOID* ppPointer, PVOID pDetour);
    LONG(WINAPI* Attach2)(HMODULE hModule, PCSTR pProcName, PVOID* pPointer, PVOID pDetour);
    LONG(WINAPI* Detach)(PVOID* ppPointer, PVOID pDetour);
};

struct InitNotificationData
{
    const DetoursData* Detours;
};

struct DllNotificationData
{
    ULONG Flags;
    PCWSTR FullName;
    SIZE_T FullNameLength;
    PCWSTR Name;
    SIZE_T NameLength;
    HINSTANCE BaseOfImage;
    ULONG SizeOfImage;
    const DetoursData* Detours;
};

struct PluginInfo2
{
    PCWSTR Name;
    PCWSTR Version;
    PCWSTR Description;
    VOID(__cdecl* InitNotification)(const struct InitNotificationData*, PVOID);
    VOID(__cdecl* DllLoadedNotification)(const struct DllNotificationData*, PVOID);
    VOID(__cdecl* DllUnloadedNotification)(const struct DllNotificationData*, PVOID);
    PVOID Context;
};

typedef void(__cdecl* GetPluginInfoFn)(PluginInfo*);
typedef void(__cdecl* GetPluginInfo2Fn)(PluginInfo2*);
