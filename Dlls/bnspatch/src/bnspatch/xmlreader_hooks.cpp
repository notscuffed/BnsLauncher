#include "xmlreader_hooks.h"
#include <phnt_windows.h>
#include <phnt.h>

#include <numeric>

#include <fmt/format.h>
#include <SafeInt.hpp>

#include "xmlpatch.h"
#include "xmlreader.h"

struct xml_buffer_writer : pugi::xml_writer
{
    std::vector<unsigned char> result;

    virtual void write(const void* data, size_t size)
    {
        result.insert(result.end(),
            reinterpret_cast<const unsigned char*>(data),
            reinterpret_cast<const unsigned char*>(data) + size);
    }
};

template <class Char, class Traits = std::char_traits<Char>, class Alloc = std::allocator<Char>>
struct xml_basic_string_writer : pugi::xml_writer
{
    std::basic_string<Char, Traits, Alloc> result;

    virtual void write(const void* data, size_t size)
    {
        result.append(
            reinterpret_cast<const Char*>(data),
            reinterpret_cast<const Char*>(reinterpret_cast<const unsigned char*>(data) + size));
    }
};

using xml_string_writer = xml_basic_string_writer<char>;
using xml_wstring_writer = xml_basic_string_writer<wchar_t>;

v13::XmlDoc* (__thiscall* g_pfnReadFromFile13)(const v13::XmlReader* a, const wchar_t*);
v13::XmlDoc* thiscall_(ReadFromFile13_hook, const v13::XmlReader* thisptr, const wchar_t* xml)
{
    if (!xml)
        return nullptr;

    auto patches = get_relevant_patches(xml);
    if (!patches.empty()) {
        pugi::xml_document document;
        if (const auto res = document.load_file(xml, pugi::parse_full)) {
            apply_patches(document, res.encoding, patches);
            xml_buffer_writer writer;
            document.save(writer, nullptr, pugi::format_raw | pugi::format_no_declaration, res.encoding);
            return g_pfnReadFromBuffer13(thisptr, writer.result.data(), SafeInt(writer.result.size()), xml);
        }
    }
    return g_pfnReadFromFile13(thisptr, xml);
}

v14::XmlDoc* (__thiscall* g_pfnReadFromFile14)(const v14::XmlReader*, const wchar_t*, class v14::XmlPieceReader*);
v14::XmlDoc* thiscall_(ReadFromFile14_hook, const v14::XmlReader* thisptr, const wchar_t* xml, class v14::XmlPieceReader* xmlPieceReader)
{
    if (!xml)
        return nullptr;

    auto patches = get_relevant_patches(xml);
    if (!patches.empty()) {
        pugi::xml_document document;
        if (const auto res = document.load_file(xml, pugi::parse_full)) {
            apply_patches(document, res.encoding, patches);
            xml_buffer_writer writer;
            document.save(writer, nullptr, pugi::format_raw | pugi::format_no_declaration, res.encoding);
            return g_pfnReadFromBuffer14(thisptr, writer.result.data(), SafeInt(writer.result.size()), xml, xmlPieceReader);
        }
    }
    return g_pfnReadFromFile14(thisptr, xml, xmlPieceReader);
}

v13::XmlDoc* (__thiscall* g_pfnReadFromBuffer13)(const v13::XmlReader*, const unsigned char*, unsigned int, const wchar_t*);
v13::XmlDoc* thiscall_(ReadFromBuffer13_hook, const v13::XmlReader* thisptr, const unsigned char* mem, unsigned int size, const wchar_t* xmlFileNameForLogging)
{
    if (!mem || !size)
        return nullptr;

    if (xmlFileNameForLogging && *xmlFileNameForLogging) {
        auto patches = get_relevant_patches(xmlFileNameForLogging);
        auto addons = get_relevant_addons(xmlFileNameForLogging);
        if (!patches.empty() || !addons.empty()) {
            pugi::xml_document document;
            if (const auto res = deserialize_document(mem, size, document)) {
                apply_patches(document, res.encoding, patches);

                if (!addons.empty() && res.encoding == pugi::encoding_utf16_le) {
                    auto writer = xml_wstring_writer();
                    document.save(writer, L"", pugi::format_default | pugi::format_no_declaration, res.encoding);

                    for (const auto& addon : addons)
                        ReplaceStringInPlace(writer.result, addon.first, addon.second);
                    return g_pfnReadFromBuffer13(thisptr, reinterpret_cast<unsigned char*>(writer.result.data()), SafeInt(writer.result.size()) * sizeof(wchar_t), xmlFileNameForLogging);
                }
                else {
                    // don't apply addons
                    auto writer = xml_buffer_writer();
                    document.save(writer, nullptr, pugi::format_raw | pugi::format_no_declaration, res.encoding);
                    return g_pfnReadFromBuffer13(thisptr, writer.result.data(), SafeInt(writer.result.size()), xmlFileNameForLogging);
                }
            }
        }
    }
    return g_pfnReadFromBuffer13(thisptr, mem, size, xmlFileNameForLogging);
}

v14::XmlDoc* (__thiscall* g_pfnReadFromBuffer14)(const v14::XmlReader*, const unsigned char*, unsigned int, const wchar_t*, class v14::XmlPieceReader*);
v14::XmlDoc* thiscall_(ReadFromBuffer14_hook, const v14::XmlReader* thisptr, const unsigned char* mem, unsigned int size, const wchar_t* xmlFileNameForLogging, class v14::XmlPieceReader* xmlPieceReader)
{
    if (!mem || !size)
        return nullptr;

    if (xmlFileNameForLogging && *xmlFileNameForLogging) {
        auto patches = get_relevant_patches(xmlFileNameForLogging);
        auto addons = get_relevant_addons(xmlFileNameForLogging);
        if (!patches.empty() || !addons.empty()) {
            auto document = pugi::xml_document();
            if (const auto res = deserialize_document(mem, size, document)) {

#ifdef _DEBUG
                if (xmlFileNameForLogging && *xmlFileNameForLogging) {
                    const auto temp = patches_path().parent_path().append(L"temp").append(xmlFileNameForLogging);
                    document.save_file(temp.c_str(), L"", pugi::format_raw | pugi::format_no_declaration, res.encoding);
                }
#endif

                apply_patches(document, res.encoding, patches);

#ifdef _DEBUG
                if (addons.empty() && xmlFileNameForLogging && *xmlFileNameForLogging) {
                    const auto temp_patched = patches_path().parent_path().append(L"temp_patched").append(xmlFileNameForLogging);
                    document.save_file(temp_patched.c_str(), L"", pugi::format_raw | pugi::format_no_declaration, res.encoding);
                }
#endif

                if (!addons.empty() && res.encoding == pugi::encoding_utf16_le) {
                    auto writer = xml_wstring_writer();
                    document.save(writer, L"", pugi::format_default | pugi::format_no_declaration, res.encoding);

                    for (const auto& addon : addons)
                        ReplaceStringInPlace(writer.result, addon.first, addon.second);
                    return g_pfnReadFromBuffer14(thisptr, reinterpret_cast<unsigned char*>(writer.result.data()), SafeInt(writer.result.size()) * sizeof(wchar_t), xmlFileNameForLogging, xmlPieceReader);
                }
                else {
                    // don't apply addons
                    auto writer = xml_buffer_writer();
                    document.save(writer, nullptr, pugi::format_raw | pugi::format_no_declaration, res.encoding);
                    return g_pfnReadFromBuffer14(thisptr, writer.result.data(), SafeInt(writer.result.size()), xmlFileNameForLogging, xmlPieceReader);
                }
            }
        }
    }
    return g_pfnReadFromBuffer14(thisptr, mem, size, xmlFileNameForLogging, xmlPieceReader);
}
