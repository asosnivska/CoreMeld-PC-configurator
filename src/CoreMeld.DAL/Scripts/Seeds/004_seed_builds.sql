BEGIN;

-- Preset Build 1: AM4 Budget Gaming
INSERT INTO builds (id, name, total_price, total_tdp_watts)
VALUES (1, 'Бюджетна ігрова збірка AM4 (Ryzen 5 5600 + RTX 4060)', 29503.00, 230)
ON CONFLICT (id) DO UPDATE SET
    name = EXCLUDED.name,
    total_price = EXCLUDED.total_price,
    total_tdp_watts = EXCLUDED.total_tdp_watts;

DELETE FROM build_components WHERE build_id = 1;
INSERT INTO build_components (build_id, component_id, quantity) VALUES
(1, 1, 1),   -- Ryzen 5 5600
(1, 5, 1),   -- ASUS TUF Gaming A520M-Plus II
(1, 2, 1),   -- GeForce RTX 4060 Dual
(1, 3, 1),   -- Fury Beast 16GB DDR4
(1, 13, 1),  -- Kingston NV2 500GB SSD
(1, 4, 1),   -- Deepcool AK400
(1, 18, 1),  -- Gamemax Destroyer MB
(1, 20, 1)   -- Chieftec Polaris 3.0 550W
ON CONFLICT (build_id, component_id) DO NOTHING;

-- Preset Build 2: AM5 Optimal Gaming
INSERT INTO builds (id, name, total_price, total_tdp_watts)
VALUES (2, 'Оптимальна збірка AM5 (Ryzen 5 7600 + RTX 4070 Super)', 60369.00, 340)
ON CONFLICT (id) DO UPDATE SET
    name = EXCLUDED.name,
    total_price = EXCLUDED.total_price,
    total_tdp_watts = EXCLUDED.total_tdp_watts;

DELETE FROM build_components WHERE build_id = 2;
INSERT INTO build_components (build_id, component_id, quantity) VALUES
(2, 22, 1),  -- AMD Ryzen 5 7600
(2, 6, 1),   -- Gigabyte B650M DS3H
(2, 12, 1),  -- Gigabyte GeForce RTX 4070 SUPER
(2, 10, 1),  -- Kingston Fury Beast 32GB DDR5
(2, 14, 1),  -- Samsung 980 1TB SSD
(2, 17, 1),  -- Thermalright Peerless Assassin 120 SE
(2, 19, 1),  -- DeepCool CC560
(2, 21, 1)   -- Chieftec PPG-850-C
ON CONFLICT (build_id, component_id) DO NOTHING;

-- Preset Build 3: Intel High-End
INSERT INTO builds (id, name, total_price, total_tdp_watts)
VALUES (3, 'Потужна збірка Intel (Core i7-13700K + RTX 4070 Super)', 69369.00, 470)
ON CONFLICT (id) DO UPDATE SET
    name = EXCLUDED.name,
    total_price = EXCLUDED.total_price,
    total_tdp_watts = EXCLUDED.total_tdp_watts;

DELETE FROM build_components WHERE build_id = 3;
INSERT INTO build_components (build_id, component_id, quantity) VALUES
(3, 24, 1),  -- Intel Core i7-13700K
(3, 8, 1),   -- ASUS PRIME Z790-P WIFI
(3, 12, 1),  -- Gigabyte GeForce RTX 4070 SUPER
(3, 10, 1),  -- Kingston Fury Beast 32GB DDR5
(3, 14, 1),  -- Samsung 980 1TB SSD
(3, 17, 1),  -- Thermalright Peerless Assassin 120 SE
(3, 19, 1),  -- DeepCool CC560
(3, 21, 1)   -- Chieftec PPG-850-C
ON CONFLICT (build_id, component_id) DO NOTHING;

SELECT setval('builds_id_seq', (SELECT MAX(id) FROM builds));

COMMIT;

