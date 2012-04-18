type ne_interval = { min: float; max: float }
type interval = Empty | NotEmpty of ne_interval
val ne_make : float -> float -> ne_interval
val make : float -> float -> interval
val isNonEmpty : interval -> bool
val isEmpty : interval -> bool
val center : ne_interval -> float
val length : interval -> float
val ne_length : ne_interval -> float
val contains : interval -> float -> bool
val ne_contains : ne_interval -> float -> bool
val overlaps : interval -> interval -> bool
val (+) : interval -> float -> interval
val (-) : interval -> float -> interval
val ne_minus : ne_interval -> float -> ne_interval
val ( * ): interval -> float -> interval
val (/) : interval -> float -> interval
val ne_divide : ne_interval -> float -> interval
val meet : interval -> interval -> interval
val join : interval -> interval -> interval
val ne_meet : ne_interval -> ne_interval -> interval
val ne_join : ne_interval -> ne_interval -> ne_interval