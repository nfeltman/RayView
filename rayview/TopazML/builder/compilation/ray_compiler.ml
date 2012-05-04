
open Box3;;
open Triangle;;
open Trees;;
open Build_triangle;;

type cRay_n = { ray : Vectors.seg3 ; intersectedTris : Build_triangle.bTri list }

type ray_compile_output = { connected : Vectors.seg3 list ; broken : cRay_n list }

let compileRays bvh rays =
	let connectedList, brokenList =
		Array.fold_left begin fun (connectedList, brokenList) ray ->
						let rec isect node intersectedList =
							match node with
							| Leaf(bounds, tris) ->
									if ne_intersectsSeg bounds ray then
										Array.fold_left (fun list bTri -> if intersectsSegment (getTriangle bTri) ray then bTri:: list else list) intersectedList tris
									else intersectedList
							| Branch(bounds, left, right) ->
									if ne_intersectsSeg bounds ray then
										isect right (isect left intersectedList)
									else intersectedList
						in
						match isect bvh [] with
						| [] -> ray :: connectedList, brokenList
						| list -> connectedList, { intersectedTris = list; ray = ray }:: brokenList
			end ([],[]) rays
	in
	{ connected = connectedList; broken = brokenList }