import { createSkillPicker } from "../skill-picker.js";
import { formatPlaces } from "./draft.js";
import { reportFirstInvalid } from "./form-fields.js";

export function createSkillsStep(section, draft, catalog) {
    const skillGroups = section.querySelector("#project-position-skills");
    const template = document.getElementById("project-skill-template");
    let pickers = [];

    function reset() {
        for (const picker of pickers) {
            picker.destroy();
        }

        pickers = [];
        skillGroups.replaceChildren();
    }

    function enter() {
        reset();
        draft.positions.forEach((position, index) => {
            const group = template.content.firstElementChild.cloneNode(true);
            group.querySelector("legend").textContent =
                `${position.title} · ${formatPlaces(position.count)}`;

            if (position.includeCaptain) {
                group.querySelector("legend").textContent += " · including you";
            }

            const input = group.querySelector("input");
            const list = group.querySelector('[role="listbox"]');
            input.id = `project-skills-${index}`;
            list.id = `${input.id}-suggestions`;
            input.setAttribute("aria-controls", list.id);
            group.querySelector("label").htmlFor = input.id;

            skillGroups.append(group);

            pickers.push(
                createSkillPicker(
                    group,
                    catalog.skills,
                    (ids) => {
                        position.skillIds = ids;
                        input.setCustomValidity("");
                    },
                    position.skillIds,
                ),
            );
        });
    }

    return {
        section,
        enter,
        validate() {
            const inputs = [...skillGroups.querySelectorAll("input")];

            for (const [index, input] of inputs.entries()) {
                let message = "";
                if (input.value.trim()) {
                    message =
                        "Choose a skill from the suggestions or clear the search.";
                } else if (!draft.positions[index].skillIds.length) {
                    message = "Choose at least one required skill.";
                }

                input.setCustomValidity(message);
            }

            return reportFirstInvalid(section);
        },
        reset,
    };
}
