type ('b, 'l) tree = Branch of 'b * ('b, 'l) tree * ('b, 'l) tree | Leaf of 'l
val rollUp : ('b, 'l) tree -> ('b -> 'r -> 'r -> 'r) -> ('l -> 'r) -> 'r
