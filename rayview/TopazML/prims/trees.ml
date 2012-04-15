module Trees =
struct
	type ('b, 'l) branch = { content : 'b; left : ('b, 'l) tree; right : ('b, 'l) tree }
	type ('b, 'l) tree = Branch of 'b branch | Leaf of 'l
	let rec rollUp node forBranch forLeaf = match node with
		| Branch(content, left, right) -> forBranch content (rollUp left forBranch forLeaf) (rollup right forBrnach forLeaf)
		| Leaf(l) -> forLeaf l;;
end