BEGIN;

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

INSERT INTO components (id, category_id, name, price, brand, color, image_url)
VALUES (1, 1, 'Ryzen 5 5600', 5200.00, 'AMD', 'Silver', 'https://example.com/cpu.jpg')
ON CONFLICT (id) DO NOTHING;

INSERT INTO cpus (component_id, socket_id, cores, threads, base_clock, boost_clock, tdp_watts, has_integrated_gpu)
VALUES (1, 1, 6, 12, 3.5, 4.4, 65, FALSE)
ON CONFLICT (component_id) DO NOTHING;

INSERT INTO components (id, category_id, name, price, brand, color, image_url)
VALUES (2, 3, 'GeForce RTX 4060 Dual', 13500.00, 'ASUS', 'Black', 'https://example.com/gpu.jpg')
ON CONFLICT (id) DO NOTHING;

INSERT INTO gpus (component_id, vram_gb, memory_type, tdp_watts, length_mm, recommended_psu_watts)
VALUES (2, 8, 'GDDR6', 115, 227, 550)
ON CONFLICT (component_id) DO NOTHING;

INSERT INTO components (id, category_id, name, price, brand, color, image_url)
VALUES (3, 4, 'Fury Beast 16GB DDR4', 1600.00, 'Kingston', 'Black', 'https://example.com/ram.jpg')
ON CONFLICT (id) DO NOTHING;

INSERT INTO ram_modules (component_id, memory_type, capacity_gb, module_count, frequency_mhz)
VALUES (3, 'DDR4', 16, 2, 3200)
ON CONFLICT (component_id) DO NOTHING;

INSERT INTO components (id, category_id, name, price, brand, color, image_url)
VALUES (4, 6, 'Deepcool AK400', 1200.00, 'Deepcool', 'Black', 'https://example.com/cooler.jpg')
ON CONFLICT (id) DO NOTHING;

INSERT INTO cpu_coolers (component_id, fan_rpm, noise_level_db, height_mm, tdp_rating_watts)
VALUES (4, 1850, 29.0, 155, 220)
ON CONFLICT (component_id) DO NOTHING;

INSERT INTO cpu_cooler_sockets (cooler_id, socket_id) VALUES
(4, 1), (4, 2), (4, 3)
ON CONFLICT DO NOTHING;

SELECT setval('categories_id_seq', (SELECT MAX(id) FROM categories));
SELECT setval('sockets_id_seq', (SELECT MAX(id) FROM sockets));
SELECT setval('form_factors_id_seq', (SELECT MAX(id) FROM form_factors));
SELECT setval('components_id_seq', (SELECT MAX(id) FROM components));

COMMIT;
