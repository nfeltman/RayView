
type kernelType = UniformRandom | LeftFirst | RightFirst | FrontToBack | BackToFront;;
type bvhBranch = { bounds : ne_box3; kernel : kernelType }
type bvhLeaf = { bounds : ne_box3 }
type bvh = (bvhBranch, bvhLeaf) tree

let bounds node = match node with
	| Branch(content, left, right) -> content.bounds
	| Leaf(content) -> content.bounds

let incCost (h, c1, c2) = if h then (true, c1 +1.0, c2) else (false, 0, c2)
let blendCost p1 c1 c2 = c1 * p1 + c2 * (1.0 - p1)

let rec measureCost node seg = match node with
	| Leaf(content) -> if intersects content.bounds seg then (true, 1.0, 0.0) else (false, 0.0, 1.0)
	| Branch(content, left, right) -> if not intersects content.bounds seg then (false, 0.0, 1.0) else
				let eval fistChild secondchild =
					let (hits, c1, c2) = (measureCost firstChild seg) in
					if hits then (true, c1, c2) else (measureCost secondChild seg) in
				incCost( match
						match kernel with
						| UniformRandom -> 0.5
						| LeftFirst -> 1.0
						| RightFirst -> 0.0
						| FrontToBack -> if length ((center bounds left) - seg.origin) <= length ((center bounds right) - seg.origin) then 0.0 else 1.0
						| BackToFront -> if length ((center bounds left) - seg.origin) > length ((center bounds right) - seg.origin) then 0.0 else 1.0
						with
						| 0.0 -> eval right left
						| 1.0 -> eval left right
						| pLeft ->
								let (lhits, lc1, lc2) = (measureCost left seg); (rhits, rc1, rc2) = (measureCost right seg) in
								(lHits || rHits, blendCost pLeft lc1 rc1, blendCost pLeft lc2 rc2));;
