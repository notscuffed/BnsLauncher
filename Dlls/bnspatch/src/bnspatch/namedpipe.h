#pragma once

#include <windows.h>
#include <string>

class NamedPipe {
public:
    NamedPipe() noexcept : m_hPipe(INVALID_HANDLE_VALUE) {
    }

    ~NamedPipe() noexcept {
        close();
    }

    BOOL open_write(_In_ LPCWSTR lpName) {
        m_hPipe = CreateFile(lpName, GENERIC_WRITE, 0, nullptr, OPEN_EXISTING, 0, nullptr);
        return m_hPipe != INVALID_HANDLE_VALUE;
    }

    BOOL write(const char* lpBuffer) {
        return write(lpBuffer, strlen(lpBuffer));
    }

    BOOL write(std::string& str) {
        return write(str.c_str(), str.length());
    }

    BOOL write(_In_reads_bytes_opt_(nNumberOfBytesToWrite) LPCVOID lpBuffer, _In_ DWORD nNumberOfBytesToWrite, _Out_opt_ LPDWORD lpNumberOfBytesWritten = nullptr, _Inout_opt_ LPOVERLAPPED lpOverlapped = nullptr) noexcept {
        if (m_hPipe == INVALID_HANDLE_VALUE)
            return FALSE;

        BOOL result = WriteFile(m_hPipe, lpBuffer, nNumberOfBytesToWrite, lpNumberOfBytesWritten, lpOverlapped);

        if (!result) {
            close();
        }

        return result;
    }

    void close() {
        if (m_hPipe == INVALID_HANDLE_VALUE)
            return;

        CloseHandle(m_hPipe);
        m_hPipe = INVALID_HANDLE_VALUE;
    }

private:
    HANDLE m_hPipe;
};
