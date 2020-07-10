#include "fastwildcompare.h"
#include <filesystem>

bool FastWildCompare(const wchar_t* pWild, const wchar_t* pTame)
{
    const wchar_t* pWildSequence; // Points to prospective wild string match after '*'
    const wchar_t* pTameSequence; // Points to prospective tame string match

    // Find a first wildcard, if one exists, and the beginning of any  
    // prospectively matching sequence after it.
    for (;; ) {
        // Check for the end from the start.  Get out fast, if possible.
        if (!*pTame) {
            if (*pWild) {
                while (*pWild++ == '*') {
                    if (!*pWild)
                        return true; // "ab" matches "ab*".
                }
                return false; // "abcd" doesn't match "abc".
            }
            else {
                return true; // "abc" matches "abc".
            }
        }
        else if (*pWild == '*') {
            // Got wild: set up for the second loop and skip on down there.
            while (*(++pWild) == '*')
                continue;

            if (!*pWild)
                return true; // "abc*" matches "abcd".

              // Search for the next prospective match.
            if (*pWild != '?') {
                while (__ascii_towupper(*pWild) != __ascii_towupper(*pTame)) {
                    if (!*(++pTame))
                        return false; // "a*bc" doesn't match "ab".
                }
            }

            // Keep fallback positions for retry in case of incomplete match.
            pWildSequence = pWild;
            pTameSequence = pTame;
            break;
        }
        else if (__ascii_towupper(*pWild) != __ascii_towupper(*pTame) && *pWild != '?') {
            return false; // "abc" doesn't match "abd".
        }

        ++pWild; // Everything's a match, so far.
        ++pTame;
    }

    // Find any further wildcards and any further matching sequences.
    for (;; ) {
        if (*pWild == '*') {
            // Got wild again.
            while (*(++pWild) == '*')
                continue;

            if (!*pWild)
                return true; // "ab*c*" matches "abcd".

            if (!*pTame)
                return false; // "*bcd*" doesn't match "abc".

              // Search for the next prospective match.
            if (*pWild != '?') {
                while (__ascii_towupper(*pWild) != __ascii_towupper(*pTame)) {
                    if (!*(++pTame))
                        return false; // "a*b*c" doesn't match "ab".
                }
            }

            // Keep the new fallback positions.
            pWildSequence = pWild;
            pTameSequence = pTame;
        }
        else if (__ascii_towupper(*pWild) != __ascii_towupper(*pTame) && *pWild != '?') {
            // The equivalent portion of the upper loop is really simple.
            if (!*pTame)
                return false; // "*bcd" doesn't match "abc".

              // A fine time for questions.
            while (*pWildSequence == '?') {
                ++pWildSequence;
                ++pTameSequence;
            }
            pWild = pWildSequence;

            // Fall back, but never so far again.
            while (__ascii_towupper(*pWild) != __ascii_towupper(*(++pTameSequence))) {
                if (!*pTameSequence)
                    return false; // "*a*b" doesn't match "ac".
            }
            pTame = pTameSequence;
        }

        // Another check for the end, at the end.
        if (!*pTame)
            return !*pWild;

        ++pWild; // Everything's still a match.
        ++pTame;
    }
}
