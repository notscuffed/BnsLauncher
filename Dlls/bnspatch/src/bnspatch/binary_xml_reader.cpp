#include "binary_xml_reader.h"


inline void binary_xml_reader::unsafe_discard_bytes(uint32_t discarded_bytes_count)
{
  mem = mem + discarded_bytes_count;
  size -= discarded_bytes_count;
}

inline bool binary_xml_reader::discard_bytes(uint32_t discarded_bytes_count)
{
  if ( size < discarded_bytes_count )
    return false;

  unsafe_discard_bytes(discarded_bytes_count);
  return true;
}

inline bool binary_xml_reader::discard_string()
{
  auto length = read_aligned<int>();
  if ( length.value_or(-1) < 0 )
    return false;

  return discard_bytes(*length * sizeof(wchar_t));
}

std::optional<std::wstring> binary_xml_reader::read_string()
{
  static const auto cipherKeys = std::array {
    0x9fa4ui16,
    0xb3d8ui16,
    0x8ef6ui16,
    0xc239ui16,
    0xe02dui16,
    0x7561ui16,
    0x4b5cui16,
    0x071aui16
  };

  auto length = read_aligned<uint32_t>().value_or(-1);
  if ( size < length * sizeof(wchar_t) )
    return std::nullopt;

  std::wstring result;
  result.reserve(length);

  auto data = reinterpret_cast<const uint16_t *>(mem);
  for ( size_t i = 0; i < length; ++i )
    result.push_back(data[i] ^ cipherKeys[i & (cipherKeys.size() - 1)]);
  mem += length * sizeof(uint16_t);
  return result;
}
