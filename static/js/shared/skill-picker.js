import { MAX_SKILLS } from "./constants/limits.js";

export function createSkillPicker(root, skills, onChange, initialIds = []) {
    const input = root.querySelector("input");
    const suggestions = root.querySelector(".skill-suggestions");
    const selectedList = root.querySelector(".selected-skills");
    const status = root.querySelector("[data-skill-status]");
    const byId = new Map(skills.map((skill) => [skill.id, skill]));
    const controller = new AbortController();
    const { signal } = controller;
    const selected = new Set(initialIds.filter((id) => byId.has(id)));

    function createBadge(skill, isSelected) {
        const item = document.createElement("li");
        const button = document.createElement("button");
        button.type = "button";
        button.className = "btn skill-badge";
        button.dataset.skillId = String(skill.id);
        button.textContent = skill.name;
        button.setAttribute("aria-pressed", String(isSelected));

        const marker = document.createElement("span");
        marker.setAttribute("aria-hidden", "true");
        marker.textContent = isSelected ? " ×" : " +";
        button.append(marker);
        item.append(button);
        return item;
    }

    function renderSuggestions() {
        const query = input.value.trim().toLocaleLowerCase();
        const lastSkill = byId.get([...selected].at(-1));
        const relatedIds = new Set(lastSkill?.relatedSkillIds);

        const matches = skills.filter(
            (skill) =>
                !selected.has(skill.id) &&
                skill.name.toLocaleLowerCase().includes(query),
        );
        const rank = ({ id, name, categoryId }) => {
            const normalized = name.toLocaleLowerCase();
            if (query && normalized === query) {
                return 3;
            }
            if (query && normalized.startsWith(query)) {
                return 2;
            }
            return Number(
                relatedIds.size
                    ? relatedIds.has(id)
                    : lastSkill?.categoryId === categoryId,
            );
        };
        matches.sort((left, right) => rank(right) - rank(left));

        const atLimit = selected.size >= MAX_SKILLS;
        suggestions.replaceChildren(
            ...(atLimit
                ? []
                : matches.map((skill) => createBadge(skill, false))),
        );
        suggestions.scrollTop = 0;
        status.textContent = atLimit
            ? `You can select up to ${MAX_SKILLS} skills.`
            : matches.length
              ? ""
              : "No matching skills.";
    }

    function renderSelection() {
        selectedList.replaceChildren(
            ...[...selected].map((id) => createBadge(byId.get(id), true)),
        );
        renderSuggestions();
    }

    root.addEventListener(
        "click",
        (event) => {
            const button = event.target.closest("button[data-skill-id]");
            if (!button) {
                return;
            }

            const id = Number(button.dataset.skillId);
            const removed = selected.delete(id);
            if (!removed) {
                if (selected.size >= MAX_SKILLS) {
                    return;
                }
                selected.add(id);
                input.value = "";
            }

            renderSelection();
            const next = (removed ? selectedList : suggestions).querySelector(
                "button",
            );
            (next ?? selectedList.querySelector("button") ?? input).focus({
                preventScroll: true,
            });
            onChange([...selected]);
        },
        { signal },
    );

    input.addEventListener(
        "input",
        () => {
            input.setCustomValidity("");
            renderSuggestions();
        },
        { signal },
    );

    input.addEventListener(
        "keydown",
        (event) => {
            if (
                event.key === "Enter" &&
                !event.isComposing &&
                input.value.trim()
            ) {
                event.preventDefault();
                suggestions.querySelector("button")?.click();
            }
        },
        { signal },
    );

    renderSelection();

    return {
        get ids() {
            return [...selected];
        },
        clear() {
            selected.clear();
            input.value = "";
            renderSelection();
            return onChange([]);
        },
        destroy() {
            controller.abort();
        },
    };
}
