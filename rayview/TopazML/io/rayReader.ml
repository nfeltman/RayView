open Vectors;;

exception RayReadError of string

let readRays input =
	let readInt() = Util.read_Int32 input in
	let header = readInt() in
	if header <> 1234 then raise (RayReadError (Printf.sprintf "Bad header. Expected 1234, got %d" header));
	let readsT = match readInt() with
		| 1 -> true
		| 2 -> false
		| _ -> raise (RayReadError "Bad version number.") in
	let numRays = readInt() in 
	let rays = Array.make numRays ({ originS = origin; difference = origin }, false) in
	for i = 0 to pred numRays do
		let rayType = readInt() in
		let origin = readVector input in 
		let diff = readVector input in
		let t = if readsT then Util.read_single input else 0.0 in
		if readInt() <> 0 then raise (RayReadError "Bad ray footer.");
		rays.(i) <- (match rayType with
			| (1 |5) -> { originS = origin; difference = diff }
			| _ -> raise (RayReadError "Unexpected ray type.")), 
			t < 10000.0
	done;
	rays