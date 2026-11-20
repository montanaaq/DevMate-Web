CREATE TABLE IF NOT EXISTS public.skill_relations (
    first_skill_id INTEGER NOT NULL REFERENCES public.skills(id) ON DELETE RESTRICT,
    second_skill_id INTEGER NOT NULL REFERENCES public.skills(id) ON DELETE RESTRICT,
    PRIMARY KEY (first_skill_id, second_skill_id),
    CHECK (first_skill_id < second_skill_id)
);
CREATE INDEX IF NOT EXISTS skill_relations_second_skill_idx ON public.skill_relations (second_skill_id);