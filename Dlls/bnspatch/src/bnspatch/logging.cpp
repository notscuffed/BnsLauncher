#include "logging.h"

std::mutex g_logMutex;

NamedPipe g_namedPipe{};

std::string Utf16ToUtf8(const wchar_t* wstring)
{
    std::string convertedString;

    if (!wstring || !*wstring)
        return convertedString;

    auto size = WideCharToMultiByte(CP_UTF8, 0, wstring, -1, nullptr, 0, nullptr, nullptr);

    if (size > 0)
    {
        convertedString.resize(size - 1);
        if (!WideCharToMultiByte(CP_UTF8, 0, wstring, -1, (char*)convertedString.c_str(), size, nullptr, nullptr))
            convertedString.clear();
    }

    return convertedString;
}
