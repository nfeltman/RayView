

let read_single input =
	let b1 = input_byte input in
	let b2 = input_byte input in
	let b3 = input_byte input in
	let b4 = Int32.shift_left (Int32.of_int (input_byte input)) 24 in
	Int32.float_of_bits (Int32.logor b4 (Int32.of_int (b1 lor (b2 lsl 8) lor (b3 lsl 16))))

let read_Int32 input =
	let b1 = input_byte input in
	let b2 = input_byte input in
	let b3 = input_byte input in
	let b4 = input_byte input in
	(((b4 * 256) + b3) *256 + b2) *256 + b1