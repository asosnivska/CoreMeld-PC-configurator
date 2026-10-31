BEGIN;

ALTER TABLE cases
    ADD COLUMN IF NOT EXISTS max_board_form_factor_id INT
    REFERENCES form_factors(id) ON DELETE RESTRICT;

COMMIT;