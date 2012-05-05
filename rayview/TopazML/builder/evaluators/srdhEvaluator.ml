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
	type intersectionCombo = HitNeither | HitBoth | HitOnlyLeft | HitOnlyRight
	
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
	
	let getInteractionType head rest leftFilter =
		if leftFilter head then
			let rec test l = match l with
				| [] -> HitOnlyLeft
				| h:: t -> if leftFilter h then test t else HitBoth
			in test rest
		else
			let rec test l = match l with
				| [] -> HitOnlyRight
				| h:: t -> if leftFilter h then HitBoth else test t
			in test rest
	
	let customSieve failureCase triFilter cRay =
		let rec filterTris oldTris newTris =
			match oldTris with
			| [] -> (match newTris with
						| [] -> PassSimpleRay(cRay.ray)
						| h:: r -> PassCRayN({ ray = cRay.ray; intersectedFirst = h; intersectedRest = r }))
			| h:: t -> if failureCase h then Skip else (filterTris t (if triFilter h then h:: newTris else newTris))
		in filterTris (cRay.intersectedFirst:: cRay.intersectedRest) []
	
	let ffalse a = false
	let errorOut = raise InternalLies
	
	let evaluate_split stack leftAgg rightAgg leftFilter =
		match leftAgg.box, rightAgg.box with
		| (Empty, Empty | Empty, NotEmpty _ | NotEmpty _, Empty) -> raise InternalLies
		| NotEmpty(ne_left), NotEmpty(ne_right) ->
				let leftCenter, rightCenter = (center ne_left), (center ne_right) in
				let leftCloser cRay = Vectors.firstIsCloser leftCenter rightCenter cRay.ray.Vectors.originS in
				let leftFactor, rightFactor = float_of_int leftAgg.count, float_of_int rightAgg.count in
				
				let sure_ltraversal, sure_rtraversal =
					let rec connCount conn sure_ltraversal sure_rtraversal =
						match conn with
						| [] -> sure_ltraversal, sure_rtraversal
						| h:: t -> connCount t (sure_ltraversal + if ne_intersectsSeg ne_left h then 1 else 0) (sure_rtraversal + if ne_intersectsSeg ne_right h then 1 else 0)
					in
					connCount stack.con_s 0 0 in
				match stack.bro_s with
				| [] -> let cost = (float_of_int sure_ltraversal) *. leftFactor +. (float_of_int sure_rtraversal) *. rightFactor in
						{ cost = cost; memo = { kernel = Kernels.UniformRandom; left_sieve = errorOut; right_sieve = errorOut }}
				| broken ->
						let kernel, cost =
							let sure_ltraversal, sure_rtraversal, lf_extra_l, rf_extra_r, ftb_extra_l, ftb_extra_r, btf_extra_l, btf_extra_r
							= ref sure_ltraversal, ref sure_rtraversal, ref 0, ref 0, ref 0, ref 0, ref 0, ref 0 in
							let rec broCount bro =
								match bro with
								| [] -> ()
								| h:: t -> match getInteractionType h.intersectedFirst h.intersectedRest leftFilter with
										| HitNeither -> begin
													if ne_intersectsSeg ne_left h.ray then incr sure_ltraversal;
													if ne_intersectsSeg ne_left h.ray then incr sure_ltraversal end
										| HitBoth -> begin
													incr rf_extra_r;
													incr lf_extra_l;
													if leftCloser h then
														(incr ftb_extra_l; incr btf_extra_r)
													else
														(incr ftb_extra_r; incr btf_extra_l) end
										| HitOnlyLeft -> begin
													incr sure_ltraversal;
													if ne_intersectsSeg ne_right h.ray then
														(incr rf_extra_r; incr (if leftCloser h then btf_extra_r else ftb_extra_r)) end
										| HitOnlyRight -> begin
													incr sure_rtraversal;
													if ne_intersectsSeg ne_left h.ray then
														(incr lf_extra_l; incr (if leftCloser h then ftb_extra_l else btf_extra_l)) end
							in broCount broken;
							let lf_total, rf_total, ftb_total, btf_total, sure_cost =
								(float_of_int !lf_extra_l) *. leftFactor,
								(float_of_int !rf_extra_r) *. rightFactor,
								(float_of_int !ftb_extra_l) *. leftFactor +. (float_of_int !ftb_extra_r) *. rightFactor,
								(float_of_int !btf_extra_l) *. leftFactor +. (float_of_int !btf_extra_r) *. rightFactor,
								(float_of_int !sure_ltraversal) *. leftFactor +. (float_of_int !sure_rtraversal) *. rightFactor
							in
							let (k1, c1), (k2, c2) =
								(if ftb_total < btf_total then Kernels.FrontToBack, ftb_total else Kernels.BackToFront, btf_total),
								(if lf_total < rf_total then Kernels.LeftFirst, lf_total else Kernels.RightFirst, rf_total) in
							if k1 < k2 then (k1, c1 +. sure_cost) else (k2, c2 +. sure_cost)
						in
						let rightFilter = (fun tri -> not (leftFilter tri)) in
						let leftSieve, rightSieve = match kernel with
							| Kernels.UniformRandom -> raise InternalLies
							| Kernels.LeftFirst -> customSieve ffalse leftFilter, customSieve leftFilter rightFilter
							| Kernels.RightFirst -> customSieve rightFilter leftFilter, customSieve ffalse rightFilter
							| Kernels.FrontToBack -> (fun cRay -> customSieve (if leftCloser cRay then ffalse else rightFilter) leftFilter cRay),
									(fun cRay -> customSieve (if (leftCloser cRay) then ffalse else leftFilter) rightFilter cRay)
							| Kernels.BackToFront -> (fun cRay -> customSieve (if not (leftCloser cRay) then ffalse else rightFilter) leftFilter cRay),
									(fun cRay -> customSieve (if not (leftCloser cRay) then ffalse else leftFilter) rightFilter cRay)
						in { cost = cost; memo = { kernel = kernel; left_sieve = leftSieve; right_sieve = rightSieve }}
	;;
	
	let finish_evaluations result unif stack =
		
		{ build_info = result.memo.kernel;
			left_transition = { con_t = stack.con_s; bro_t = stack.bro_s; ray_sieve = result.memo.left_sieve };
			right_transition = { con_t = stack.con_s; bro_t = stack.bro_s; ray_sieve = result.memo.right_sieve };
			build_left_first = true };;
	
end

let getInitialTransition compiler_output = { E.con_t = compiler_output.connected; E.bro_t = compiler_output.broken; E.ray_sieve = fun r -> E.PassCRayN(r) }