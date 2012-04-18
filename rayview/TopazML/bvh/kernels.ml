type kernelType = UniformRandom | LeftFirst | RightFirst | FrontToBack | BackToFront
exception KernelParse of string

let getKernel i = match i with
						| 11 -> LeftFirst
						| 12 -> RightFirst
						| 13 -> UniformRandom
						| 14 -> FrontToBack
						| 15 -> BackToFront
						| n -> raise (KernelParse("Unexpected kernel number " ^ string_of_int n))