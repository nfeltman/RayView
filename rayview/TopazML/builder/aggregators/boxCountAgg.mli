


type boxCount = { box : Box3.ne_box3; count : int }
module A : TriangleAggregator.Aggregator with type ne_agg = boxCount