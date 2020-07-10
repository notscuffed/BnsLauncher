#include <phnt_windows.h>
#include <phnt.h>
#include <delayimp.h>

#include <filesystem>
#include <vector>

#include <pe/export_directory.h>
#include <pe/module.h>
#include <wil/stl.h>
#include <wil/win32_helpers.h>
#include <detours.h>

#include "FastWildCompare.h"
#include "pluginsdk.h"

LONG WINAPI DetourAttach2(HMODULE hModule, PCSTR pProcName, PVOID* pPointer, PVOID pDetour)
{
    if (!hModule) return ERROR_INVALID_PARAMETER;
    if (!pPointer) return ERROR_INVALID_PARAMETER;

    if (*pPointer = GetProcAddress(hModule, pProcName)) {
        return DetourAttachEx(pPointer, pDetour, nullptr, nullptr, nullptr);
    }
    return ERROR_PROC_NOT_FOUND;
}

static const DetoursData g_detourfunctions = {
  &DetourTransactionBegin,
  &DetourTransactionAbort,
  &DetourTransactionCommit,
  &DetourUpdateThread,
  &DetourAttach,
  &DetourAttach2,
  &DetourDetach
};

static std::vector<PluginInfo2> g_plugins;

PVOID g_pvDllNotificationCookie;

VOID CALLBACK DllNotification(ULONG NotificationReason, PLDR_DLL_NOTIFICATION_DATA NotificationData, PVOID Context)
{
    switch (NotificationReason) {
    case LDR_DLL_NOTIFICATION_REASON_LOADED: {
        const auto Data = DllNotificationData{
          NotificationData->Loaded.Flags,
          NotificationData->Loaded.FullDllName->Buffer,
          (SIZE_T)NotificationData->Loaded.FullDllName->Length >> 1u,
          NotificationData->Loaded.BaseDllName->Buffer,
          (SIZE_T)NotificationData->Loaded.BaseDllName->Length >> 1u,
          (HINSTANCE)NotificationData->Loaded.DllBase,
          NotificationData->Loaded.SizeOfImage,
          &g_detourfunctions
        };

        for (const auto& plgi : g_plugins) {
            if (plgi.DllLoadedNotification)
                plgi.DllLoadedNotification(&Data, plgi.Context);
        }
        break;
    }

    case LDR_DLL_NOTIFICATION_REASON_UNLOADED: {
        const auto Data = DllNotificationData{
          NotificationData->Unloaded.Flags,
          NotificationData->Unloaded.FullDllName->Buffer,
          (SIZE_T)NotificationData->Unloaded.FullDllName->Length >> 1u,
          NotificationData->Unloaded.BaseDllName->Buffer,
          (SIZE_T)NotificationData->Unloaded.BaseDllName->Length >> 1u,
          (HINSTANCE)NotificationData->Unloaded.DllBase,
          NotificationData->Unloaded.SizeOfImage,
          &g_detourfunctions
        };

        for (const auto& plgi : g_plugins) {
            if (plgi.DllUnloadedNotification)
                plgi.DllUnloadedNotification(&Data, plgi.Context);
        }
        break;
    }
    }
}

VOID NTAPI ApcLoadPlugins(ULONG_PTR Parameter)
{
    auto find_file_data = WIN32_FIND_DATAW();
    const auto folder = std::filesystem::path(pe::get_module()->full_name()).remove_filename().append(L"plugins");

    auto ec = std::error_code();
    for (const auto& it : std::filesystem::directory_iterator(folder, ec)) {
        if (!it.is_regular_file())
            continue;

        if (FastWildCompare(L"*.dll", it.path().filename())) {
            const auto path = std::filesystem::canonical(folder / find_file_data.cFileName);
            auto module = static_cast<pe::module*>(LoadLibraryExW(it.path().c_str(), nullptr, LOAD_WITH_ALTERED_SEARCH_PATH));
            if (!module)
                continue;

            if (const auto GetPluginInfo2 = reinterpret_cast<GetPluginInfo2Fn>(module->function("GetPluginInfo2"))) {
                auto plgi = PluginInfo2();
                memset(&plgi, 0, sizeof plgi);
                GetPluginInfo2(&plgi);
                g_plugins.push_back(plgi);
                if (plgi.InitNotification) {
                    const auto Data = InitNotificationData{ &g_detourfunctions };
                    plgi.InitNotification(&Data, plgi.Context);
                }
            }
            else if (const auto GetPluginInfo = reinterpret_cast<GetPluginInfoFn>(module->function("GetPluginInfo"))) {
                auto plgi = PluginInfo();
                memset(&plgi, 0, sizeof plgi);
                GetPluginInfo(&plgi);
                if (plgi.Init)
                    plgi.Init();
            }
            else {
                FreeLibrary(module);
                continue;
            }
        }
    }

    if (const auto module = pe::get_module(L"ntdll.dll")) {
        if (const auto pLdrRegisterDllNotification = reinterpret_cast<decltype(&LdrRegisterDllNotification)>(module->function("LdrRegisterDllNotification"))) {
            pLdrRegisterDllNotification(0, &DllNotification, nullptr, &g_pvDllNotificationCookie);
        }
    }
}

BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD fdwReason, LPVOID lpvReserved)
{
    if (fdwReason == DLL_PROCESS_ATTACH) {
        DisableThreadLibraryCalls(hInstance);
        QueueUserAPC(&ApcLoadPlugins, NtCurrentThread(), 0);
    }
    return TRUE;
}

ExternC const PfnDliHook __pfnDliNotifyHook2 = [](unsigned dliNotify, PDelayLoadInfo pdli) -> FARPROC {
    if (dliNotify == dliNotePreLoadLibrary) {
        const auto module = pe::instance_module();
        if (!_stricmp(pdli->szDll, module->export_directory()->name())) {
            NtTestAlert();
            if (std::wstring result; SUCCEEDED(wil::GetSystemDirectoryW(result))) {
                const auto path = std::filesystem::path(result).append(pdli->szDll);
                return (FARPROC)LoadLibraryExW(path.c_str(), nullptr, LOAD_WITH_ALTERED_SEARCH_PATH);
            }
        }
    }
    return nullptr;
};
