
let loadBVH scene_loc bvh_loc =
	let scene_channel = open_in scene_loc in
	let bvh_channel = open_in bvh_loc in
	let tris = ObjReader.readOBJ scene_channel in
	close_in scene_channel;
	print_endline "Number Triangles: "; print_int (Array.length tris); print_newline();
	let indexed_bvh = BvhReader.readRefBVH_text bvh_channel in
	close_in bvh_channel;
	print_endline "Read bvh in.";
	Trees.foldMapTree BvhReader.branchMapFold (BvhReader.leafMapFold (Array.get tris)) indexed_bvh;;

let loadRays ray_loc =
	let ray_channel = open_in_bin ray_loc in
	let rays = RayReader.readRays ray_channel in
	close_in ray_channel;
	rays;;