
let loadTris scene_loc=
	let scene_channel = open_in scene_loc in
	let tris = ObjReader.readOBJ scene_channel in
	close_in scene_channel;
	tris

let loadBVH bvh_loc=
	let bvh_channel = open_in bvh_loc in
	let indexed_bvh = BvhReader.readRefBVH_text bvh_channel in
	close_in bvh_channel;	
	indexed_bvh

let loadRays ray_loc =
	let ray_channel = open_in_bin ray_loc in
	let rays = RayReader.readRays ray_channel in
	close_in ray_channel;
	rays;;