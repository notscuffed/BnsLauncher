#pragma once
#include <Windows.h>

class thread_local_lock
{
public:
  using native_handle_type = unsigned long;

private:
  native_handle_type state_index;

public:
  thread_local_lock() : state_index(TlsAlloc()) {}

  ~thread_local_lock() { (void)TlsFree(state_index); }

  void lock() { (void)TlsSetValue(state_index, (LPVOID)1); }

  bool try_lock()
  {
    if ( TlsGetValue(state_index) == nullptr && GetLastError() == ERROR_SUCCESS ) {
      return TlsSetValue(state_index, (LPVOID)1);
    }
    return false;
  }

  void unlock() { (void)TlsSetValue(state_index, nullptr); }

  native_handle_type native_handle() { return state_index; }
};
