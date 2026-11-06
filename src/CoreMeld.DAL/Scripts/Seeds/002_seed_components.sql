BEGIN;

-- Ensure categories, sockets, form factors exist
INSERT INTO categories (id, name) VALUES
(1, 'CPU'),
(2, 'Motherboard'),
(3, 'GPU'),
(4, 'RAM'),
(5, 'Storage'),
(6, 'CPU Cooler'),
(7, 'Case'),
(8, 'Power Supply')
ON CONFLICT (id) DO NOTHING;

INSERT INTO sockets (id, name) VALUES
(1, 'AM4'),
(2, 'AM5'),
(3, 'LGA1700')
ON CONFLICT (id) DO NOTHING;

INSERT INTO form_factors (id, name) VALUES
(1, 'ATX'),
(2, 'Micro-ATX'),
(3, 'Mini-ITX')
ON CONFLICT (id) DO NOTHING;

-- Ensure has_wifi column exists on motherboards
ALTER TABLE motherboards ADD COLUMN IF NOT EXISTS has_wifi BOOLEAN NOT NULL DEFAULT FALSE;

-- Base components
INSERT INTO components (id, category_id, name, price, brand, color, image_url) VALUES
-- Motherboards (Category 2)
(5, 2, 'ASUS TUF Gaming A520M-Plus II', 3199.00, 'ASUS', 'Black', 'https://example.com/mb-a520.jpg'),
(6, 2, 'Gigabyte B650M DS3H', 5400.00, 'Gigabyte', 'Black', 'https://example.com/mb-b650.jpg'),
(7, 2, 'MSI PRO B660M-A WIFI DDR4', 5600.00, 'MSI', 'Black', 'https://example.com/mb-b660.jpg'),
(8, 2, 'ASUS PRIME Z790-P WIFI', 9800.00, 'ASUS', 'Black', 'https://example.com/mb-z790.jpg'),

-- RAM Modules (Category 4)
(9, 4, 'G.Skill Ripjaws V 32GB (2x16) DDR4-3600', 3300.00, 'G.Skill', 'Black', 'https://example.com/ram-3600.jpg'),
(10, 4, 'Kingston Fury Beast 32GB (2x16) DDR5-5600', 3900.00, 'Kingston', 'Black', 'https://example.com/ram-5600.jpg'),

-- GPUs (Category 3)
(11, 3, 'MSI GeForce GTX 1650 D6 Ventus XS 4GB', 6200.00, 'MSI', 'Black', 'https://example.com/gpu-1650.jpg'),
(12, 3, 'Gigabyte GeForce RTX 4070 SUPER WINDFORCE OC 12GB', 28500.00, 'Gigabyte', 'Black', 'https://example.com/gpu-4070.jpg'),

-- Storage Drives (Category 5)
(13, 5, 'Kingston NV2 500GB M.2 NVMe SSD', 1500.00, 'Kingston', 'Black', 'https://example.com/ssd-500.jpg'),
(14, 5, 'Samsung 980 1TB M.2 NVMe SSD', 3100.00, 'Samsung', 'Black', 'https://example.com/ssd-1tb.jpg'),
(15, 5, 'Seagate BarraCuda 2TB HDD', 2000.00, 'Seagate', 'Silver', 'https://example.com/hdd-2tb.jpg'),

-- CPU Coolers (Category 6)
(16, 6, 'ID-Cooling SE-214-XT', 849.00, 'ID-Cooling', 'Black', 'https://example.com/cooler-214.jpg'),
(17, 6, 'Thermalright Peerless Assassin 120 SE', 1500.00, 'Thermalright', 'Black', 'https://example.com/cooler-pa120.jpg'),

-- Cases (Category 7)
(18, 7, 'Gamemax Destroyer MB', 1755.00, 'Gamemax', 'Black', 'https://example.com/case-gamemax.jpg'),
(19, 7, 'DeepCool CC560', 2300.00, 'DeepCool', 'Black', 'https://example.com/case-cc560.jpg'),

-- Power Supplies (Category 8)
(20, 8, 'Chieftec Polaris 3.0 550W', 1900.00, 'Chieftec', 'Black', 'https://example.com/psu-550.jpg'),
(21, 8, 'Chieftec PPG-850-C', 4769.00, 'Chieftec', 'Black', 'https://example.com/psu-850.jpg'),

-- CPUs (Category 1)
(22, 1, 'AMD Ryzen 5 7600', 8900.00, 'AMD', 'Silver', 'https://example.com/cpu-7600.jpg'),
(23, 1, 'Intel Core i5-12400F', 6000.00, 'Intel', 'Silver', 'https://example.com/cpu-12400f.jpg'),
(24, 1, 'Intel Core i7-13700K', 15500.00, 'Intel', 'Silver', 'https://example.com/cpu-13700k.jpg')
ON CONFLICT (id) DO UPDATE SET
    category_id = EXCLUDED.category_id,
    name = EXCLUDED.name,
    price = EXCLUDED.price,
    brand = EXCLUDED.brand,
    color = EXCLUDED.color,
    image_url = EXCLUDED.image_url;

-- Motherboards detail
INSERT INTO motherboards (component_id, socket_id, form_factor_id, chipset, memory_type, memory_slots, max_memory_gb, has_wifi) VALUES
(5, 1, 2, 'A520', 'DDR4', 4, 128, FALSE),
(6, 2, 2, 'B650', 'DDR5', 4, 192, FALSE),
(7, 3, 2, 'B660', 'DDR4', 4, 128, TRUE),
(8, 3, 1, 'Z790', 'DDR5', 4, 192, TRUE)
ON CONFLICT (component_id) DO UPDATE SET
    socket_id = EXCLUDED.socket_id,
    form_factor_id = EXCLUDED.form_factor_id,
    chipset = EXCLUDED.chipset,
    memory_type = EXCLUDED.memory_type,
    memory_slots = EXCLUDED.memory_slots,
    max_memory_gb = EXCLUDED.max_memory_gb,
    has_wifi = EXCLUDED.has_wifi;

-- RAM Modules detail
INSERT INTO ram_modules (component_id, memory_type, capacity_gb, module_count, frequency_mhz) VALUES
(9, 'DDR4', 32, 2, 3600),
(10, 'DDR5', 32, 2, 5600)
ON CONFLICT (component_id) DO UPDATE SET
    memory_type = EXCLUDED.memory_type,
    capacity_gb = EXCLUDED.capacity_gb,
    module_count = EXCLUDED.module_count,
    frequency_mhz = EXCLUDED.frequency_mhz;

-- GPUs detail
INSERT INTO gpus (component_id, vram_gb, memory_type, tdp_watts, length_mm, recommended_psu_watts) VALUES
(11, 4, 'GDDR6', 75, 204, 300),
(12, 12, 'GDDR6X', 220, 261, 650)
ON CONFLICT (component_id) DO UPDATE SET
    vram_gb = EXCLUDED.vram_gb,
    memory_type = EXCLUDED.memory_type,
    tdp_watts = EXCLUDED.tdp_watts,
    length_mm = EXCLUDED.length_mm,
    recommended_psu_watts = EXCLUDED.recommended_psu_watts;

-- Storage Drives detail
INSERT INTO storage_drives (component_id, drive_type, capacity_gb, interface) VALUES
(13, 'SSD', 500, 'NVMe'),
(14, 'SSD', 1000, 'NVMe'),
(15, 'HDD', 2000, 'SATA')
ON CONFLICT (component_id) DO UPDATE SET
    drive_type = EXCLUDED.drive_type,
    capacity_gb = EXCLUDED.capacity_gb,
    interface = EXCLUDED.interface;

-- CPU Coolers detail
INSERT INTO cpu_coolers (component_id, fan_rpm, noise_level_db, height_mm, tdp_rating_watts) VALUES
(16, 1500, 28.0, 155, 180),
(17, 1550, 25.6, 155, 245)
ON CONFLICT (component_id) DO UPDATE SET
    fan_rpm = EXCLUDED.fan_rpm,
    noise_level_db = EXCLUDED.noise_level_db,
    height_mm = EXCLUDED.height_mm,
    tdp_rating_watts = EXCLUDED.tdp_rating_watts;

-- CPU Cooler Sockets
INSERT INTO cpu_cooler_sockets (cooler_id, socket_id) VALUES
(16, 1),
(17, 3)
ON CONFLICT DO NOTHING;

-- Cases detail
INSERT INTO cases (component_id, max_gpu_length_mm, max_cpu_cooler_height_mm) VALUES
(18, 300, 155),
(19, 370, 163)
ON CONFLICT (component_id) DO UPDATE SET
    max_gpu_length_mm = EXCLUDED.max_gpu_length_mm,
    max_cpu_cooler_height_mm = EXCLUDED.max_cpu_cooler_height_mm;

-- Power Supplies detail
INSERT INTO power_supplies (component_id, form_factor_id, wattage, efficiency_rating) VALUES
(20, 1, 550, '80+ Bronze'),
(21, 1, 850, '80+ Gold')
ON CONFLICT (component_id) DO UPDATE SET
    form_factor_id = EXCLUDED.form_factor_id,
    wattage = EXCLUDED.wattage,
    efficiency_rating = EXCLUDED.efficiency_rating;

-- CPUs detail
INSERT INTO cpus (component_id, socket_id, cores, threads, base_clock, boost_clock, tdp_watts, has_integrated_gpu) VALUES
(22, 2, 6, 12, 3.8, 5.1, 65, TRUE),
(23, 3, 6, 12, 2.5, 4.4, 65, FALSE),
(24, 3, 16, 24, 3.4, 5.4, 125, TRUE)
ON CONFLICT (component_id) DO UPDATE SET
    socket_id = EXCLUDED.socket_id,
    cores = EXCLUDED.cores,
    threads = EXCLUDED.threads,
    base_clock = EXCLUDED.base_clock,
    boost_clock = EXCLUDED.boost_clock,
    tdp_watts = EXCLUDED.tdp_watts,
    has_integrated_gpu = EXCLUDED.has_integrated_gpu;

-- Reset sequence to highest ID
SELECT setval('components_id_seq', (SELECT COALESCE(MAX(id), 1) FROM components));

COMMIT;

