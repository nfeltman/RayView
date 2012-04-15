module Triangle =
struct
	type triangle = vec3 * vec3 * vec
	intersects triangle seg = true
end