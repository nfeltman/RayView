open Trees;;
open Bvh2;;
open Box3;;

exception BVH_Error of string

let readRefBVH_text input =
	let scanner = Scanf.fscanf input " %i" in
	let readNum = fun () -> scanner (fun x -> x) in
	let rec parseNode() =
		let nodeType = readNum() in
		match nodeType with
		| 2 -> let kern = Kernels.getKernel (readNum()) in
				let left = parseNode() in
				let right = parseNode() in
				Branch (kern, left, right)
		| 3 -> let count = readNum() in
				let prims = Array.make count (-1) in
				for i = 0 to count -1 do
					prims.(i) <- readNum()
				done; Leaf(prims)
		| t -> raise (BVH_Error ("Bad node type: " ^ (string_of_int t)))
	in
	if readNum() <> 267534 then raise (BVH_Error "Bad Header");
	if readNum() <> 202 then raise (BVH_Error "Bad BVH Type");
	let bvh = parseNode() in
	if readNum() <> 9215 then raise (BVH_Error "Bad Sentinel");
	bvh

(* these functions are for use with Trees.foldMapTree I expect to call the *)
(* leaf case by partially invoking with the triMap                         *)
let branchMapFold k b1 b2 = let b = ne_join b1 b2 in ({ kernel = k; boundsB = b }, b);;
let leafMapFold triMap indeces = let tris = Array.map triMap indeces in
	let b = calcBound tris in ({ prims = tris; boundsL = b }, b)