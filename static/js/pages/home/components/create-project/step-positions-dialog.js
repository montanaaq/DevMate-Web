import { MAX_PEOPLE } from "@shared/constants/limits.js";
import { clearError, showError } from "@shared/ui.js";
import {
    createPosition,
    totalPlaces,
    hasCaptain,
    formatPlaces,
    toggleCaptain,
} from "./draft.js";
import { reportFirstInvalid, updateCharacterCount } from "./form-fields.js";

export function createPositionsStep(section, draft, catalog) {
    const people = section.querySelector("#project-positions");
    const add = section.querySelector("#add-project-position");
    const countLabel = section.querySelector("#project-people-count");
    const peopleError = section.querySelector("#project-people-error");
    const template = document.getElementById("project-position-template");

    function updatePeopleCount() {
        const count = totalPlaces(draft);
        const includesCaptain = hasCaptain(draft);
        countLabel.textContent = `${formatPlaces(count)} · ${count - Number(includesCaptain)} people to recruit${includesCaptain ? " · You are included" : " · You are not assigned to a position"}`;
        if (count <= MAX_PEOPLE) {
            clearError(peopleError);
        }
        add.disabled = draft.positions.length >= MAX_PEOPLE;
    }

    function renderPeople() {
        people.replaceChildren();

        draft.positions.forEach((position, index) => {
            const row = template.content.firstElementChild.cloneNode(true);
            row.dataset.index = String(index);
            row.querySelector("legend").textContent = `Position ${index + 1}`;

            for (const field of row.querySelectorAll("[data-field]")) {
                const key = field.dataset.field;
                field.id = `project-position-${index}-${key}`;
                row.querySelector(`[data-label="${key}"]`).htmlFor = field.id;
                if (key === "categoryId") {
                    field.add(new Option("Choose a category", ""));
                    for (const category of catalog.categories) {
                        field.add(
                            new Option(category.name, String(category.id)),
                        );
                    }
                }
                field.value = position[key] || "";
                if (field.hasAttribute("data-counted")) {
                    const counter = field.parentElement.querySelector(
                        "[data-character-count]",
                    );
                    counter.id = `${field.id}-count`;
                    field.setAttribute("aria-describedby", counter.id);
                    updateCharacterCount(field);
                }
            }

            const captain = row.querySelector("[data-include-captain]");
            captain.textContent = position.includeCaptain
                ? "Remove yourself"
                : "Add yourself";
            captain.setAttribute(
                "aria-pressed",
                String(position.includeCaptain),
            );
            row.querySelector("[data-remove-position]").disabled =
                draft.positions.length === 1;
            people.append(row);
        });

        updatePeopleCount();
    }

    function addPosition() {
        if (draft.positions.length >= MAX_PEOPLE) {
            return;
        }
        draft.positions.push(createPosition());
        renderPeople();
    }

    people.addEventListener("input", (event) => {
        const field = event.target.closest("[data-field]");
        if (!field) {
            return;
        }
        const position =
            draft.positions[
                Number(field.closest("[data-index]").dataset.index)
            ];
        position[field.dataset.field] =
            field.dataset.field === "title"
                ? field.value.trim()
                : Number(field.value);
        updatePeopleCount();
    });
    people.addEventListener("click", (event) => {
        const button = event.target.closest("button");
        if (!button) {
            return;
        }
        const index = Number(button.closest("[data-index]").dataset.index);
        if (
            button.hasAttribute("data-remove-position") &&
            draft.positions.length > 1
        ) {
            draft.positions.splice(index, 1);
            renderPeople();
            people.querySelector('[data-field="title"]').focus();
        } else if (button.hasAttribute("data-include-captain")) {
            toggleCaptain(draft, index);
            renderPeople();
            people.children[index]
                .querySelector("[data-include-captain]")
                .focus();
        }
    });
    add.addEventListener("click", () => {
        addPosition();
        people.lastElementChild.querySelector("input").focus();
    });

    return {
        section,
        enter() {
            if (!draft.positions.length) {
                draft.positions.push(createPosition());
            }
            renderPeople();
        },
        validate() {
            for (const field of people.querySelectorAll(
                '[data-field="title"]',
            )) {
                field.setCustomValidity(
                    field.value.trim() ? "" : "Enter a position title.",
                );
            }

            if (!reportFirstInvalid(section)) {
                return false;
            }

            if (totalPlaces(draft) > MAX_PEOPLE) {
                showError(
                    peopleError,
                    `Use up to ${MAX_PEOPLE} places in total.`,
                );
                if (!section.hidden) {
                }
                return false;
            }

            clearError(peopleError);
            return true;
        },
        reset() {
            people.replaceChildren();
            clearError(peopleError);
            updatePeopleCount();
        },
    };
}
