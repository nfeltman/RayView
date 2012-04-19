open Trees;;
open Vectors;;
open Box3;;
open Kernels;;

type bvhBranch = { boundsB : Box3.ne_box3; kernel : kernelType }
type bvhLeaf = { boundsL : Box3.ne_box3 }
type bvh = (bvhBranch, bvhLeaf) Trees.tree
type refBVH = (Kernels.kernelType, int array) Trees.tree

let bounds node = match node with
	| Branch(content, left, right) -> content.boundsB
	| Leaf(content) -> content.boundsL

let incCost (h, c1, c2) = if h then (true, c1 +. 1.0, c2) else (false, 0.0, c2)
let blendCost p1 c1 c2 = c1 *. p1 +. c2 *. (1.0 -. p1)

let rec measureCost node seg = match node with
	| Leaf(leaf) -> if Box3.ne_intersectsSeg leaf.boundsL seg then (true, 1.0, 0.0) else (false, 0.0, 1.0)
	| Branch(branch, left, right) -> if not (Box3.ne_intersectsSeg branch.boundsB seg) then (false, 0.0, 1.0) else
				let eval firstChild secondChild =
					let (hits, c1, c2) = (measureCost firstChild seg) in
					if hits then (true, c1, c2) else (measureCost secondChild seg) in
				incCost( match
						match branch.kernel with
						| UniformRandom -> 0.5
						| LeftFirst -> 1.0
						| RightFirst -> 0.0
						| FrontToBack -> if length2 ((center (bounds left)) - seg.originS) <= length2 ((center (bounds right)) - seg.originS) then 0.0 else 1.0
						| BackToFront -> if length2 ((center (bounds left)) - seg.originS) > length2 ((center (bounds right)) - seg.originS) then 0.0 else 1.0
						with
						| 0.0 -> eval right left
						| 1.0 -> eval left right
						| pLeft ->
								let (lHits, lc1, lc2) = (measureCost left seg)in
								let (rHits, rc1, rc2) = (measureCost right seg) in
								(lHits || rHits, blendCost pLeft lc1 rc1, blendCost pLeft lc2 rc2));;
