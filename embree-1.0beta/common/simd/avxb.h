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

#ifndef __EMBREE_AVXB_H__
#define __EMBREE_AVXB_H__

namespace embree
{
  /*! 8-wide AVX bool type. */
  struct avxb
  {
    union { __m256 m256; int32 v[8]; };

    ////////////////////////////////////////////////////////////////////////////////
    /// Constructors, Assignment & Cast Operators
    ////////////////////////////////////////////////////////////////////////////////

    __forceinline avxb           () {}
    __forceinline avxb           ( const avxb& other ) { m256 = other.m256; }
    __forceinline avxb& operator=( const avxb& other ) { m256 = other.m256; return *this; }

    __forceinline avxb( const __m256  input ) : m256(input) {}
    __forceinline avxb( const __m256i input ) : m256(_mm256_castsi256_ps(input)) {}
    __forceinline avxb( const __m256d input ) : m256(_mm256_castpd_ps(input)) {}

    __forceinline avxb( const bool& a ) : m256(a ? _mm256_cmp_ps(_mm256_setzero_ps(), _mm256_setzero_ps(), _CMP_EQ_OQ) : _mm256_setzero_ps()) {}

    __forceinline avxb( const sseb& a ) : m256(_mm256_insertf128_ps(_mm256_castps128_ps256(a),a,1)) {}
    __forceinline avxb( const sseb& a, const sseb& b ) : m256(_mm256_insertf128_ps(_mm256_castps128_ps256(a),b,1)) {}

    __forceinline operator const __m256&( void ) const { return m256; }
    __forceinline operator const __m256i( void ) const { return _mm256_castps_si256(m256); }
    __forceinline operator const __m256d( void ) const { return _mm256_castps_pd(m256); }
    __forceinline operator         sseb ( void ) const { return sseb(_mm256_castps256_ps128(m256)); }

    ////////////////////////////////////////////////////////////////////////////////
    /// Constants
    ////////////////////////////////////////////////////////////////////////////////

    __forceinline avxb( FalseTy ) : m256(_mm256_setzero_ps()) {}
    __forceinline avxb( TrueTy  ) : m256(_mm256_cmp_ps(_mm256_setzero_ps(), _mm256_setzero_ps(), _CMP_EQ_OQ)) {}

    ////////////////////////////////////////////////////////////////////////////////
    /// Properties
    ////////////////////////////////////////////////////////////////////////////////

    __forceinline bool   operator []( const size_t index ) const { assert(index < 8); return (_mm256_movemask_ps(m256) >> index) & 1; }
    __forceinline int32& operator []( const size_t index )       { assert(index < 8); return v[index]; }
  };

  ////////////////////////////////////////////////////////////////////////////////
  /// Unary Operators
  ////////////////////////////////////////////////////////////////////////////////

  __forceinline const avxb operator !( const avxb& a ) { return _mm256_xor_ps(a, avxb(True)); }


  ////////////////////////////////////////////////////////////////////////////////
  /// Binary Operators
  ////////////////////////////////////////////////////////////////////////////////

  __forceinline const avxb operator &( const avxb& a, const avxb& b ) { return _mm256_and_ps(a, b); }
  __forceinline const avxb operator |( const avxb& a, const avxb& b ) { return _mm256_or_ps (a, b); }
  __forceinline const avxb operator ^( const avxb& a, const avxb& b ) { return _mm256_xor_ps(a, b); }

  __forceinline avxb operator &=( avxb& a, const avxb& b ) { return a = a & b; }
  __forceinline avxb operator |=( avxb& a, const avxb& b ) { return a = a | b; }
  __forceinline avxb operator ^=( avxb& a, const avxb& b ) { return a = a ^ b; }


  ////////////////////////////////////////////////////////////////////////////////
  /// Comparison Operators + Select
  ////////////////////////////////////////////////////////////////////////////////

  __forceinline const avxb operator !=( const avxb& a, const avxb& b ) { return _mm256_xor_ps(a, b); }
  __forceinline const avxb operator ==( const avxb& a, const avxb& b ) { return _mm256_xor_ps(_mm256_xor_ps(a,b),avxb(True)); }

  __forceinline const avxb select( const avxb& mask, const avxb& t, const avxb& f ) { return _mm256_blendv_ps(f, t, mask); }


  ////////////////////////////////////////////////////////////////////////////////
  /// Reduction Operations
  ////////////////////////////////////////////////////////////////////////////////

  __forceinline bool reduce_and( const avxb& a ) { return _mm256_movemask_ps(a) == 0xff; }
  __forceinline bool reduce_or ( const avxb& a ) { return !_mm256_testz_ps(a,a); }

  __forceinline bool all       ( const avxb& a ) { return _mm256_movemask_ps(a) == 0xff; }
  __forceinline bool none      ( const avxb& a ) { return _mm256_testz_ps(a,a) != 0; }
  __forceinline bool any       ( const avxb& a ) { return !_mm256_testz_ps(a,a); }

  __forceinline size_t movemask( const avxb& a ) { return _mm256_movemask_ps(a); }

  ////////////////////////////////////////////////////////////////////////////////
  /// Movement/Shifting/Shuffling Functions
  ////////////////////////////////////////////////////////////////////////////////

  template<size_t index> __forceinline const avxb insert (const avxb& a, const sseb& b) { return _mm256_insertf128_ps (a,b,index); }
  template<size_t index> __forceinline const sseb extract(const avxb& a               ) { return _mm256_extractf128_ps(a  ,index); }

  __forceinline avxb unpacklo( const avxb& a, const avxb& b ) { return _mm256_unpacklo_ps(a.m256, b.m256); }
  __forceinline avxb unpackhi( const avxb& a, const avxb& b ) { return _mm256_unpackhi_ps(a.m256, b.m256); }

  template<size_t index> __forceinline const avxb shuffle( const avxb& a ) {
    return _mm256_permute_ps(a, _MM_SHUFFLE(index, index, index, index));
  }

  template<size_t index_0, size_t index_1> __forceinline const avxb shuffle( const avxb& a ) {
    return _mm256_permute2f128_ps(a, a, (index_1 << 4) | (index_0 << 0));
  }

  template<size_t index_0, size_t index_1> __forceinline const avxb shuffle( const avxb& a,  const avxb& b) {
    return _mm256_permute2f128_ps(a, b, (index_1 << 4) | (index_0 << 0));
  }

  template<size_t index_0, size_t index_1, size_t index_2, size_t index_3> __forceinline const avxb shuffle( const avxb& a ) {
    return _mm256_permute_ps(a, _MM_SHUFFLE(index_3, index_2, index_1, index_0));
  }

  template<size_t index_0, size_t index_1, size_t index_2, size_t index_3> __forceinline const avxb shuffle( const avxb& a, const avxb& b ) {
    return _mm256_shuffle_ps(a, b, _MM_SHUFFLE(index_3, index_2, index_1, index_0));
  }

  template<> __forceinline const avxb shuffle<0, 0, 2, 2>( const avxb& b ) { return _mm256_moveldup_ps(b); }
  template<> __forceinline const avxb shuffle<1, 1, 3, 3>( const avxb& b ) { return _mm256_movehdup_ps(b); }
  template<> __forceinline const avxb shuffle<0, 1, 0, 1>( const avxb& b ) { return _mm256_castpd_ps(_mm256_movedup_pd(_mm256_castps_pd(b))); }


  ////////////////////////////////////////////////////////////////////////////////
  /// Output Operators
  ////////////////////////////////////////////////////////////////////////////////

  inline std::ostream& operator<<(std::ostream& cout, const avxb& a) {
    return cout << "<" << a[0] << ", " << a[1] << ", " << a[2] << ", " << a[3] << ", "
                       << a[4] << ", " << a[5] << ", " << a[6] << ", " << a[7] << ">";
  }
}

#endif
