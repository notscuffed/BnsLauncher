#include "hooks.h"

#include <phnt_windows.h>
#include <phnt.h>
#include <fnv1a.h>

#include <mutex>

#include <ntapi/string.h>

#include "thread_local_lock.h"
#include "versioninfo.h"
#include "logging.h"
#include "antigg.h"
#include "iprewriter.h"

decltype(&NtCreateMutant) g_pfnNtCreateMutant;
NTSTATUS NTAPI NtCreateMutant_hook(
    PHANDLE MutantHandle,
    ACCESS_MASK DesiredAccess,
    POBJECT_ATTRIBUTES ObjectAttributes,
    BOOLEAN InitialOwner)
{
    if (ObjectAttributes && ObjectAttributes->ObjectName) {
        const auto objectName = static_cast<ntapi::ustring*>(ObjectAttributes->ObjectName);
        if (objectName->starts_with(L"BnSGameClient")
            || objectName->starts_with(L"Global\\MtxNPG"))
        {
            ObjectAttributes->ObjectName = nullptr;
            ObjectAttributes->Attributes &= ~OBJ_OPENIF;
            ObjectAttributes->RootDirectory = nullptr;
        }
    }
    return g_pfnNtCreateMutant(MutantHandle, DesiredAccess, ObjectAttributes, InitialOwner);
}

decltype(&NtOpenKeyEx) g_pfnNtOpenKeyEx;
NTSTATUS NTAPI NtOpenKeyEx_hook(
    PHANDLE KeyHandle,
    ACCESS_MASK DesiredAccess,
    POBJECT_ATTRIBUTES ObjectAttributes,
    ULONG OpenOptions)
{
    if (auto const ObjectName = static_cast<ntapi::ustring*>(ObjectAttributes->ObjectName)) {
        switch (fnv1a::make_hash(ObjectName->data(), ObjectName->size(), fnv1a::ascii_toupper)) {
        case L"Software\\Wine"_fnv1au:
        case L"HARDWARE\\ACPI\\DSDT\\VBOX__"_fnv1au:
            return STATUS_OBJECT_NAME_NOT_FOUND;
        }
    }
    return g_pfnNtOpenKeyEx(KeyHandle, DesiredAccess, ObjectAttributes, OpenOptions);
}

decltype(&NtProtectVirtualMemory) g_pfnNtProtectVirtualMemory;
NTSTATUS NTAPI NtProtectVirtualMemory_hook(
    HANDLE ProcessHandle,
    PVOID* BaseAddress,
    PSIZE_T RegionSize,
    ULONG NewProtect,
    PULONG OldProtect)
{
    PROCESS_BASIC_INFORMATION pbi;
    SYSTEM_BASIC_INFORMATION sbi;
    ULONG_PTR StartingAddress;

    if (NewProtect & (PAGE_READWRITE | PAGE_WRITECOPY | PAGE_EXECUTE_READWRITE | PAGE_EXECUTE_WRITECOPY | PAGE_WRITECOMBINE)
        && (ProcessHandle == NtCurrentProcess()
            || (NT_SUCCESS(NtQueryInformationProcess(ProcessHandle, ProcessBasicInformation, &pbi, sizeof(PROCESS_BASIC_INFORMATION), nullptr))
                && pbi.UniqueProcessId == NtCurrentTeb()->ClientId.UniqueProcess))
        && NT_SUCCESS(NtQuerySystemInformation(SystemBasicInformation, &sbi, sizeof(SYSTEM_BASIC_INFORMATION), nullptr))) {

        __try {
            StartingAddress = (ULONG_PTR)*BaseAddress & ~((ULONG_PTR)sbi.PageSize - 1);
        }
        __except (EXCEPTION_EXECUTE_HANDLER) {
            return GetExceptionCode();
        }

        for (const auto Address : std::array{ (ULONG_PTR)&DbgBreakPoint, (ULONG_PTR)&DbgUiRemoteBreakin }) {
            if (Address && StartingAddress == (Address & ~((ULONG_PTR)sbi.PageSize - 1)))
                return STATUS_INVALID_PARAMETER_2;
        }
    }
    return g_pfnNtProtectVirtualMemory(ProcessHandle, BaseAddress, RegionSize, NewProtect, OldProtect);
}

decltype(&NtQueryInformationProcess) g_pfnNtQueryInformationProcess;
NTSTATUS NTAPI NtQueryInformationProcess_hook(
    HANDLE ProcessHandle,
    PROCESSINFOCLASS ProcessInformationClass,
    PVOID ProcessInformation,
    ULONG ProcessInformationLength,
    PULONG ReturnLength)
{
    PROCESS_BASIC_INFORMATION pbi;

    if (ProcessHandle == NtCurrentProcess()
        || (NT_SUCCESS(g_pfnNtQueryInformationProcess(ProcessHandle, ProcessBasicInformation, &pbi, sizeof(PROCESS_BASIC_INFORMATION), nullptr))
            && pbi.UniqueProcessId == NtCurrentTeb()->ClientId.UniqueProcess)) {

        switch (ProcessInformationClass) {
        case ProcessDebugPort:
            if (ProcessInformationLength != sizeof(DWORD_PTR))
                return STATUS_INFO_LENGTH_MISMATCH;
            *(PDWORD_PTR)ProcessInformation = 0;
            if (ReturnLength)
                *ReturnLength = sizeof(DWORD_PTR);
            return STATUS_SUCCESS;

        case ProcessDebugObjectHandle:
            if (ProcessInformationLength != sizeof(HANDLE))
                return STATUS_INFO_LENGTH_MISMATCH;
            *(PHANDLE)ProcessInformation = nullptr;
            if (ReturnLength)
                *ReturnLength = sizeof(HANDLE);
            return STATUS_PORT_NOT_SET;
        }
    }
    return g_pfnNtQueryInformationProcess(
        ProcessHandle,
        ProcessInformationClass,
        ProcessInformation,
        ProcessInformationLength,
        ReturnLength);
}

decltype(&NtQuerySystemInformation) g_pfnNtQuerySystemInformation;
NTSTATUS NTAPI NtQuerySystemInformation_hook(
    SYSTEM_INFORMATION_CLASS SystemInformationClass,
    PVOID SystemInformation,
    ULONG SystemInformationLength,
    PULONG ReturnLength)
{
    switch (SystemInformationClass) {
    case SystemModuleInformation:
        if (SystemInformationLength < FIELD_OFFSET(RTL_PROCESS_MODULES, Modules))
            return STATUS_INFO_LENGTH_MISMATCH;
        return STATUS_ACCESS_DENIED;

    case SystemModuleInformationEx:
        if (SystemInformationLength < sizeof(RTL_PROCESS_MODULE_INFORMATION_EX))
            return STATUS_INFO_LENGTH_MISMATCH;
        return STATUS_ACCESS_DENIED;

    case SystemKernelDebuggerInformation:
        if (SystemInformationLength < sizeof(SYSTEM_KERNEL_DEBUGGER_INFORMATION))
            return STATUS_INFO_LENGTH_MISMATCH;
        ((PSYSTEM_KERNEL_DEBUGGER_INFORMATION)SystemInformation)->KernelDebuggerEnabled = FALSE;
        ((PSYSTEM_KERNEL_DEBUGGER_INFORMATION)SystemInformation)->KernelDebuggerNotPresent = TRUE;
        if (ReturnLength)
            *ReturnLength = sizeof(SYSTEM_KERNEL_DEBUGGER_INFORMATION);
        return STATUS_SUCCESS;
    }
    return g_pfnNtQuerySystemInformation(
        SystemInformationClass,
        SystemInformation,
        SystemInformationLength,
        ReturnLength);
}

decltype(&NtSetInformationThread) g_pfnNtSetInformationThread;
NTSTATUS NTAPI NtSetInformationThread_hook(
    HANDLE ThreadHandle,
    THREADINFOCLASS ThreadInformationClass,
    PVOID ThreadInformation,
    ULONG ThreadInformationLength)
{
    THREAD_BASIC_INFORMATION tbi;

    if (ThreadInformationClass == ThreadHideFromDebugger
        && ThreadInformationLength == 0) {
        if (ThreadHandle == NtCurrentThread()
            || (NT_SUCCESS(NtQueryInformationThread(ThreadHandle, ThreadBasicInformation, &tbi, sizeof(THREAD_BASIC_INFORMATION), 0))
                && tbi.ClientId.UniqueProcess == NtCurrentTeb()->ClientId.UniqueProcess)) {
            return STATUS_SUCCESS;
        }
    }
    return g_pfnNtSetInformationThread(ThreadHandle, ThreadInformationClass, ThreadInformation, ThreadInformationLength);
}

decltype(&FindWindowA) g_pfnFindWindowA;
HWND WINAPI FindWindowA_hook(
    LPCSTR lpClassName,
    LPCSTR lpWindowName)
{
    if (lpClassName) {
        switch (fnv1a::make_hash(lpClassName, fnv1a::ascii_toupper)) {
#ifdef _M_IX86
        case "OLLYDBG"_fnv1au:
        case "GBDYLLO"_fnv1au:
        case "pediy06"_fnv1au:
#endif         
        case "FilemonClass"_fnv1au:
        case "PROCMON_WINDOW_CLASS"_fnv1au:
        case "RegmonClass"_fnv1au:
        case "18467-41"_fnv1au:
            return nullptr;
        }
    }
    if (lpWindowName) {
        switch (fnv1a::make_hash(lpWindowName)) {
        case "File Monitor - Sysinternals: www.sysinternals.com"_fnv1a:
        case "Process Monitor - Sysinternals: www.sysinternals.com"_fnv1a:
        case "Registry Monitor - Sysinternals: www.sysinternals.com"_fnv1a:
            return nullptr;
        }
    }
    return g_pfnFindWindowA(lpClassName, lpWindowName);
}

decltype(&NtOpenFile) g_pfnNtOpenFile;
NTSTATUS NTAPI NtOpenFile_hook(
    PHANDLE FileHandle,
    ACCESS_MASK DesiredAccess,
    POBJECT_ATTRIBUTES ObjectAttributes,
    PIO_STATUS_BLOCK IoStatusBlock,
    ULONG ShareAccess,
    ULONG OpenOptions) {

    if (auto const ObjectName = static_cast<ntapi::ustring*>(ObjectAttributes->ObjectName)) {
        std::basic_string_view fname(ObjectName->data());

        if (fname.ends_with(L"\\NETUTILS.DLL")) {
            logfmt(patch_gameguard() ? "Successfully bypassed GameGuard" : "Failed to bypass GameGuard");
        }
    }

    return g_pfnNtOpenFile(FileHandle, DesiredAccess, ObjectAttributes, IoStatusBlock, ShareAccess, OpenOptions);
}

decltype(&WSAConnect) g_pfnWSAConnect;
int WSAAPI WSAConnect_hook(
    SOCKET s,
    sockaddr* name,
    int namelen,
    LPWSABUF lpCallerData,
    LPWSABUF lpCalleeData,
    LPQOS lpSQOS,
    LPQOS lpGQOS) {

    if (name->sa_family != AF_INET)
        return g_pfnWSAConnect(s, name, namelen, lpCallerData, lpCalleeData, lpSQOS, lpGQOS);

    auto ip4name = reinterpret_cast<sockaddr_in*>(name);

    patch_ip(ip4name->sin_addr.S_un.S_addr, ip4name->sin_port);

    return g_pfnWSAConnect(s, name, namelen, lpCallerData, lpCalleeData, lpSQOS, lpGQOS);
}

decltype(&NtCreateFile) g_pfnNtCreateFile;
NTSTATUS NTAPI NtCreateFile_hook(
    PHANDLE FileHandle,
    ACCESS_MASK DesiredAccess,
    POBJECT_ATTRIBUTES ObjectAttributes,
    PIO_STATUS_BLOCK IoStatusBlock,
    PLARGE_INTEGER AllocationSize,
    ULONG FileAttributes,
    ULONG ShareAccess,
    ULONG CreateDisposition,
    ULONG CreateOptions,
    PVOID EaBuffer,
    ULONG EaLength)
{
#ifdef _M_IX86
    if (auto const ObjectName = static_cast<ntapi::ustring*>(ObjectAttributes->ObjectName)) {
        switch (fnv1a::make_hash(ObjectName->data(), ObjectName->size(), fnv1a::ascii_toupper)) {
        case L"\\\\.\\SICE"_fnv1au:
        case L"\\\\.\\SIWVID"_fnv1au:
        case L"\\\\.\\NTICE"_fnv1au:
            return STATUS_OBJECT_NAME_NOT_FOUND;
        }
    }
#endif
    return g_pfnNtCreateFile(
        FileHandle,
        DesiredAccess,
        ObjectAttributes,
        IoStatusBlock,
        AllocationSize,
        FileAttributes,
        ShareAccess ? ShareAccess : FILE_SHARE_READ,
        CreateDisposition,
        CreateOptions,
        EaBuffer,
        EaLength);
}
