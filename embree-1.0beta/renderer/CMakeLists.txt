## ======================================================================== ##
## Copyright 2009-2011 Intel Corporation                                    ##
##                                                                          ##
## Licensed under the Apache License, Version 2.0 (the "License");          ##
## you may not use this file except in compliance with the License.         ##
## You may obtain a copy of the License at                                  ##
##                                                                          ##
##     http://www.apache.org/licenses/LICENSE-2.0                           ##
##                                                                          ##
## Unless required by applicable law or agreed to in writing, software      ##
## distributed under the License is distributed on an "AS IS" BASIS,        ##
## WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. ##
## See the License for the specific language governing permissions and      ##
## limitations under the License.                                           ##
## ======================================================================== ##

INCLUDE_DIRECTORIES(${CMAKE_CURRENT_SOURCE_DIR})

ADD_LIBRARY(renderer STATIC
  api/api.cpp
  lights/hdrilight.cpp   
  shapes/trianglemesh.cpp   
  shapes/trianglemesh_normals.cpp   
  shapes/trianglemesh_consistent_normals.cpp   
  samplers/sampler.cpp
  samplers/distribution1d.cpp
  samplers/distribution2d.cpp
  integrators/pathtraceintegrator.cpp
  filters/filter.cpp
  renderers/debugrenderer.cpp
  renderers/integratorrenderer.cpp)

TARGET_LINK_LIBRARIES(renderer sys image rtcore)
