export function updateCharacterCount(field) {
    field.parentElement.querySelector("[data-character-count]").textContent =
        `${Math.max(0, field.maxLength - field.value.length)} left`;
}

export function reportFirstInvalid(section) {
    const invalid = [
        ...section.querySelectorAll("input, textarea, select"),
    ].find((field) => !field.checkValidity());
    if (!invalid) {
        return true;
    }
    invalid.setAttribute("aria-invalid", "true");
    if (!section.hidden) {
        invalid.reportValidity();
    }
    return false;
}
