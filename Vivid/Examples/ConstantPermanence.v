copy(source, destination) {
	offset = 256
	i = 0

	loop (i = 0, i < 10, ++i) {
		destination[offset + i] = source[offset + i]
	}
}

init() {
	source = allocate(1024)
	destination = allocate(1024)
	copy(source, destination)
}