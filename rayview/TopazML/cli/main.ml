open Bvh2;;
open Printf;;

let runEval () =
	
	let output_loc = ref "" in
	let rays_loc = ref "" in
	let bvh_loc = ref "" in
	let scene_loc = ref "" in
	let doEvalBVH = ref false in
	
	let arglist = [("-s", Arg.Set_string(scene_loc), "Scene file");
		("-b", Arg.Set_string(bvh_loc), "BVH file");
		("-r", Arg.Set_string(rays_loc), "Ray file");
		("-o", Arg.Set_string(output_loc), "Output file");
		("-evalbvh", Arg.Set(doEvalBVH), "Perf eval run")]
	in
	Arg.parse arglist (fun anons -> ()) "Topaz: research-oriented build and evaluation software for bounding volume hierarchies. OCaml port.";
	
	print_endline !scene_loc;
	print_endline !bvh_loc;
	print_endline !rays_loc;
	let timer = Timer.create() in
	
	let tris = MainHelpers.loadTris !scene_loc in
	printf "Triangles loaded. Time = %f \n" (Timer.reset_s timer); flush_all();
	
	let indexed_bvh = MainHelpers.loadBVH !bvh_loc in
	printf "Indexed BVH loaded. Time = %f \n" (Timer.reset_s timer); flush_all();
	
	let bvh = Trees.foldMapTree BvhReader.branchMapFold (BvhReader.leafMapFold (Array.get tris)) indexed_bvh in
	printf "BVH built. Time = %f \n" (Timer.reset_s timer); flush_all();
	
	let (leafCount, branchCount) = (Trees.foldUp (fun _ (l1, b1) (l2, b2) -> (l1 + l2, b1 + b2 +1)) (fun _ -> (1,0)) bvh) in
	Printf.printf "counts: %d %d \n" leafCount branchCount; flush_all();
	
	let rays = MainHelpers.loadRays !rays_loc in
	printf "Rays loaded! Time = %f \n" (Timer.reset_s timer); flush_all();
	
	let costs, acc = Bvh2.measureCost bvh rays in
	
	printf "Measuring done! %f \n" (Timer.reset_s timer); flush_all();
	
	printf "costs: %f %f %f \n" costs.spineCost costs.sideCost costs.missCost;
	print_endline "th_mh tm_mm tm_mh th_mm";
	printf "accuracy: %d %d %d %d \n" acc.th_mh acc.tm_mm acc.tm_mh acc.th_mm;;

module SAHBuilder = General_builder.Builder(SahEvaluator.E)(Sah50Factory.F);;

let runBuild () =
	
	let output_loc = ref "" in
	let scene_loc = ref "" in
	let doEvalBVH = ref false in
	
	let arglist = [("-s", Arg.Set_string(scene_loc), "Scene file");
		("-o", Arg.Set_string(output_loc), "Output file");
		("-evalbvh", Arg.Set(doEvalBVH), "Perf eval run")]
	in
	Arg.parse arglist (fun anons -> ()) "Topaz: research-oriented build and evaluation software for bounding volume hierarchies. OCaml port.";
	
	print_endline !scene_loc;
	let timer = Timer.create() in
	
	let tris = MainHelpers.loadTris !scene_loc in
	printf "Triangles loaded. Time = %f \n" (Timer.reset_s timer); flush_all();
	SAHBuilder.build_bvh {General_builder.leaf_size = 1} (Build_triangle.createBuildTriangles tris) ()
	;;
	

runBuild()