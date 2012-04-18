open Trees;;
open Bvh2;;
open Box3;;

exception BVH_Error of string

let rec parseNode (parser : (int -> 'b) -> 'b) : bvh * ne_box3 =
	parser (fun i ->
					match i with
					| 2 -> let kern = parser Kernels.getKernel in
							let (left, leftBox) = parseNode parser in
							let (right, rightBox) = parseNode parser in
							let myBox = ne_join leftBox rightBox in
							(Branch ({ boundsB = myBox; kernel = kern }, left, right), myBox)
					| 3 -> parser
					| t -> raise (BVH_Error ("Bad node type: " ^ (string_of_int t))))

let readBVH input =
	let parser = Scanf.sscanf input " %i" in parseNode;