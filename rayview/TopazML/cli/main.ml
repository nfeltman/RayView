
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

let bvh = MainHelpers.loadBVH !scene_loc !bvh_loc in

print_string "converted. \n";

let counts = (Trees.foldUp (fun _ (l1,b1) (l2,b2) -> (l1 + l2, b1+b2+1)) (fun _ -> (1,0)) bvh) in
print_int (fst counts);
print_string " ";
print_int (snd counts)