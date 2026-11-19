import { HttpError } from "../../../shared/http.js";
import { errorMessages } from "../../../shared/constants/error-messages.js";
import { clearError, showError } from "../../../shared/ui.js";

export function clearFieldError(input) {
    input.classList.remove("is-invalid");
    input.removeAttribute("aria-invalid");
    document.getElementById(`${input.name}-error`).textContent = "";
}

export function clearFormErrors(form, summary) {
    clearError(summary);
    for (const input of form.querySelectorAll("input")) {
        clearFieldError(input);
    }
}

export function showFormError(form, summary, error) {
    const fieldErrors = error instanceof HttpError ? error.body?.errors : null;

    if (Array.isArray(fieldErrors)) {
        for (const fieldError of fieldErrors) {
            if (
                typeof fieldError?.field !== "string" ||
                typeof fieldError.message !== "string"
            ) {
                continue;
            }

            const input = form.elements.namedItem(fieldError.field);
            const feedback = document.getElementById(
                `${fieldError.field}-error`,
            );
            if (input instanceof HTMLInputElement && feedback) {
                input.classList.add("is-invalid");
                input.setAttribute("aria-invalid", "true");
                if (!feedback.textContent) {
                    feedback.textContent = fieldError.message;
                }
            }
        }
    }

    showError(
        summary,
        error instanceof Error ? error.message : errorMessages.requestFailed,
    );
}

export function setFormPending(form, pending, label) {
    const submit = form.querySelector('[type="submit"]');
    submit.disabled = pending;
    submit.textContent = label;
}
