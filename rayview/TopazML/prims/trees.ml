type ('b, 'l) tree = Branch of 'b * ('b, 'l) tree * ('b, 'l) tree | Leaf of 'l

let rec foldUp node forBranch forLeaf = match node with
	| Branch(content, left, right) -> forBranch content (foldUp left forBranch forLeaf) (foldUp right forBranch forLeaf)
	| Leaf(l) -> forLeaf l;;

let rec mapTree forBranch forLeaf node =
	let rec subMap n = match n with
		| Branch(content, left, right) -> Branch(forBranch content, subMap left, subMap right)
		| Leaf(l) -> Leaf(forLeaf l)
	in subMap node

let rec foldMapTree forBranch forLeaf node =
	let rec subMap n = match n with
		| Branch(content, left, right) ->
				let (lt, ld), (rt, rd) = subMap left, subMap right in
				let br, d = forBranch content ld rd in
				(Branch(br, lt, rt), d)
		| Leaf(l) -> let le, d = forLeaf l in (Leaf le, d)
	in fst (subMap node)