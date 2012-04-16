type ('b, 'l) branch = { content : 'b; left : ('b, 'l) tree; right : ('b, 'l) tree }
type ('b, 'l) tree = Branch of 'b branch | Leaf of 'l
val rollUp : ('b, 'l) tree -> ('b -> 'r -> 'r -> 'r) -> ('l -> 'r) -> 'r
