// Copyright 2018 IBM Corporation
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// Compares two text strings.  Accepts '?' as a single-character wildcard.  
// For each '*' wildcard, seeks out a matching sequence of any characters 
// beyond it.  Otherwise compares the strings a character at a time. 
//
#pragma once
#include <filesystem>

bool fast_wild_compare(const wchar_t* pWild, const wchar_t* pTame);

inline bool FastWildCompare(const std::filesystem::path& wild, const std::filesystem::path& tame)
{
    return fast_wild_compare(wild.c_str(), tame.c_str());
}

inline bool FastWildCompare(const wchar_t* pWild, const std::filesystem::path& tame)
{
    return fast_wild_compare(pWild, tame.c_str());
}
