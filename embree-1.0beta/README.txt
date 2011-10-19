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

Embree is a collection of high-performance ray tracing kernels,
developed at Intel Labs. The kernels are optimized for photo-realistic
rendering on the latest Intel® processors with support for SSE and AVX
instructions. In addition to the ray tracing kernels, Embree provides
an example photo-realistic rendering engine to demonstrate how the ray
tracing kernels are used in practice and to measure the performance of
the kernels in a realistic application scenario.

Embree is designed for Monte Carlo ray tracing algorithms, where the
vast majority of rays are incoherent. The specific single-ray
traversal kernels in Embree provide the best performance in this
scenario and they are very easy to integrate into existing
applications. The kernels can be used to develop new rendering engines
on top of them, to replace the core of an existing renderer or simply
as a benchmark. Embree is released as Open Source under the Apache 2.0
license.

--- Supported Platforms ---

Embree runs on Windows, Linux and MacOSX, each in 32bit and 64bit
modes. The code compiles with the Intel Compiler, the Microsoft
Compiler and with GCC. We have tested the following configurations:

Linux, GCC 4.4.4, 64 bit
Linux, ICC 11.1, 64 bit
Linux, ICC 12.0, 64 bit
MacOSX 10.6.7, GCC 4.2.1, 32 bit and 64 bit
MacOSX 10.6.7, ICC 11.1, 32 bit and 64 bit
Windows 7, VS 2008, Microsoft Compiler 15, 32 and 64 bit
Windows 7, VS 2008, ICC 11.0, 32 and 64 bit
Windows 7, VS 2010, Microsoft Compiler 16, 32 and 64 bit
Windows 7, VS 2010, ICC 12.0, 32 and 64 bit

Other operating systems and compiler versions will probably work but
may require some adaption of the code. Using the Intel Compiler improves
performance by approximately 10%. Performance also varies across different 
operating systems. Embree is optimized for Intel CPUs supporting SSSE3, 
SSE4.1, SSE4.2 and AVX.

--- Compiling Embree on Linux and MacOSX ---

For compilation under Linux and MacOSX you have to install CMake (for
compilation) the developer version of GLUT (for display) and we
recommend installing the ImageMagick and OpenEXR developer packages
(for reading and writing images).  To compile the code using CMake
create a build directory such as embree/build and execute ccmake
.. inside this directory. 

   mkdir build
   cd build
   ccmake ..

This will open a configuration dialog where you should set the build
mode to “Release”, the SSE version to either SSSE3, SSE4.1, SSE4.2, or
AVX, and possibly enable the ICC compiler for better performance.
Press c (for configure) and g (for generate) to generate a Makefile
and leave the configuration. The code can now be compiled by executing
make. The executable embree will be generated in the build folder.

      make

--- Compiling Embree on Windows ---

For compilation under Windows we recommend using the Visual Studio
2008 or Visual Studio 2010 solution files. You can switch between the
Microsoft Compiler and the Intel Compiler by right clicking on the
project and selecting the compiler. The project compiles with both
compilers in 32 bit and 64 bit mode. We recommend using 64 bit mode
and the Intel Compiler for best performance. When using the Microsoft
Compiler, SSE4 is enabled by default in the codebase. Disabling this
default setting by removing the __SSE4_2__ define in
common/sys/platform.h is necessary when SSE4 is not supported on your
system. 

Before you can run the application under Windows you have to install
the GLUT library provided in the app/freeglut folder. Copy the 64bit
DLL into the x64/Release and x64/Debug folder, and the 32bit DLL into
the Win32/Release and Win32/Debug folder.

--- Running Embree ---

This section describes howto run embree. Execute embree -help for a
complete list of parameters. Embree ships with a few simple test
scenes, each consisting of a scene file (.xml or .obj) and an Embree
command script file (.ecs). The command script file contains command
line parameters that set the camera parameters, lights and render
settings.  The following command line will render the Cornell box
scene with 16 samples per pixel and write the resulting image to the
file cb.tga in the current directory:

    embree -c ../models/cornell_box.ecs -spp 16 -o cb.tga

To interactively display the same scene, enter the following command:

   embree -c ../models/cornell_box.ecs

A window will open and you can control the camera using the mouse and
keyboard. Pressing c in interactive mode outputs the current camera
parameters, pressing r enables or disables the progressive refinement
mode.

The navigation in the interactive display mode follows the camera
orbit model, where the camera revolves around the current center of
interest. The camera navigation assumes the y-axis to point
upwards. If your scene is modeled using the z-axis as up axis we
recommend rotating the scene.

	LMB: Rotate around center of interest
	MMB: Pan
	RMB: Dolly (move camera closer or away from center of interest)
	Strg+LMB: Pick center of interest
	Strg+Shift+LMB: Pick focal distance
	Alt+LMB: Roll camera around view direction
	L: Decrease lens radius by one world space unit
	Shift+L: Increase lens radius by one world space unit

--- Contact ---

Please contact embree_support@intel.com if you have questions related to
Embree or if you want to report a bug.
