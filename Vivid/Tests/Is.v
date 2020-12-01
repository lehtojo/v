Entity {}

Entity Person {
    name: String
    skill: num
    likes_driving: bool
    likes_riding: bool

    init(name: link, skill: num, likes_driving: bool, likes_riding: bool) {
        this.name = String(name)
        this.skill = skill
        this.likes_driving = likes_driving
        this.likes_riding = likes_riding
    }
}

Usable {
    likes(entity: Entity): bool
}

Usable Vehicle {
    speed: decimal
    weight: num
    acceleration: decimal
    passengers: small

    time(distance: decimal) => sqrt(2 * distance / acceleration)

    skill(): num
    reliability(): num
}

Drivable {}
Ridable {}

Ridable Vehicle Pig {

    init() {
        speed = 7
        weight = 100
        acceleration = 3
        passengers = 1
    }

    skill() => 1
    reliability() => -1

    likes(entity: Entity) {
        => entity is Person person and person.likes_riding
    }
}

Drivable Vehicle Car {

    init() {
        speed = 55
        weight = 1500
        acceleration = 5.555
        passengers = 5
    }

    skill() => 10
    reliability() => 100

    likes(entity: Entity) {
        => entity is Person person and person.likes_driving
    }
}

Usable Entity Banana {
    likes(entity: Entity) => true as bool
}

Drivable Vehicle Bus {
    init() {
        speed = 40
        weight = 4000
        acceleration = 2.5
        passengers = 40
    }

    skill() => 40
    reliability() => 100

    likes(entity: Entity) {
        => entity is Person person and person.likes_driving
    }
}

export can_use(entity: Entity, usable: Usable) {
    if !usable.likes(entity) {
        => false as bool
    }
    else usable is Vehicle vehicle and entity is Person person {
        => person.skill >= vehicle.skill()
    }

    => false as bool
}

export get_reliable_vehicles(usables: Array<Usable>, min_reliability: num) {
    vehicles = List<Vehicle>()

    loop (i = 0, i < usables.count, i++) {
        if usables[i] is Vehicle {
            vehicles.add(usables[i] as Vehicle)
        }
    }

    loop (i = vehicles.size() - 1, i >= 0, i--) {
        if vehicles[i].reliability() < min_reliability {
            vehicles.remove(i)
        }
    }

    => vehicles
}

export choose_vehicle(entity: Entity, vehicles: List<Vehicle>, distance: num) {
	=> choose_vehicle(entity, vehicles, distance as decimal)
}

export choose_vehicle(entity: Entity, vehicles: List<Vehicle>, distance: decimal) {
    if entity is Person person and person.name == 'Steve' {
        => Pig() as Vehicle
    }

    chosen_vehicle = vehicles[0]
    minimum_time = vehicles[0].time(distance)

    loop (i = 1, i < vehicles.size(), i++) {
        vehicle = vehicles[i]
        time = vehicle.time(distance)

        if time < minimum_time {
            chosen_vehicle = vehicle
            minimum_time = time
        }
    }

    => chosen_vehicle
}

export create_pig() => Pig()
export create_bus() => Bus()
export create_car() => Car()
export create_banana() => Banana()

export create_john() => Person('John', 10, true as bool, false as bool)
export create_max() => Person('Max', 7, true as bool, true as bool)
export create_gabe() => Person('Gabe', 50, true as bool, false as bool)
export create_steve() => Person('Steve', 1, false as bool, true as bool)

export create_array(size: num) => Array<Usable>(size)
export set(array: Array<Usable>, usable: Usable, i: num) {
    array[i] = usable
}

export is_pig(vehicle: Vehicle) => vehicle is Pig

init() {
	john = create_john()

	pig = create_pig()
	bus = create_bus()
	car = create_car()
	banana = create_banana()

	array = create_array(4)
	array[0] = pig
	array[1] = bus
	array[2] = car
	array[3] = banana

	vehicles = get_reliable_vehicles(array, 10)

	choose_vehicle(john, vehicles, 7000)

	=> true
}