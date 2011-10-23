#include "PrintingTraverser.h"

namespace embree{

void PrintingTraverser::intersect(const Ray& ray, Hit& hit) const
{
	subIntersector.ptr->intersect(ray,hit);
	fprintf(file,"%f %f %f %f %f %f \n",ray.org.x,ray.org.y,ray.org.z,ray.dir.x,ray.dir.y,ray.dir.z);
}

bool PrintingTraverser::occluded (const Ray& ray) const
{
	return subIntersector.ptr->occluded(ray);
}

PrintingTraverser::PrintingTraverser(const Ref<Intersector >& sub, const FileName& fileName) : subIntersector(sub)
{
	file = fopen(fileName.c_str(), "wb");
    if (!file) throw std::runtime_error("cannot open file " + fileName.str());
}

PrintingTraverser::~PrintingTraverser()
{
    fclose(file);
}

}