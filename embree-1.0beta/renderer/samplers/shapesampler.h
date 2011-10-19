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

#ifndef __EMBREE_SHAPESAMPLER_H__
#define __EMBREE_SHAPESAMPLER_H__

#include "default.h"

/*! \file shapesampler.h Implements sampling functions for different
 *  geometric shapes. */

namespace embree
{
  ////////////////////////////////////////////////////////////////////////////////
  /// Sampling of Sphere
  ////////////////////////////////////////////////////////////////////////////////

  /*! Uniform sphere sampling. */
  __forceinline Sample3f uniformSampleSphere(const float& u, const float& v) {
    const float phi = 2.0f * float(pi) * u;
    const float cosTheta = 1.0f - 2.0f * v, sinTheta = 2.0f * sqrt(v * (1.0f - v));
    return Sample3f(Vec3f(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta), 1.0f/(float(4)*float(pi)));
  }

  /*! Computes the probability density for the uniform shere sampling. */
  __forceinline float uniformSampleSpherePDF() {
    return 1.0f/(float(4)*float(pi));
  }

  /*! Cosine weighted sphere sampling. Up direction is the z direction. */
  __forceinline Sample3f cosineSampleSphere(const float& u, const float& v) {
    const float phi = 2.0f * float(pi) * u;
    const float vv = 2.0f*(v-0.5f);
    const float cosTheta = sign(vv)*sqrt(abs(vv)), sinTheta = cos2sin(cosTheta);
    return Sample3f(Vec3f(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta), 2.0f*cosTheta/float(pi));
  }

  /*! Computes the probability density for the cosine weighted sphere sampling. */
  __forceinline float cosineSampleSpherePDF(const Vec3f& s) {
    return 2.0f*abs(s.z)/float(pi);
  }

  /*! Cosine weighted sphere sampling. Up direction is provided as argument. */
  __forceinline Sample3f cosineSampleSphere(const float& u, const float& v, const Vec3f& N) {
    Sample3f s = cosineSampleSphere(u,v);
    return Sample3f(frame(N)*Vec3f(s),s.pdf);
  }

  /*! Computes the probability density for the cosine weighted sphere sampling. */
  __forceinline float cosineSampleSpherePDF(const Vec3f& s, const Vec3f& N) {
    return 2.0f*abs(dot(s,N))/float(pi);
  }

  ////////////////////////////////////////////////////////////////////////////////
  /// Sampling of Hemisphere
  ////////////////////////////////////////////////////////////////////////////////

  /*! Uniform hemisphere sampling. Up direction is the z direction. */
  __forceinline Sample3f uniformSampleHemisphere(const float& u, const float& v) {
    const float phi = 2.0f * float(pi) * u;
    const float cosTheta = v, sinTheta = cos2sin(v);
    return Sample3f(Vec3f(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta), 1.0f/(2.0f*float(pi)));
  }

  /*! Computes the probability density for the uniform hemisphere sampling. */
  __forceinline float uniformSampleHemispherePDF(const Vec3f& s) {
    return select(s.z < 0.0f, 0.0f, 1.0f/(2.0f*float(pi)));
  }

  /*! Uniform hemisphere sampling. Up direction is provided as argument. */
  __forceinline Sample3f uniformSampleHemisphere(const float& u, const float& v, const Vec3f& N) {
    Sample3f s = uniformSampleHemisphere(u,v);
    return Sample3f(frame(N)*Vec3f(s),s.pdf);
  }

  /*! Computes the probability density for the uniform hemisphere sampling. */
  __forceinline float uniformSampleHemispherePDF(const Vec3f& s, const Vec3f& N) {
    return select(dot(s,N) < 0.0f, 0.0f, 1.0f/(2.0f*float(pi)));
  }

  /*! Cosine weighted hemisphere sampling. Up direction is the z direction. */
  __forceinline Sample3f cosineSampleHemisphere(const float& u, const float& v) {
    const float phi = 2.0f * float(pi) * u;
    const float cosTheta = sqrt(v), sinTheta = sqrt(1.0f - v);
    return Sample3f(Vec3f(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta), cosTheta/float(pi));
  }

  /*! Computes the probability density for the cosine weighted hemisphere sampling. */
  __forceinline float cosineSampleHemispherePDF(const Vec3f& s) {
    return select(s.z < 0.0f, 0.0f, s.z/float(pi));
  }

  /*! Cosine weighted hemisphere sampling. Up direction is provided as argument. */
  __forceinline Sample3f cosineSampleHemisphere(const float& u, const float& v, const Vec3f& N) {
    Sample3f s = cosineSampleHemisphere(u,v);
    return Sample3f(frame(N)*Vec3f(s),s.pdf);
  }

  /*! Computes the probability density for the cosine weighted hemisphere sampling. */
  __forceinline float cosineSampleHemispherePDF(const Vec3f& s, const Vec3f& N) {
    return select(dot(s,N) < 0.0f, 0.0f, dot(s,N)/float(pi));
  }

  /*! Samples hemisphere with power cosine distribution. Up direction
   *  is the z direction. */
  __forceinline Sample3f powerCosineSampleHemisphere(const float& u, const float& v, float exp) {
    const float phi = 2.0f * float(pi) * u;
    const float cosTheta = pow(v,1.0f/(exp+1.0f));
    const float sinTheta = cos2sin(cosTheta);
    return Sample3f(Vec3f(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta), (exp+1.0f)*pow(cosTheta,exp)*0.5f/float(pi));
  }

  /*! Computes the probability density for the power cosine sampling of the hemisphere. */
  __forceinline float powerCosineSampleHemispherePDF(const Vec3f& s, float exp) {
    return select(s.z < 0.0f, 0.0f, (exp+1.0f)*pow(s.z,exp)*0.5f/float(pi));
  }

  /*! Samples hemisphere with power cosine distribution. Up direction
   *  is provided as argument. */
  __forceinline Sample3f powerCosineSampleHemisphere(const float& u, const float& v, const Vec3f& N, float exp) {
    Sample3f s = powerCosineSampleHemisphere(u,v,exp);
    return Sample3f(frame(N)*Vec3f(s),s.pdf);
  }

  /*! Computes the probability density for the power cosine sampling of the hemisphere. */
  __forceinline float powerCosineSampleHemispherePDF(const Vec3f& s, const Vec3f& N, float exp) {
    return select(dot(s,N) < 0.0f, 0.0f, (exp+1.0f)*pow(dot(s,N),exp)*0.5f/float(pi));
  }

  ////////////////////////////////////////////////////////////////////////////////
  /// Sampling of Spherical Cone
  ////////////////////////////////////////////////////////////////////////////////

  /*! Uniform sampling of spherical cone. Cone direction is the z
   *  direction. */
  __forceinline Sample3f uniformSampleCone(const float& u, const float& v, const float& angle) {
    const float phi = 2.0f * float(pi) * u;
    const float cosTheta = 1.0f - v*(1.0f - cos(angle));
    const float sinTheta = cos2sin(cosTheta);
    return Sample3f(Vec3f(cos(phi) * sinTheta, sin(phi) * sinTheta, cosTheta), rcp(4.0f*float(pi)*sqr(sin(0.5f*angle))));
  }

  /*! Computes the probability density of uniform spherical cone sampling. */
  __forceinline float uniformSampleConePDF(const Vec3f& s, const float& angle) {
    return select(s.z < cos(angle), 0.0f, rcp(4.0f*float(pi)*sqr(sin(0.5f*angle))));
  }

  /*! Uniform sampling of spherical cone. Cone direction is provided as argument. */
  __forceinline Sample3f uniformSampleCone(const float& u, const float& v, const float& angle, const Vec3f& N) {
    Sample3f s = uniformSampleCone(u,v,angle);
    return Sample3f(frame(N)*Vec3f(s),s.pdf);
  }

  /*! Computes the probability density of uniform spherical cone sampling. */
  __forceinline float uniformSampleConePDF(const Vec3f& s, const float& angle, const Vec3f& N) {
    return select(dot(s,N) < cos(angle), 0.0f, rcp(4.0f*float(pi)*sqr(sin(0.5f*angle))));
  }

  ////////////////////////////////////////////////////////////////////////////////
  /// Sampling of Triangle
  ////////////////////////////////////////////////////////////////////////////////

  /*! Uniform sampling of triangle. */
  __forceinline Vec3f uniformSampleTriangle(const float& u, const float& v, const Vec3f& A, const Vec3f& B, const Vec3f& C) {
    float su = sqrt(u);
    return Vec3f(C+(1.0f-su)*(A-C)+(v*su)*(B-C));
  }
}

#endif
