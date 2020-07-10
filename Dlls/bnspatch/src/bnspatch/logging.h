#pragma once

#include <mutex>
#include "namedpipe.h"

std::string Utf16ToUtf8(const wchar_t* wstring);

extern std::mutex g_logMutex;
extern NamedPipe g_namedPipe;

#define log(text) \
  g_namedPipe.write(text);

#define logfmt(format, ...) \
  _logfmt(format, __VA_ARGS__ );

template<typename ... Args>
void _logfmt(const char* format, Args ... args)
{
    const std::lock_guard<std::mutex> lock(g_logMutex);
    size_t size = snprintf(nullptr, 0, format, args ...) + 1;

    if (size <= 0)
        throw std::runtime_error("Error during formatting.");

    std::unique_ptr<char[]> buf(new char[size]);
    snprintf(buf.get(), size, format, args ...);

    g_namedPipe.write(buf.get(), size - 1);
}
