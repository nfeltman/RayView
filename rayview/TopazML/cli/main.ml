
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

let scene_channel = open_in !scene_loc in
let readObj = ObjReader.readOBJ scene_channel in
close_in scene_channel; 
print_endline "Number Triangles: "; print_int (Array.length readObj); print_newline();
let scene_channel = open_in !bvh_loc in
let readObj = ObjReader.readOBJ scene_channel in
readObj