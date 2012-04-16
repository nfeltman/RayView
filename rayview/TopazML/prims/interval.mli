type ne_interval = { min: float; max: float }
type interval = Empty | NotEmpty of ne_interval
val isNonEmpty : interval -> bool
val isEmpty : interval -> bool
val center : ne_interval -> vec3
val length : interval -> float
val contains : interval -> float -> bool
val intersects : interval -> interval -> bool
val (+) : interval -> float -> interval
val (-) : interval -> float -> interval
val ( * ): interval -> float -> interval
val (/) : interval -> float -> interval
val (&) : interval -> interval -> interval
val ( || ) : interval -> interval -> interval