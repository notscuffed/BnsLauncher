#include "versioninfo.h"
#include <phnt_windows.h>
#include <phnt.h>
#include <intsafe.h>
#include <strsafe.h>
#include <wchar.h>

#define DWORDUP(x) (((x) + 3) & ~3)
#define KEYSIZEMAX(p) ((p)->wTotLen - FIELD_OFFSET(VERBLOCK, szKey))

typedef struct tagVERBLOCK
{
    WORD wTotLen;
    WORD wValLen;
    WORD wType;
    WCHAR szKey[1];
} VERBLOCK;

__declspec(align(4))
typedef struct tagVERHEAD
{
    WORD wTotLen;
    WORD wValLen;
    WORD wType;
    WCHAR szKey[ARRAYSIZE(L"VS_VERSION_INFO")];
    VS_FIXEDFILEINFO vsf;
} VERHEAD;

EXTERN_C int GetModuleVersionInfo(HMODULE hModule, PCWSTR pwszSubBlock, LPCVOID* ppv)
{
    HRSRC rsrc;
    size_t cb;
    const VERHEAD* head;
    size_t off;
    size_t len;
    UINT_PTR end;
    const VERBLOCK* block;
    const VERBLOCK* sub;
    int cch;
    PCWSTR start;
    long index;
    wchar_t* endptr;
    int ncmp;
    UINT_PTR remain;

    if (!ppv)
        return -1;

    *ppv = NULL;

    if (!pwszSubBlock
        || !(rsrc = FindResourceW(hModule, MAKEINTRESOURCEW(VS_VERSION_INFO), VS_FILE_INFO))
        || !(cb = SizeofResource(hModule, rsrc))
        || !(head = (VERHEAD*)LoadResource(hModule, rsrc))
        || head->wTotLen > cb
        || head->wTotLen < sizeof * head
        || head->wTotLen > MAXSHORT
        || head->wType
        || head->wValLen != sizeof head->vsf
        || FAILED(StringCbLengthW(head->szKey, KEYSIZEMAX(head), &cb))
        || (off = DWORDUP(sizeof(VERBLOCK) + cb)) != FIELD_OFFSET(VERHEAD, vsf)
        || head->vsf.dwSignature != VS_FFI_SIGNATURE
        || (len = off + head->wValLen) > head->wTotLen)
        return -1;

    block = (const VERBLOCK*)head;

    while (*pwszSubBlock) {
        while (*pwszSubBlock == '\\')
            ++pwszSubBlock;
        if (!*pwszSubBlock) break;

        end = (UINT_PTR)block + block->wTotLen;
        sub = (const VERBLOCK*)((UINT_PTR)block + DWORDUP(len));

        start = pwszSubBlock;
        while (*pwszSubBlock && *pwszSubBlock != '\\')
            ++pwszSubBlock;

        cch = (int)(pwszSubBlock - start);

        index = -1;
        if (*start == '*' && start + 1 == pwszSubBlock) {
            // take first match
            index = 0;
        }
        else if (*start == '#') {
            // take index #n
            index = wcstol(start + 1, &endptr, 10);
            if (endptr != pwszSubBlock)
                index = -1;
        }

        ncmp = 0;
        while (SUCCEEDED(UIntPtrSub(end, (UINT_PTR)sub, &remain))
            && remain >= sizeof * sub
            && sub->wTotLen >= sizeof * sub
            && sub->wTotLen <= remain
            && SUCCEEDED(StringCbLengthW(sub->szKey, KEYSIZEMAX(sub), &cb))
            && (len = (off = DWORDUP(sizeof * sub + cb)) + sub->wValLen) <= sub->wTotLen) {

            if (index >= 0) {
                if (!index--)
                    ncmp = CSTR_EQUAL;
            }
            else {
                ncmp = CompareStringEx(
                    LOCALE_NAME_INVARIANT,
                    NORM_IGNORECASE,
                    start,
                    cch,
                    sub->szKey,
                    (int)(cb / sizeof * sub->szKey),
                    NULL,
                    NULL,
                    0);
                if (!ncmp)
                    return -1;
            }
            if (ncmp == CSTR_EQUAL) {
                block = sub;
                break;
            }
            sub = (const VERBLOCK*)((UINT_PTR)sub + DWORDUP(sub->wTotLen));
        }
        if (ncmp != CSTR_EQUAL)
            return -1;
    }
    *ppv = (LPCVOID)(off < block->wTotLen ? (UINT_PTR)block + off : (UINT_PTR)block->szKey + cb);
    return block->wValLen;
}
