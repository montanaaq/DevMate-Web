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
            input.id = `project-skills-${index}`;
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
                input.setCustomValidity(
                    draft.positions[index].skillIds.length
                        ? ""
                        : "Choose at least one required skill.",
                );
            }

            return reportFirstInvalid(section);
        },
        reset,
    };
}
