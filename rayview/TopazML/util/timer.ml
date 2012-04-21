type timer = { time : float ref }
let create () = { time = ref (Sys.time()) }
let elapsed_s timer = Sys.time() -. !(timer.time)
let reset_s timer = let curr = Sys.time() in
	let elapsed = curr -. !(timer.time) in
	timer.time := curr;
	elapsed