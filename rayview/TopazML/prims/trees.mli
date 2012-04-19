type ('b, 'l) tree = Branch of 'b * ('b, 'l) tree * ('b, 'l) tree | Leaf of 'l
val foldUp : ('b, 'l) tree -> ('b -> 'r -> 'r -> 'r) -> ('l -> 'r) -> 'r
val mapTree : ('bi -> 'bo) -> ('li -> 'lo) -> ('bi, 'li) tree -> ('bo, 'lo) tree
val foldMapTree : ('bi -> 'd -> 'd -> 'bo * 'd) -> ('li -> 'lo * 'd) -> ('bi, 'li) tree -> ('bo, 'lo) tree
