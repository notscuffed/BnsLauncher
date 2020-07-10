#include <phnt_windows.h>
#include <phnt.h>

#include <fnv1a.h>
#include <pe/export_directory.h>
#include <pe/module.h>
#include <string>
#include <wil/stl.h>
#include <wil/win32_helpers.h>

#include "hooks.h"
#include "pluginsdk.h"
#include "versioninfo.h" 
#include "xmlreader_hooks.h"
#include "logging.h"
#include "iprewriter.h"

const DetoursData* g_DetoursData;

BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD fdwReason, LPVOID lpvReserved)
{
    if (fdwReason == DLL_PROCESS_ATTACH)
        DisableThreadLibraryCalls(hInstance);
    return TRUE;
}

BOOL(WINAPI* g_pfnDllEntryPoint)(HINSTANCE, DWORD, LPVOID);
BOOL WINAPI DllEntryPoint_hook(HINSTANCE hInstance, DWORD fdwReason, LPVOID lpvReserved)
{
    const auto res = g_pfnDllEntryPoint(hInstance, fdwReason, lpvReserved);
    if (res && fdwReason == DLL_PROCESS_ATTACH) {
        g_DetoursData->TransactionBegin();
        g_DetoursData->UpdateThread(NtCurrentThread());

        const auto GetInterfaceVersion = reinterpret_cast<const wchar_t* (__cdecl*)(void)>(GetProcAddress(hInstance, "GetInterfaceVersion"));
        const auto CreateXmlReader = reinterpret_cast<void* (__cdecl*)()>(GetProcAddress(hInstance, "CreateXmlReader"));
        const auto DestroyXmlReader = reinterpret_cast<void(__cdecl*)(void*)>(GetProcAddress(hInstance, "DestroyXmlReader"));

        if (GetInterfaceVersion && CreateXmlReader && DestroyXmlReader) {
            auto xmlReader = std::unique_ptr<void, decltype(DestroyXmlReader)>(CreateXmlReader(), DestroyXmlReader);
            auto vfptr = *reinterpret_cast<void***>(xmlReader.get());
            switch (fnv1a::make_hash(GetInterfaceVersion())) {
            case L"13"_fnv1a:
                g_pfnReadFromFile13 = reinterpret_cast<decltype(g_pfnReadFromFile13)>(vfptr[6]);
                g_DetoursData->Attach(&(PVOID&)g_pfnReadFromFile13, ReadFromFile13_hook);
                g_pfnReadFromBuffer13 = reinterpret_cast<decltype(g_pfnReadFromBuffer13)>(vfptr[7]);
                g_DetoursData->Attach(&(PVOID&)g_pfnReadFromBuffer13, ReadFromBuffer13_hook);
                break;

            case L"14"_fnv1a:
            case L"15"_fnv1a:
                g_pfnReadFromFile14 = reinterpret_cast<decltype(g_pfnReadFromFile14)>(vfptr[6]);
                g_DetoursData->Attach(&(PVOID&)g_pfnReadFromFile14, ReadFromFile14_hook);
                g_pfnReadFromBuffer14 = reinterpret_cast<decltype(g_pfnReadFromBuffer14)>(vfptr[7]);
                g_DetoursData->Attach(&(PVOID&)g_pfnReadFromBuffer14, ReadFromBuffer14_hook);
                break;
            }
        }

        g_DetoursData->Detach(&(PVOID&)g_pfnDllEntryPoint, &DllEntryPoint_hook);
        g_DetoursData->TransactionCommit();
    }
    return res;
}

void __cdecl DllLoadedNotification(const struct DllNotificationData* Data, void* Context)
{
    const wchar_t* OriginalFilename;
    if (GetModuleVersionInfo(Data->BaseOfImage, L"\\StringFileInfo\\*\\OriginalFilename", &(LPCVOID&)OriginalFilename) >= 0) {
        switch (fnv1a::make_hash(OriginalFilename, fnv1a::ascii_toupper)) {
        case L"XmlReader.dll"_fnv1au: {
            const auto module = static_cast<pe::module*>(Data->BaseOfImage);
            g_DetoursData->TransactionBegin();
            g_DetoursData->UpdateThread(NtCurrentThread());
            g_pfnDllEntryPoint = reinterpret_cast<decltype(g_pfnDllEntryPoint)>(module->rva_to(module->nt_header()->OptionalHeader.AddressOfEntryPoint));
            g_DetoursData->Attach(&(PVOID&)g_pfnDllEntryPoint, &DllEntryPoint_hook);
            g_DetoursData->TransactionCommit();
            break;
        }
        }
    }
}

void __cdecl InitNotification(const struct InitNotificationData* Data, void* Context)
{
    g_DetoursData = Data->Detours;

    std::wstring pipe_name;

    const wchar_t* OriginalFilename;
    if (GetModuleVersionInfo(nullptr, L"\\StringFileInfo\\*\\OriginalFilename", &(LPCVOID&)OriginalFilename) < 0)
        return;

    if (SUCCEEDED(wil::TryGetEnvironmentVariableW(L"BNS_LOG", pipe_name))) {
        std::wstring full_pipe_name = L"\\\\.\\pipe\\" + pipe_name;
        g_namedPipe.open_write(full_pipe_name.c_str());
    }

    load_iprewriter_patches();

    switch (fnv1a::make_hash(OriginalFilename, fnv1a::ascii_toupper)) {
    case L"Client.exe"_fnv1au:
    case L"BNSR.exe"_fnv1au:
        NtCurrentPeb()->BeingDebugged = FALSE;
        g_DetoursData->TransactionBegin();
        g_DetoursData->UpdateThread(NtCurrentThread());

        LoadLibraryA("ws2_32.dll");

        if (const auto module = pe::get_module(L"ws2_32.dll")) {
            if (g_DetoursData->Attach2(module, "WSAConnect", &(PVOID&)g_pfnWSAConnect, &WSAConnect_hook))
                log("Failed to patch import: WSAConnect")
            else
                log("Successfully patched import: WSAConnect");
        }
        else log("Failed to patch module: ws2_32.dll");

        if (const auto module = pe::get_module(L"ntdll.dll")) {
            g_DetoursData->Attach2(module, "NtCreateMutant", &(PVOID&)g_pfnNtCreateMutant, &NtCreateMutant_hook);
            g_DetoursData->Attach2(module, "NtOpenKeyEx", &(PVOID&)g_pfnNtOpenKeyEx, &NtOpenKeyEx_hook);
            g_DetoursData->Attach2(module, "NtProtectVirtualMemory", &(PVOID&)g_pfnNtProtectVirtualMemory, &NtProtectVirtualMemory_hook);
            g_DetoursData->Attach2(module, "NtQuerySystemInformation", &(PVOID&)g_pfnNtQuerySystemInformation, &NtQuerySystemInformation_hook);
            g_DetoursData->Attach2(module, "NtOpenFile", &(PVOID&)g_pfnNtOpenFile, &NtOpenFile_hook);
            g_DetoursData->Attach2(module, "NtCreateFile", &(PVOID&)g_pfnNtCreateFile, &NtCreateFile_hook);
#ifdef _M_X64
            g_DetoursData->Attach2(module, "NtQueryInformationProcess", &(PVOID&)g_pfnNtQueryInformationProcess, &NtQueryInformationProcess_hook);
            g_DetoursData->Attach2(module, "NtSetInformationThread", &(PVOID&)g_pfnNtSetInformationThread, &NtSetInformationThread_hook);
#endif
        }

        if (const auto module = pe::get_module(L"user32.dll"))
            g_DetoursData->Attach2(module, "FindWindowA", &(PVOID&)g_pfnFindWindowA, &FindWindowA_hook);

        g_DetoursData->TransactionCommit();
        break;
    }
}

#ifndef __DATEW__
#define __DATEW__ _CRT_WIDE(__DATE__)
#endif

__declspec(dllexport)
void __cdecl GetPluginInfo2(PluginInfo2* plgi)
{
    plgi->Name = L"bnspatch";
    plgi->Version = __DATEW__;
    plgi->Description = L"XML patching, multi-client, and bypasses some Themida/WL protections";
    plgi->InitNotification = &InitNotification;
    plgi->DllLoadedNotification = &DllLoadedNotification;
}
