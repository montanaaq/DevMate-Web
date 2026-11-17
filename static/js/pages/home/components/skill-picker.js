import { MAX_SKILLS } from "../../../shared/constants/limits.js";

export function createSkillPicker(root, skills, onChange, initialIds = []) {
    const input = root.querySelector("input");
    const list = root.querySelector('[role="listbox"]');
    const selectedList = root.querySelector("[data-selected-skills]");
    const status = root.querySelector("[data-skill-status]");
    const byId = new Map(skills.map((skill) => [skill.id, skill]));
    const controller = new AbortController();
    const { signal } = controller;
    const supportsPopover = typeof list.showPopover === "function";
    const anchor = `--${input.id}-anchor`;

    list.classList.add("skill-popover");
    input.style.setProperty("anchor-name", anchor);
    list.style.setProperty("position-anchor", anchor);

    if (supportsPopover) {
        list.setAttribute("popover", "manual");
    }

    status.setAttribute("role", "status");
    let selected = [...new Set(initialIds)]
        .filter((id) => byId.has(id))
        .slice(0, MAX_SKILLS);
    let suggestions = [];
    let active = -1;

    function close() {
        if (supportsPopover && list.matches(":popover-open")) {
            list.hidePopover();
        }

        list.hidden = true;
        input.setAttribute("aria-expanded", "false");
        input.removeAttribute("aria-activedescendant");
        active = -1;
    }

    function highlight(index) {
        active = index;

        for (const [optionIndex, option] of [...list.children].entries()) {
            option.setAttribute(
                "aria-selected",
                String(optionIndex === active),
            );
        }

        const option = list.children[active];
        if (option) {
            input.setAttribute("aria-activedescendant", option.id);
            option.scrollIntoView({ block: "nearest" });
        }
    }

    function suggest() {
        const query = input.value.trim().toLocaleLowerCase();
        const selectedIds = new Set(selected);
        suggestions = skills.filter(
            (skill) =>
                !selectedIds.has(skill.id) &&
                skill.name.toLocaleLowerCase().includes(query),
        );

        list.replaceChildren();
        close();

        if (selected.length >= MAX_SKILLS) {
            status.textContent = `You can select up to ${MAX_SKILLS} skills.`;
            return;
        }
        status.textContent = suggestions.length ? "" : "No skills found.";

        for (const [index, skill] of suggestions.entries()) {
            const option = document.createElement("li");
            option.id = `${input.id}-option-${index}`;
            option.setAttribute("role", "option");
            option.setAttribute("aria-selected", "false");
            option.textContent = skill.name;
            list.append(option);
        }

        list.hidden = suggestions.length === 0;
        if (!list.hidden && supportsPopover) {
            list.showPopover();
        }

        input.setAttribute("aria-expanded", String(!list.hidden));
    }

    function renderSelected() {
        selectedList.replaceChildren();

        for (const id of selected) {
            const skill = byId.get(id);
            if (!skill) {
                continue;
            }

            const item = document.createElement("li");

            const remove = document.createElement("button");
            remove.type = "button";
            remove.className = "selected-skill";
            remove.append(document.createTextNode(`${skill.name} `));

            const icon = document.createElement("span");
            icon.className = "ui-icon";
            icon.setAttribute("aria-hidden", "true");
            icon.textContent = "\uf00d";

            remove.append(icon);
            remove.setAttribute("aria-label", `Remove ${skill.name}`);
            remove.dataset.skillId = String(id);

            item.append(remove);
            selectedList.append(item);
        }
    }

    function choose(skill) {
        if (
            !skill ||
            selected.length >= MAX_SKILLS ||
            selected.includes(skill.id)
        ) {
            return;
        }

        selected = [...selected, skill.id];
        input.value = "";
        status.textContent = "";

        renderSelected();
        input.focus();
        close();
        status.textContent = `${skill.name} added.`;
        onChange([...selected]);
    }

    list.addEventListener("pointerdown", (event) => event.preventDefault(), {
        signal,
    });

    list.addEventListener(
        "click",
        (event) => {
            const option = event.target.closest('[role="option"]');
            if (option && option.parentElement === list) {
                choose(suggestions[[...list.children].indexOf(option)]);
            }
        },
        { signal },
    );

    selectedList.addEventListener(
        "click",
        (event) => {
            const button = event.target.closest("button[data-skill-id]");
            if (!button || !selectedList.contains(button)) {
                return;
            }
            const id = selected.find(
                (value) => String(value) === button.dataset.skillId,
            );
            const skill = byId.get(id);

            selected = selected.filter((value) => value !== id);
            renderSelected();
            input.focus();
            close();
            status.textContent = `${skill?.name ?? "Skill"} removed.`;
            onChange([...selected]);
        },
        { signal },
    );

    input.addEventListener("input", suggest, { signal });
    input.closest("dialog")?.addEventListener("close", close, { signal });
    input.addEventListener("focus", suggest, { signal });
    input.addEventListener(
        "blur",
        () => {
            close();
            status.textContent = "";
        },
        { signal },
    );
    input.addEventListener(
        "keydown",
        (event) => {
            if (
                event.isComposing ||
                event.altKey ||
                event.ctrlKey ||
                event.metaKey
            ) {
                return;
            }

            if (event.key === "Escape" && !list.hidden) {
                event.preventDefault();
                event.stopPropagation();
                close();
            } else if (event.key === "ArrowDown" || event.key === "ArrowUp") {
                event.preventDefault();
                if (list.hidden) {
                    suggest();
                }
                if (!list.hidden) {
                    let nextIndex;
                    if (event.key === "ArrowDown") {
                        nextIndex = Math.min(
                            active + 1,
                            suggestions.length - 1,
                        );
                    } else if (active < 0) {
                        nextIndex = suggestions.length - 1;
                    } else {
                        nextIndex = Math.max(active - 1, 0);
                    }

                    highlight(nextIndex);
                }
            } else if (event.key === "Enter" && !list.hidden && active >= 0) {
                event.preventDefault();
                choose(suggestions[active]);
            }
        },
        { signal },
    );

    renderSelected();

    return {
        get ids() {
            return [...selected];
        },
        clear() {
            selected = [];
            input.value = "";
            status.textContent = "";
            renderSelected();
            close();
            return onChange([]);
        },
        destroy() {
            controller.abort();
            close();
        },
    };
}
