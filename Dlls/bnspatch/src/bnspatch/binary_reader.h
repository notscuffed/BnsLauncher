#pragma once
#include <type_traits>
#include <string>
#include <cstdint>
#include <array>

#include <SafeInt.hpp>

class binary_reader
{
    const uint8_t* ptr;
    const size_t size;
    SafeInt<ptrdiff_t> pos;

public:
    binary_reader(const void* ptr, const size_t size)
        : ptr(reinterpret_cast<const uint8_t*>(ptr)), size(size), pos(0)
    {
    }

    template <class T>
    inline const T peek()
    {
        static_assert(std::is_integral_v<T>);

        if (sizeof(T) > size)
            throw std::exception();

        return *reinterpret_cast<const T*>(ptr + static_cast<ptrdiff_t>(pos));
    }

    template <class T>
    inline const T get_aligned()
    {
        static_assert(std::is_integral_v<T>);

        const auto res = peek<T>();
        pos += (sizeof(T) + 3) & ~3;
        return res;
    }

    template <class T>
    inline const T get()
    {
        static_assert(std::is_integral_v<T>);

        const auto res = peek<T>();
        pos += sizeof(T);
        return res;
    }

    template <>
    inline const std::wstring get<std::wstring>()
    {
        static const auto cipherKeys = std::array{
          0x9fa4ui16,
          0xb3d8ui16,
          0x8ef6ui16,
          0xc239ui16,
          0xe02dui16,
          0x7561ui16,
          0x4b5cui16,
          0x071aui16
        };

        auto res = std::wstring();
        const auto count = get<uint32_t>();
        res.reserve(count);

        for (auto i = 0ui32; i < count; ++i)
            res.push_back(get<uint16_t>() ^ cipherKeys[i & (cipherKeys.size() - 1)]);

        return res;
    }

    template <class T>
    inline void discard(const size_t count = 1)
    {
        pos += sizeof(T) * count;
    }

    template <class T>
    inline void discarda(const size_t count = 1)
    {
        pos += ((sizeof(T) + 3) & ~3) * count;
    }
};
