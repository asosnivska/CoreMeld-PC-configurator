BEGIN;

CREATE TABLE IF NOT EXISTS categories (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS sockets (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS form_factors (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS components (
    id SERIAL PRIMARY KEY,
    category_id INT NOT NULL REFERENCES categories(id) ON DELETE RESTRICT,
    name VARCHAR(250) NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    brand VARCHAR(100) NOT NULL,
    image_url VARCHAR(500)
);

CREATE TABLE IF NOT EXISTS cpus (
    component_id INT PRIMARY KEY REFERENCES components(id) ON DELETE CASCADE,
    socket_id INT NOT NULL REFERENCES sockets(id) ON DELETE RESTRICT,
    cores INT NOT NULL,
    threads INT NOT NULL,
    base_clock DECIMAL(4, 2) NOT NULL,
    boost_clock DECIMAL(4, 2) NOT NULL,
    tdp_watts INT NOT NULL,
    has_integrated_gpu BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE TABLE IF NOT EXISTS motherboards (
    component_id INT PRIMARY KEY REFERENCES components(id) ON DELETE CASCADE,
    socket_id INT NOT NULL REFERENCES sockets(id) ON DELETE RESTRICT,
    form_factor_id INT NOT NULL REFERENCES form_factors(id) ON DELETE RESTRICT,
    chipset VARCHAR(50) NOT NULL,
    memory_type VARCHAR(20) NOT NULL,
    memory_slots INT NOT NULL,
    max_memory_gb INT NOT NULL
);

CREATE TABLE IF NOT EXISTS gpus (
    component_id INT PRIMARY KEY REFERENCES components(id) ON DELETE CASCADE,
    vram_gb INT NOT NULL,
    memory_type VARCHAR(20) NOT NULL,
    tdp_watts INT NOT NULL,
    length_mm INT NOT NULL,
    recommended_psu_watts INT NOT NULL
);

CREATE TABLE IF NOT EXISTS ram_modules (
    component_id INT PRIMARY KEY REFERENCES components(id) ON DELETE CASCADE,
    memory_type VARCHAR(20) NOT NULL,
    capacity_gb INT NOT NULL,
    module_count INT NOT NULL,
    frequency_mhz INT NOT NULL
);

CREATE TABLE IF NOT EXISTS power_supplies (
    component_id INT PRIMARY KEY REFERENCES components(id) ON DELETE CASCADE,
    form_factor_id INT NOT NULL REFERENCES form_factors(id) ON DELETE RESTRICT,
    wattage INT NOT NULL,
    efficiency_rating VARCHAR(50) NOT NULL
);

CREATE TABLE IF NOT EXISTS cases (
    component_id INT PRIMARY KEY REFERENCES components(id) ON DELETE CASCADE,
    max_gpu_length_mm INT NOT NULL,
    max_cpu_cooler_height_mm INT NOT NULL
);

CREATE TABLE IF NOT EXISTS storage_drives (
    component_id INT PRIMARY KEY REFERENCES components(id) ON DELETE CASCADE,
    drive_type VARCHAR(50) NOT NULL,
    capacity_gb INT NOT NULL,
    interface VARCHAR(50) NOT NULL
);

CREATE TABLE IF NOT EXISTS cpu_coolers (
    component_id INT PRIMARY KEY REFERENCES components(id) ON DELETE CASCADE,
    fan_rpm INT,
    noise_level_db DECIMAL(4, 1),
    height_mm INT NOT NULL,
    tdp_rating_watts INT NOT NULL
);

CREATE TABLE IF NOT EXISTS cpu_cooler_sockets (
    cooler_id INT REFERENCES cpu_coolers(component_id) ON DELETE CASCADE,
    socket_id INT REFERENCES sockets(id) ON DELETE RESTRICT,
    PRIMARY KEY (cooler_id, socket_id)
);

CREATE TABLE IF NOT EXISTS builds (
    id SERIAL PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    total_price DECIMAL(10, 2) NOT NULL DEFAULT 0.00,
    total_tdp_watts INT NOT NULL DEFAULT 0,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS build_components (
    build_id INT REFERENCES builds(id) ON DELETE CASCADE,
    component_id INT REFERENCES components(id) ON DELETE RESTRICT,
    quantity INT NOT NULL DEFAULT 1,
    PRIMARY KEY (build_id, component_id)
);

COMMIT;