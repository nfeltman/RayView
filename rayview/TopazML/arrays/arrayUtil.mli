type range = int * int (* left-inclusive, right-exclusive *)

type 'a left_filter = 'a -> bool

val split_range : range -> int -> range * range
val incrBottom : range -> range
val entireRange : 'a array -> range
val rangeSize : range -> int
val rangeMidpoint : range -> int
val iterRange : ('a -> unit) -> range -> 'a array -> unit
val partition : 'a left_filter -> range -> 'a array -> int
val smartPartition : 'a left_filter -> ('a -> int -> unit) -> range -> 'a array -> int

val pickMin : ('a -> float * 'b) -> 'a array -> float * 'b

type ('retVal, 'foldVal) skipable = Skip of 'foldVal | NoSkip of 'retVal * 'foldVal
val pickMinFoldLeft : (int -> 'a -> 'c -> (float * 'b, 'c) skipable) -> 'c -> 'a array -> float * 'b * 'c