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

ADD_LIBRARY(sys STATIC
  platform.cpp
  sysinfo.cpp
  filename.cpp
  library.cpp
  thread.cpp
  tasking.cpp
  sync/mutex.cpp
  sync/condition.cpp
  stl/string.cpp
)

LINK_LIBRARIES(sys)

TARGET_LINK_LIBRARIES(sys pthread dl)
