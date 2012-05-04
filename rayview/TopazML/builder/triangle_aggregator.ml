open Build_triangle;;
open Box3;;
open ArrayUtil;;

type agg = {box : Box3.box3; count : int}

let combine a1 a2 = {box = join a1.box a2.box; count = a1.count + a2.count}
let add_triangle tri a = {box = NotEmpty(ne1_join (getBounds tri) a.box); count = a.count + 1}
let roll range arr = {box = NotEmpty(calcBoundMap getTriangle arr range); count = rangeSize range}
let rollList range arr = {box = NotEmpty(calcBoundMap getTriangle arr range); count = rangeSize range}
let defaultAgg = {box = Empty; count = 0}