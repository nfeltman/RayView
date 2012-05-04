open Ray_compiler;;
open ArrayUtil;;
open Cost_evaluator;;
open Triangle_aggregator;;
open Box3;;
open Build_triangle;;

module E =
struct
	type rayResult = Skip | PassSimpleRay of Vectors.seg3 | PassCRayN of cRay_n
	type raySieve = cRay_n -> rayResult
	
	type uniform_data = unit
	type stack_data = { con_s : Vectors.seg3 list ; bro_s : cRay_n list }
	type transition_data = { con_t : Vectors.seg3 list ; bro_t : cRay_n list; ray_sieve : raySieve }
	type memo_data = { kernel : Kernels.kernelType; left_sieve : raySieve; right_sieve : raySieve }
	type kernel_data = Kernels.kernelType
	
	exception EvaluatingEmptyNode
	let begin_evaluations wholeAgg unif trans =
		match wholeAgg.box with
		| Empty -> raise EvaluatingEmptyNode
		| NotEmpty(ne) ->
				let already_connected = List.filter (Box3.ne_intersectsSeg ne) trans.con_t in
				let connected, broken = List.fold_left (fun (conn, bro) cRay ->
									if Box3.ne_intersectsSeg ne cRay.ray then
										match trans.ray_sieve cRay with
										| Skip -> conn, bro
										| PassSimpleRay(seg) -> seg:: conn, bro
										| PassCRayN(cray) -> conn, cray:: bro
									else conn, bro
						) (already_connected,[]) trans.bro_t in
				{ con_s = connected; bro_s = broken }
	
	exception InternalLies
	
	let customSieve failureCase triFilter cRay =
		let rec filterTris oldTris newTris =
			match oldTris with
			| [] -> (match newTris with
						| [] -> PassSimpleRay(cRay.ray)
						| tris -> PassCRayN({ ray = cRay.ray; intersectedTris = tris }))
			| h:: t -> if failureCase h then Skip else (filterTris t (if triFilter h then h:: newTris else newTris))
		in filterTris cRay.intersectedTris []
		
	let ffalse a = false
	
	let evaluate_split stack leftAgg rightAgg leftFilter =
		match leftAgg.box, rightAgg.box with
		| (Empty, Empty | Empty, NotEmpty _ | NotEmpty _, Empty) -> raise InternalLies
		| NotEmpty(ne_left), NotEmpty(ne_right) ->
				let leftCenter, rightCenter = (center ne_left), (center ne_right) in
				let kernel, cost = Kernels.UniformRandom, 0.0 in
				let rightFilter = (fun tri -> not (leftFilter tri)) in
				let leftCloser cRay = Vectors.firstIsCloser leftCenter rightCenter cRay.ray.Vectors.originS in
				let leftSieve, rightSieve = match kernel with
					| Kernels.UniformRandom -> raise InternalLies
					| Kernels.LeftFirst -> customSieve ffalse leftFilter, customSieve leftFilter rightFilter
					| Kernels.RightFirst -> customSieve rightFilter leftFilter, customSieve ffalse rightFilter
					| Kernels.FrontToBack -> (fun cRay -> customSieve (if leftCloser cRay then ffalse else rightFilter) leftFilter cRay),
							(fun cRay -> customSieve (if (leftCloser cRay) then ffalse else leftFilter) rightFilter cRay)
					| Kernels.BackToFront ->  (fun cRay -> customSieve (if not (leftCloser cRay) then ffalse else rightFilter) leftFilter cRay),
							(fun cRay -> customSieve (if not (leftCloser cRay) then ffalse else leftFilter) rightFilter cRay)
				in { cost = cost; memo = { kernel = kernel; left_sieve = leftSieve; right_sieve = rightSieve }}
	;;
	
	let finish_evaluations result unif stack =
		
		{ build_info = result.memo.kernel;
			left_transition = { con_t = stack.con_s; bro_t = stack.bro_s; ray_sieve = result.memo.left_sieve };
			right_transition = { con_t = stack.con_s; bro_t = stack.bro_s; ray_sieve = result.memo.right_sieve };
			build_left_first = true };;
	
end

let getInitialTransition compiler_output = { E.con_t = compiler_output.connected; E.bro_t = compiler_output.broken; E.ray_sieve = fun r -> E.PassCRayN(r)}