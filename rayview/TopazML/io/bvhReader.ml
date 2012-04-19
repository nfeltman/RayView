open Trees;;
open Bvh2;;
open Box3;;

exception BVH_Error of string

let readRefBVH input =
	let readNum = fun () -> Scanf.fscanf input " %i" (fun x -> x) in
	let rec parseNode = fun () ->
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
	in parseNode()

let branchMapFold k b1 b2 = let b = ne_join b1 b2 in ({ kernel = k; boundsB = b }, b)