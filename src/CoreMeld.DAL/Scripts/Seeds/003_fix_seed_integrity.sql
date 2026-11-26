-- Seed 003: data integrity fixes after seeds 001 and 002 (#61).
-- Safe to run on an existing database and safe to run more than once.
-- Run after migrations 001-003 and seeds 001-002.

BEGIN;

-- 1. Remove detail rows whose component belongs to another category.
--    Seed 002 reused id 5 (Corsair CX650 power supply from seed 001) for a
--    motherboard, so power_supplies(5) pointed at a motherboard.
DELETE FROM cpus d USING components c
WHERE c.id = d.component_id AND c.category_id <> 1;

DELETE FROM motherboards d USING components c
WHERE c.id = d.component_id AND c.category_id <> 2;

DELETE FROM gpus d USING components c
WHERE c.id = d.component_id AND c.category_id <> 3;

DELETE FROM ram_modules d USING components c
WHERE c.id = d.component_id AND c.category_id <> 4;

DELETE FROM storage_drives d USING components c
WHERE c.id = d.component_id AND c.category_id <> 5;

DELETE FROM cpu_coolers d USING components c
WHERE c.id = d.component_id AND c.category_id <> 6;

DELETE FROM cases d USING components c
WHERE c.id = d.component_id AND c.category_id <> 7;

DELETE FROM power_supplies d USING components c
WHERE c.id = d.component_id AND c.category_id <> 8;

-- Keep the sequence ahead of the explicit ids used by seeds 001 and 002.
SELECT setval('components_id_seq', (SELECT COALESCE(MAX(id), 1) FROM components));

-- 2. Restore Corsair CX650 under a new id (it lost id 5 to a motherboard).
INSERT INTO components (category_id, name, price, brand, color, image_url)
SELECT 8, 'Corsair CX650', 2300.00, 'Corsair', 'Black', 'https://example.com/psu.jpg'
WHERE NOT EXISTS (
    SELECT 1 FROM components WHERE category_id = 8 AND name = 'Corsair CX650'
);

INSERT INTO power_supplies (component_id, form_factor_id, wattage, efficiency_rating, pcie_8pin_count, has_12vhpwr)
SELECT id, 1, 650, '80+ Bronze', 2, FALSE
FROM components
WHERE category_id = 8 AND name = 'Corsair CX650'
ON CONFLICT (component_id) DO NOTHING;

-- 3. Coolers from seed 002 support all three sockets (manufacturer specs).
INSERT INTO cpu_cooler_sockets (cooler_id, socket_id) VALUES
(16, 1), (16, 2), (16, 3),
(17, 1), (17, 2), (17, 3)
ON CONFLICT DO NOTHING;

-- 4. One more AM4 CPU, so AM4 does not depend on a single seed 001 row.
INSERT INTO components (category_id, name, price, brand, color, image_url)
SELECT 1, 'AMD Ryzen 5 5500', 3100.00, 'AMD', 'Silver', 'https://example.com/cpu-5500.jpg'
WHERE NOT EXISTS (
    SELECT 1 FROM components WHERE category_id = 1 AND name = 'AMD Ryzen 5 5500'
);

INSERT INTO cpus (component_id, socket_id, cores, threads, base_clock, boost_clock, tdp_watts, has_integrated_gpu)
SELECT id, 1, 6, 12, 3.6, 4.2, 65, FALSE
FROM components
WHERE category_id = 1 AND name = 'AMD Ryzen 5 5500'
ON CONFLICT (component_id) DO NOTHING;

COMMIT;
