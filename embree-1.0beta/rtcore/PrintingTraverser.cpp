#include "PrintingTraverser.h"

namespace embree{

void PrintingTraverser::intersect(const Ray& ray, Hit& hit) const
{
	std::cout << "trolol" << std::endl;
	subIntersector.ptr->intersect(ray,hit);
}

bool PrintingTraverser::occluded (const Ray& ray) const
{
	return subIntersector.ptr->occluded(ray);
}

}