open Bvh2;;

let output_loc = ref ""
let rays_loc = ref ""
let bvh_loc = ref ""
let scene_loc = ref ""
let doEvalBVH = ref false;;

let arglist = [("-s", Arg.Set_string(scene_loc), "Scene file");
	("-b", Arg.Set_string(bvh_loc), "BVH file");
	("-r", Arg.Set_string(rays_loc), "Ray file");
	("-o", Arg.Set_string(output_loc), "Output file");
	("-evalbvh", Arg.Set(doEvalBVH), "Perf eval run")]
in Arg.parse arglist (fun anons -> ()) "Topaz: research-oriented build and evaluation software for bounding volume hierarchies. OCaml port.";;


print_endline !scene_loc;
print_endline !bvh_loc;
print_endline !rays_loc;

let bvh = MainHelpers.loadBVH !scene_loc !bvh_loc in

print_string "converted. \n";

let (leafCount, branchCount) = (Trees.foldUp (fun _ (l1,b1) (l2,b2) -> (l1 + l2, b1+b2+1)) (fun _ -> (1,0)) bvh) in
Printf.printf "counts: %d %d \n" leafCount branchCount;

let rays = MainHelpers.loadRays !rays_loc in

print_endline "I read the rays!";

let costs, acc = Bvh2.measureCost bvh rays in

Printf.printf "costs: %f %f %f \n" costs.spineCost costs.sideCost costs.missCost;
Printf.printf "accuracy: %d %d %d %d \n" acc.th_mh acc.tm_mm acc.tm_mh acc.th_mm;