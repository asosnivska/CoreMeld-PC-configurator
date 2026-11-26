-- Seed 004: fill power connector and board limit columns added by
-- migrations 002_add_case_board_limits and 003_add_power_connectors (#67).
-- Rows from seed 002 had NULL there, so the engine skipped them.
-- Only UPDATEs of existing rows: no new ids, safe to run more than once.
-- Run after seed 003.
--
-- power_connector_type: 0 = none, 1 = 6-pin, 2 = 8-pin, 3 = 12VHPWR (16-pin)
-- form_factors:         1 = ATX, 2 = Micro-ATX, 3 = Mini-ITX

BEGIN;

-- GPUs
-- MSI GeForce GTX 1650 D6 Ventus XS: 1 x 6-pin (MSI datasheet)
UPDATE gpus SET power_connector_type = 1, power_connector_count = 1
WHERE component_id = 11;

-- Gigabyte RTX 4070 SUPER WINDFORCE OC: 1 x 16-pin 12VHPWR
UPDATE gpus SET power_connector_type = 3, power_connector_count = 1
WHERE component_id = 12;

-- Power supplies
-- Chieftec Polaris 3.0 550W (ATX 3.0): 2 x PCIe 8-pin, 1 x 16-pin
UPDATE power_supplies SET pcie_8pin_count = 2, has_12vhpwr = TRUE
WHERE component_id = 20;

-- Chieftec PPG-850-C (Vega M, ATX 3.1): 4 x PCIe 8-pin, 1 x 16-pin
UPDATE power_supplies SET pcie_8pin_count = 4, has_12vhpwr = TRUE
WHERE component_id = 21;

-- Cases: largest supported motherboard
-- Gamemax Destroyer MB: Micro-ATX
UPDATE cases SET max_board_form_factor_id = 2
WHERE component_id = 18;

-- DeepCool CC560: ATX
UPDATE cases SET max_board_form_factor_id = 1
WHERE component_id = 19;

COMMIT;
