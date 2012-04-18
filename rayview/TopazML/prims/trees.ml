type ('b, 'l) tree = Branch of 'b * ('b, 'l) tree * ('b, 'l) tree | Leaf of 'l

let rec rollUp node forBranch forLeaf = match node with
	| Branch(content, left, right) -> forBranch content (rollUp left forBranch forLeaf) (rollUp right forBranch forLeaf)
	| Leaf(l) -> forLeaf l;;
