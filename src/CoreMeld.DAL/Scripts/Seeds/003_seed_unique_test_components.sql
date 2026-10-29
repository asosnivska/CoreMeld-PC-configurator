BEGIN;

-- Unique CPU for testing DB connection
INSERT INTO components (id, category_id, name, price, brand, color, image_url)
VALUES (50, 1, 'AMD Ryzen 7 7800X3D', 18999.00, 'AMD', 'Silver', 'https://example.com/7800x3d.jpg')
ON CONFLICT (id) DO UPDATE SET
    category_id = EXCLUDED.category_id,
    name = EXCLUDED.name,
    price = EXCLUDED.price,
    brand = EXCLUDED.brand,
    color = EXCLUDED.color,
    image_url = EXCLUDED.image_url;

INSERT INTO cpus (component_id, socket_id, cores, threads, base_clock, boost_clock, tdp_watts, has_integrated_gpu)
VALUES (50, 2, 8, 16, 4.2, 5.0, 120, TRUE)
ON CONFLICT (component_id) DO UPDATE SET
    socket_id = EXCLUDED.socket_id,
    cores = EXCLUDED.cores,
    threads = EXCLUDED.threads,
    base_clock = EXCLUDED.base_clock,
    boost_clock = EXCLUDED.boost_clock,
    tdp_watts = EXCLUDED.tdp_watts,
    has_integrated_gpu = EXCLUDED.has_integrated_gpu;

-- Unique GPU for testing DB connection
INSERT INTO components (id, category_id, name, price, brand, color, image_url)
VALUES (51, 3, 'ASUS ROG Strix GeForce RTX 4090 24GB', 89999.00, 'ASUS', 'Black', 'https://example.com/rtx4090.jpg')
ON CONFLICT (id) DO UPDATE SET
    category_id = EXCLUDED.category_id,
    name = EXCLUDED.name,
    price = EXCLUDED.price,
    brand = EXCLUDED.brand,
    color = EXCLUDED.color,
    image_url = EXCLUDED.image_url;

INSERT INTO gpus (component_id, vram_gb, memory_type, tdp_watts, length_mm, recommended_psu_watts)
VALUES (51, 24, 'GDDR6X', 450, 357, 1000)
ON CONFLICT (component_id) DO UPDATE SET
    vram_gb = EXCLUDED.vram_gb,
    memory_type = EXCLUDED.memory_type,
    tdp_watts = EXCLUDED.tdp_watts,
    length_mm = EXCLUDED.length_mm,
    recommended_psu_watts = EXCLUDED.recommended_psu_watts;

SELECT setval('components_id_seq', (SELECT MAX(id) FROM components));

COMMIT;

