// ======================================================================== //
// Copyright 2009-2011 Intel Corporation                                    //
//                                                                          //
// Licensed under the Apache License, Version 2.0 (the "License");          //
// you may not use this file except in compliance with the License.         //
// You may obtain a copy of the License at                                  //
//                                                                          //
//     http://www.apache.org/licenses/LICENSE-2.0                           //
//                                                                          //
// Unless required by applicable law or agreed to in writing, software      //
// distributed under the License is distributed on an "AS IS" BASIS,        //
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. //
// See the License for the specific language governing permissions and      //
// limitations under the License.                                           //
// ======================================================================== //

#ifndef __EMBREE_CONSTANTS_H__
#define __EMBREE_CONSTANTS_H__

#ifndef NULL
#define NULL 0
#endif

#include <limits>

namespace embree
{
  static struct NullTy {
  } null MAYBE_UNUSED;

  static struct TrueTy {
    __forceinline operator bool( ) const { return true; }
  } True MAYBE_UNUSED;

  static struct FalseTy {
    __forceinline operator bool( ) const { return false; }
  } False MAYBE_UNUSED;

  static struct ZeroTy
  {
    __forceinline operator double( ) const { return 0; }
    __forceinline operator float ( ) const { return 0; }
    __forceinline operator int64 ( ) const { return 0; }
    __forceinline operator uint64( ) const { return 0; }
    __forceinline operator int32 ( ) const { return 0; }
    __forceinline operator uint32( ) const { return 0; }
    __forceinline operator int16 ( ) const { return 0; }
    __forceinline operator uint16( ) const { return 0; }
    __forceinline operator int8  ( ) const { return 0; }
    __forceinline operator uint8 ( ) const { return 0; }
#ifndef _WIN32
    __forceinline operator size_t( ) const { return 0; }
#endif

  } zero MAYBE_UNUSED;

  static struct OneTy
  {
    __forceinline operator double( ) const { return 1; }
    __forceinline operator float ( ) const { return 1; }
    __forceinline operator int64 ( ) const { return 1; }
    __forceinline operator uint64( ) const { return 1; }
    __forceinline operator int32 ( ) const { return 1; }
    __forceinline operator uint32( ) const { return 1; }
    __forceinline operator int16 ( ) const { return 1; }
    __forceinline operator uint16( ) const { return 1; }
    __forceinline operator int8  ( ) const { return 1; }
    __forceinline operator uint8 ( ) const { return 1; }
#ifndef _WIN32
    __forceinline operator size_t( ) const { return 1; }
#endif
  } one MAYBE_UNUSED;

  static struct NegInfTy
  {
    __forceinline operator double( ) const { return -std::numeric_limits<double>::infinity(); }
    __forceinline operator float ( ) const { return -std::numeric_limits<float>::infinity(); }
    __forceinline operator int64 ( ) const { return std::numeric_limits<int64>::min(); }
    __forceinline operator uint64( ) const { return std::numeric_limits<uint64>::min(); }
    __forceinline operator int32 ( ) const { return std::numeric_limits<int32>::min(); }
    __forceinline operator uint32( ) const { return std::numeric_limits<uint32>::min(); }
    __forceinline operator int16 ( ) const { return std::numeric_limits<int16>::min(); }
    __forceinline operator uint16( ) const { return std::numeric_limits<uint16>::min(); }
    __forceinline operator int8  ( ) const { return std::numeric_limits<int8>::min(); }
    __forceinline operator uint8 ( ) const { return std::numeric_limits<uint8>::min(); }
#ifndef _WIN32
    __forceinline operator size_t( ) const { return std::numeric_limits<size_t>::min(); }
#endif

  } neg_inf MAYBE_UNUSED;

  static struct PosInfTy
  {
    __forceinline operator double( ) const { return std::numeric_limits<double>::infinity(); }
    __forceinline operator float ( ) const { return std::numeric_limits<float>::infinity(); }
    __forceinline operator int64 ( ) const { return std::numeric_limits<int64>::max(); }
    __forceinline operator uint64( ) const { return std::numeric_limits<uint64>::max(); }
    __forceinline operator int32 ( ) const { return std::numeric_limits<int32>::max(); }
    __forceinline operator uint32( ) const { return std::numeric_limits<uint32>::max(); }
    __forceinline operator int16 ( ) const { return std::numeric_limits<int16>::max(); }
    __forceinline operator uint16( ) const { return std::numeric_limits<uint16>::max(); }
    __forceinline operator int8  ( ) const { return std::numeric_limits<int8>::max(); }
    __forceinline operator uint8 ( ) const { return std::numeric_limits<uint8>::max(); }
#ifndef _WIN32
    __forceinline operator size_t( ) const { return std::numeric_limits<size_t>::max(); }
#endif
  } inf MAYBE_UNUSED, pos_inf MAYBE_UNUSED;

  static struct NaNTy
  {
    __forceinline operator double( ) const { return std::numeric_limits<double>::quiet_NaN(); }
    __forceinline operator float ( ) const { return std::numeric_limits<float>::quiet_NaN(); }
  } nan MAYBE_UNUSED;

  static struct UlpTy
  {
    __forceinline operator double( ) const { return std::numeric_limits<double>::epsilon(); }
    __forceinline operator float ( ) const { return std::numeric_limits<float>::epsilon(); }
  } ulp MAYBE_UNUSED;

  static struct PiTy
  {
    __forceinline operator double( ) const { return 3.14159265358979323846; }
    __forceinline operator float ( ) const { return 3.14159265358979323846f; }
  } pi MAYBE_UNUSED;

  static struct StepTy {
  } step MAYBE_UNUSED;

  static struct EmptyTy {
  } empty MAYBE_UNUSED;

  static struct FullTy {
  } full MAYBE_UNUSED;
}

#endif
