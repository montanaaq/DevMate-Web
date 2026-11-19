import { reportFirstInvalid, updateCharacterCount } from "./form-fields.js";

export function createDetailsStep(section, draft) {
    const name = section.querySelector('[name="name"]');
    const description = section.querySelector('[name="description"]');
    const fields = [name, description];

    function updateCounters() {
        fields.forEach(updateCharacterCount);
    }

    return {
        section,
        enter: updateCounters,
        validate() {
            draft.name = name.value.trim();
            draft.description = description.value.trim();

            name.setCustomValidity(draft.name ? "" : "Enter a project name.");
            description.setCustomValidity(
                draft.description ? "" : "Describe your project.",
            );
            return reportFirstInvalid(section);
        },
        reset() {
            for (const field of fields) {
                field.setCustomValidity("");
            }
            updateCounters();
        },
    };
}
