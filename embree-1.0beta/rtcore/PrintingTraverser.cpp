#include "PrintingTraverser.h"

namespace embree{

void PrintingTraverser::intersect(const Ray& ray, Hit& hit, int depth) const
{
    subIntersector.ptr->intersect(ray,hit,depth);

    int ints[2];
    float floats[6];
    if(hit) //they overrode boolean cast; how cute
    {
		ints[0] = 0;
		ints[1] = depth;
		floats[0] = ray.org.x;
		floats[1] = ray.org.y;
		floats[2] = ray.org.z;
		floats[3] = ray.dir.x*hit.t;
		floats[4] = ray.dir.y*hit.t;
		floats[5] = ray.dir.z*hit.t;
    }
    else
    {
		ints[0] = 1;
		ints[1] = depth;
		floats[0] = ray.org.x;
		floats[1] = ray.org.y;
		floats[2] = ray.org.z;
		floats[3] = ray.dir.x;
		floats[4] = ray.dir.y;
		floats[5] = ray.dir.z;
    }
    fwrite(&ints,sizeof(int),2,file);
    fwrite(&floats,sizeof(float),6,file);
}

bool PrintingTraverser::occluded (const Ray& ray, int depth) const
{	
    bool res = subIntersector.ptr->occluded(ray, depth);

    int ints[2];
    float floats[6];
    if(res)
    {
		ints[0] = 3;
		ints[1] = depth;
		floats[0] = ray.org.x;
		floats[1] = ray.org.y;
		floats[2] = ray.org.z;
		floats[3] = ray.dir.x;
		floats[4] = ray.dir.y;
		floats[5] = ray.dir.z;
    }
    else
    {
		ints[0] = 2;
		ints[1] = depth;
		floats[0] = ray.org.x;
		floats[1] = ray.org.y;
		floats[2] = ray.org.z;
		floats[3] = ray.dir.x;
		floats[4] = ray.dir.y;
		floats[5] = ray.dir.z;
    }
    fwrite(&ints,sizeof(int),2,file);
    fwrite(&floats,sizeof(float),6,file);
    return res;
}

PrintingTraverser::PrintingTraverser(const Ref<Intersector >& sub, const FileName& fileName) : subIntersector(sub)
{
    file = fopen(fileName.c_str(), "wb");
    if (!file) throw std::runtime_error("cannot open file " + fileName.str());
}

PrintingTraverser::~PrintingTraverser()
{
	int end_sentinel = 9215;
    fwrite(&end_sentinel,sizeof(int),1,file);
    fclose(file);
}

}