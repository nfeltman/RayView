type range = int * int (* left-inclusive, right-exclusive *)

type 'a left_filter = 'a -> bool

val split_range : range -> int -> range * range
val entireRange : 'a array -> range
val rangeSize : range -> int
val rangeMidpoint : range -> int
val partition : 'a left_filter -> range -> 'a array -> unit