
exception OBJ_Error of string
module Vec = Vectors

let readOBJ input =
	let (continue, vCount, vHead, fCount, fHead) = (ref true, ref 0, ref [], ref 0, ref []) in
	while !continue do
		try
			let line = input_line input in
			Scanf.sscanf line " %s"
				(fun token -> match token with
							| "" -> () (* skip empty lines *)
							| "v" -> Scanf.sscanf line " v %f %f %f" (fun x y z -> vHead := { Vec.x = x; Vec.y = y; Vec.z = z } :: !vHead); incr vCount
							| "f" -> Scanf.sscanf line " f %i %i %i" (fun x y z -> fHead := (x - 1, y - 1, z - 1) :: !fHead); incr fCount
							| s -> raise (OBJ_Error("Unrecognized starting token: \'"^s^"\'")) )
		with
			End_of_file -> continue := false
	done;
	let ori = Vec.origin in
	let (verteces, faces) =
		(Array.make !vCount ori, Array.make !fCount (ori, ori, ori)) in
	List.iter (fun v ->
					decr vCount; verteces.(!vCount) <- v
		) !vHead;
	List.iter (fun (v1, v2, v3) ->
					decr fCount; faces.(!fCount) <- (verteces.(v1), verteces.(v2), verteces.(v3))
		) !fHead;
	faces
