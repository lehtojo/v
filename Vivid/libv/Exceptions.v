Outcome<T, E> {
	value: u64
	is_error: bool

	has_value() => !is_error
	get_value() => value as T
}

Outcome<T, E> Ok<T, E> {
	init(value: T) {
		this.value = value as u64
		this.is_error = false
	}
}

Outcome<T, E> Error<T, E> {
	init(value: E) {
		this.value = value as u64
		this.is_error = true
	}

	get_error() => value as E
}

Optional<T> {
	value: T
	empty: bool

	init() {
		empty = true
	}

	init(value: T) {
		this.value = value
		this.empty = false
	}

	has_value() => !empty
	get_value() => value
}