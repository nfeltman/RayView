
#ifndef __EMBREE_PRINTING_TRAVERSER_H__
#define __EMBREE_PRINTING_TRAVERSER_H__

#include "rtcore.h"

namespace embree
{

	class PrintingTraverser :
		public Intersector
	{
	public:
		PrintingTraverser(const Ref<Intersector >& sub, const FileName& file);
		~PrintingTraverser();
		void intersect(const Ray& ray, Hit& hit) const;
		bool occluded (const Ray& ray) const;
	
	private:
		Ref<Intersector> subIntersector;
		FILE* file;
	};
}

#endif