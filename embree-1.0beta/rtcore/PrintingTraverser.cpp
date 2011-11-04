#include "PrintingTraverser.h"

namespace embree{

void PrintingTraverser::intersect(const Ray& ray, Hit& hit, int depth) const
{
    subIntersector.ptr->intersect(ray,hit,depth);

    //BEWARE: it's multi-threaded; only use one fprintf per ray
    if(hit) //they overrode boolean cast; how cute
    {
        float dist = hit.t;
        fprintf(file,"i-hit %i %f %f %f %f %f %f\n",depth,ray.org.x,ray.org.y,ray.org.z,ray.dir.x*dist,ray.dir.y*dist,ray.dir.z*dist);
    }
    else
    {
        fprintf(file,"i-mis %i %f %f %f %f %f %f\n",depth,ray.org.x,ray.org.y,ray.org.z,ray.dir.x,ray.dir.y,ray.dir.z);
    }
}

bool PrintingTraverser::occluded (const Ray& ray, int depth) const
{
    bool res = subIntersector.ptr->occluded(ray, depth);
    if(res)
    {
        fprintf(file,"o-con %i %f %f %f %f %f %f\n",depth,ray.org.x,ray.org.y,ray.org.z,ray.dir.x,ray.dir.y,ray.dir.z);
    }
    else
    {
        fprintf(file,"o-bro %i %f %f %f %f %f %f\n",depth,ray.org.x,ray.org.y,ray.org.z,ray.dir.x,ray.dir.y,ray.dir.z);
    }
    return res;
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