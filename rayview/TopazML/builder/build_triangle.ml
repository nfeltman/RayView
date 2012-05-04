open Vectors;;

type bTri = { build_index : int; obj_index : int; tri : Triangle.triangle; center : vec3 }

let defaultTri = { build_index = -1; obj_index = -1; tri = (Vectors.origin, Vectors.origin, Vectors.origin); center = Vectors.origin } ;;
let getCenter bTri = bTri.center
let getBuildIndex bTri = bTri.build_index
let getObjIndex bTri = bTri.obj_index
let getBounds bTri = Box3.fromTri bTri.tri
let getTriangle bTri = bTri.tri

let createBuildTriangleArray tris =
	let calcCenter a b c =
		if (a < b) != (a < c) then (b +.c) *. 0.5
		else if (b < a) != (b < c) then (a +.c) *. 0.5
		else (a +.b) *. 0.5
	in
	Array.mapi (fun i (p1, p2, p3) ->
					{ build_index = i; obj_index = i; tri = (p1, p2, p3); center = { x = calcCenter p1.x p2.x p3.x; y = calcCenter p1.y p2.y p3.y; z = calcCenter p1.z p2.z p3.z } }) tris

let createBuildTriangleList tris =
	let calcCenter a b c =
		if (a < b) != (a < c) then (b +.c) *. 0.5
		else if (b < a) != (b < c) then (a +.c) *. 0.5
		else (a +.b) *. 0.5
	in
	let _, list = Array.fold_left (fun (i, list) (p1, p2, p3) -> (succ i),
						{ build_index = i; obj_index = i; tri = (p1, p2, p3); center = { x = calcCenter p1.x p2.x p3.x; y = calcCenter p1.y p2.y p3.y; z = calcCenter p1.z p2.z p3.z } }:: list )
			(0,[]) tris
	in list