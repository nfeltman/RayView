open Bvh2;;
open Printf;;

module SAHBuilder = General_builder.Builder(SahEvaluator.E)(Sah50Factory.F);;
module HelperBuilder = General_builder.Builder(SahEvaluator.E)(HelperFactory.F);;
module SRDHBuilder = General_builder.Builder(SrdhEvaluator.E)(SrdhFactory.F);;

exception TopazException of string
type 'a param = Supplied of 'a | Unsupplied

let runTopaz () =
	
	let output_loc = ref Unsupplied in
	let build_rays_loc = ref Unsupplied in
	let eval_rays_loc = ref Unsupplied in
	let bvh_loc = ref Unsupplied in
	let scene_loc = ref Unsupplied in
	let doEvalBVH = ref false in
	let doBuildBVH = ref false in
	
	let arglist = [("-s", Arg.String(fun s -> scene_loc := Supplied(s)), "Scene file");
		("-b", Arg.String(fun s -> bvh_loc := Supplied(s)), "BVH file");
		("-br", Arg.String(fun s -> build_rays_loc := Supplied(s)), "Build Ray file");
		("-er", Arg.String(fun s -> eval_rays_loc := Supplied(s)), "Eval Ray file");
		("-o", Arg.String(fun s -> output_loc := Supplied(s)), "Output file");
		("-evalbvh", Arg.Set(doEvalBVH), "Perf eval run");
		("-buildbvh", Arg.Set(doBuildBVH), "Perf eval run")]
	in
	Arg.parse arglist (fun anons -> ()) "Topaz: research-oriented build and evaluation software for bounding volume hierarchies. OCaml port.";
	
	match !doEvalBVH, !doBuildBVH with
	| false, false -> raise (TopazException "Need one of -doEvalBVH or -doBuildBVH")
	| true, true -> raise (TopazException "Cannot do more than one of -doEvalBVH and -doBuildBVH")
	| true, false -> begin
				match !scene_loc, !bvh_loc, !eval_rays_loc, !output_loc with
				| Supplied(scene_loc), Supplied(bvh_loc), Supplied(eval_rays_loc), Supplied(output_loc) ->
						begin
							print_endline scene_loc;
							print_endline bvh_loc;
							print_endline eval_rays_loc;
							let timer = Timer.create() in
							
							let tris = MainHelpers.loadTris scene_loc in
							printf "Triangles loaded. Time = %f \n" (Timer.reset_s timer); flush_all();
							
							let indexed_bvh = MainHelpers.loadBVH bvh_loc in
							printf "Indexed BVH loaded. Time = %f \n" (Timer.reset_s timer); flush_all();
							
							let bvh = Trees.foldMapTree BvhReader.branchMapFold (BvhReader.leafMapFold (Array.get tris)) indexed_bvh in
							printf "BVH built. Time = %f \n" (Timer.reset_s timer); flush_all();
							
							let (leafCount, branchCount) = (Trees.foldUp (fun _ (l1, b1) (l2, b2) -> (l1 + l2, b1 + b2 +1)) (fun _ -> (1,0)) bvh) in
							Printf.printf "counts: %d %d \n" leafCount branchCount; flush_all();
							
							let rays = MainHelpers.loadRays eval_rays_loc in
							printf "Rays loaded! Time = %f \n" (Timer.reset_s timer); flush_all();
							
							let costs, acc = Bvh2.measureCost bvh rays in
							
							printf "Measuring done! %f \n" (Timer.reset_s timer); flush_all();
							
							printf "costs: %f %f %f \n" costs.spineCost costs.sideCost costs.missCost;
							print_endline "th_mh tm_mm tm_mh th_mm";
							printf "accuracy: %d %d %d %d \n" acc.th_mh acc.tm_mm acc.tm_mh acc.th_mm
						end
				| _ -> raise (TopazException "Need scene, bvh, eval rays, and output specified.")
			end
	| false, true -> begin
				match !scene_loc, !build_rays_loc, !output_loc with
				| Supplied(scene_loc), Supplied(build_rays_loc), Supplied(output_loc) ->
						begin
							print_endline scene_loc;
							print_endline build_rays_loc;
							print_endline output_loc;
							let timer = Timer.create() in
							
							let tris = MainHelpers.loadTris scene_loc in
							printf "Triangles loaded. Time = %f \n" (Timer.reset_s timer); flush_all();
							
							let rays = MainHelpers.loadRays build_rays_loc in
							printf "Rays loaded! Time = %f \n" (Timer.reset_s timer); flush_all();
							
							let bTris = Build_triangle.createBuildTriangleList tris in
							let helperBVH = HelperBuilder.build_bvh { General_builder.leaf_size = 4 } bTris () () in
							printf "Helper bvh built. Time = %f \n" (Timer.reset_s timer); flush_all();
							let compiledRays = RayCompiler.compileRays helperBVH (Array.map fst rays) in
							printf "Compiled rays. Time = %f \n" (Timer.reset_s timer); flush_all();
							
							let ml_ref_bvh = SRDHBuilder.build_bvh { General_builder.leaf_size = 1 } bTris () (SrdhEvaluator.getInitialTransition compiledRays) in
							printf "BVH built from scratch. Time = %f \n" (Timer.reset_s timer); flush_all();
							
							let ml_bvh = Trees.foldMapTree BvhReader.branchMapFold (BvhReader.leafMapFold (Array.get tris)) ml_ref_bvh in
							printf "BVH built from indeces. Time = %f \n" (Timer.reset_s timer); flush_all();
							
							let costs2, acc2 = Bvh2.measureCost ml_bvh rays in
							printf "Measuring done! %f \n" (Timer.reset_s timer); flush_all();
							
							printf "costs ml: %f %f %f \n" costs2.spineCost costs2.sideCost costs2.missCost;
							print_endline "th_mh tm_mm tm_mh th_mm";
							printf "accuracy ml: %d %d %d %d \n" acc2.th_mh acc2.tm_mm acc2.tm_mh acc2.th_mm
						end
				| _ -> raise (TopazException "Need scene, bvh, build rays, and output specified.")
			end;;
			
runTopaz()
			