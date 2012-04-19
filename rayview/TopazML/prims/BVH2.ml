open Trees;;
open Vectors;;
open Box3;;
open Kernels;;

type bvhBranch = { boundsB : Box3.ne_box3; kernel : kernelType }
type bvhLeaf = { boundsL : Box3.ne_box3; prims : Triangle.triangle array }
type bvh = (bvhBranch, bvhLeaf) Trees.tree
type refBVH = (Kernels.kernelType, int array) Trees.tree

type agreementReport = { th_mh : int ; th_mm : int ; tm_mh : int ; tm_mm : int }
type traversalCost = { spineCost : float; sideCost : float; missCost : float }

let bounds node = match node with
	| Branch(content, left, right) -> content.boundsB
	| Leaf(content) -> content.boundsL

type rayCost = Hits of float * float | Misses of float

let incCost cost = match cost with
	| Hits(c1, c2) -> Hits( c1 +. 1.0, c2)
	| Misses(c) -> Misses(c +.1.0)

let measureCost root segs =
	let intersectRay seg : rayCost =
		let rec intersectNode node = match node with
			| Leaf(leaf) -> if Box3.ne_intersectsSeg leaf.boundsL seg then Hits(1.0, 0.0) else Misses 1.0
			| Branch(branch, left, right) ->
					if not (Box3.ne_intersectsSeg branch.boundsB seg) then Misses 1.0 else
						let detEval firstChild secondChild =
							match intersectNode firstChild with
							| Hits(c1, c2) -> Hits(c1, c2)
							| Misses(c) -> match intersectNode secondChild with
									| Hits(c1, c2) -> Hits(c1, c +.c2)
									| Misses(c2) -> Misses(c +.c2)
						in
						incCost(match
								match branch.kernel with
								| UniformRandom -> 0.5
								| LeftFirst -> 1.0
								| RightFirst -> 0.0
								| FrontToBack -> if length2 ((center (bounds left)) - seg.originS) <= length2 ((center (bounds right)) - seg.originS) then 0.0 else 1.0
								| BackToFront -> if length2 ((center (bounds left)) - seg.originS) > length2 ((center (bounds right)) - seg.originS) then 0.0 else 1.0
								with
								| 0.0 -> detEval right left
								| 1.0 -> detEval left right
								| pLeft -> let blend c1 c2 = c1 *. pLeft +. c2 *. (1.0 -. pLeft) in
										match (intersectNode left), (intersectNode right) with
										| (Hits(sp1, si1), Hits(sp2, si2)) -> Hits(blend sp1 sp2, blend si1 si2)
										| (Hits(sp, si), Misses(mi)) -> Hits(sp, blend si (si +. mi))
										| (Misses(mi), Hits(sp, si)) -> Hits(sp, blend (mi +. si) si)
										| (Misses(c1), Misses(c2)) -> Misses(c1 +. c2))
		in intersectNode root
	in
	let (spine, side, miss) = (ref 0.0, ref 0.0, ref 0.0) in
	let (th_mh, th_mm, tm_mh, tm_mm) = (ref 0, ref 0, ref 0, ref 0) in
	Array.iter ( fun (segment, manta_hits) ->
					match intersectRay segment with
					| Hits(raySpine, raySide) ->
							spine := !spine +. raySpine;
							side := !side +. raySide;
							incr(if manta_hits then th_mh else th_mm)
					| Misses (rayMiss) ->
							miss := !miss +. rayMiss;
							incr(if manta_hits then tm_mh else tm_mm)
		) segs;
	({ spineCost = !spine; sideCost = !side; missCost = !miss },
		{ th_mh = !th_mh ; th_mm = !th_mm ; tm_mh = !tm_mh ; tm_mm = !tm_mm })