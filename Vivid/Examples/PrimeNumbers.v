init() {
	i = 1000

	alkuluvut = num[i]
	alkuluvut[0] = 2

	luvut = 1
	I = 2

	loop (I <= i) {
		C = 0
		epaonnistuneet = 0

		loop (C < luvut) {
			luku = alkuluvut[C]

			if I % luku == 0 {
				++epaonnistuneet
			}

			if epaonnistuneet == 0 {
				if luku * luku >= I {
					alkuluvut[luvut] = I
					++luvut
					stop
				}
				else if epaonnistuneet > 0 {
					stop
				}
			}
			else if epaonnistuneet > 0 {
				stop
			}

			++C
		}

		++I
	}

	println('-------------------')
	k = 0
	loop (k < luvut) {
		printsln(to_string(alkuluvut[k]))
		++k
	}
	println('-------------------')
}