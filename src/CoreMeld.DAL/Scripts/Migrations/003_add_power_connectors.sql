BEGIN;

ALTER TABLE gpus
    ADD COLUMN IF NOT EXISTS power_connector_type INT
    CHECK (power_connector_type BETWEEN 0 AND 3),
    ADD COLUMN IF NOT EXISTS power_connector_count INT
    CHECK (power_connector_count >= 0);

ALTER TABLE power_supplies
    ADD COLUMN IF NOT EXISTS pcie_8pin_count INT
    CHECK (pcie_8pin_count >= 0),
    ADD COLUMN IF NOT EXISTS has_12vhpwr BOOLEAN;

COMMIT;