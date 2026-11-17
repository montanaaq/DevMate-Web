import { signIn, signUp } from "../../api/auth-api.js";
import { errorMessages } from "../../shared/constants/error-messages.js";
import { clearError } from "../../shared/ui.js";
import {
    clearFieldError,
    clearFormErrors,
    setFormPending,
    showFormError,
} from "./components/auth-form.js";

const form = document.querySelector("[data-auth-form]");
const errorSummary = document.getElementById("form-error");
const password = form.elements.namedItem("password");
const passwordToggle = form.querySelector("[data-password-toggle]");
const submit = form.querySelector('[type="submit"]');
const buttonLabel = submit.textContent.trim();
const isSignUp = form.dataset.authForm === "signup";
const submitRequest = isSignUp ? signUp : signIn;
let pending = false;

passwordToggle.hidden = false;
passwordToggle.addEventListener("click", () => {
    const visible = password.type === "password";
    password.type = visible ? "text" : "password";
    passwordToggle.setAttribute("aria-pressed", String(visible));
});

form.addEventListener("input", (event) => {
    if (event.target instanceof HTMLInputElement) {
        event.target.setCustomValidity("");
        clearFieldError(event.target);
    }
    clearError(errorSummary);
});

form.addEventListener(
    "invalid",
    (event) => {
        if (event.target instanceof HTMLInputElement) {
            event.target.setCustomValidity(
                event.target.validity.valueMissing
                    ? errorMessages.requiredField
                    : errorMessages.invalidField,
            );
        }
    },
    true,
);

form.addEventListener("submit", async (event) => {
    event.preventDefault();
    if (pending) {
        return;
    }

    clearFormErrors(form, errorSummary);
    pending = true;
    setFormPending(form, true, isSignUp ? "Creating account…" : "Signing in…");

    try {
        await submitRequest(Object.fromEntries(new FormData(form)));
        password.value = "";
        window.location.assign(isSignUp ? "/signin?registered=1" : "/");
    } catch (error) {
        showFormError(form, errorSummary, error);
    } finally {
        pending = false;
        setFormPending(form, false, buttonLabel);
    }
});

submit.disabled = false;
const notice = document.getElementById("registration-notice");
if (
    notice &&
    new URLSearchParams(window.location.search).get("registered") === "1"
) {
    notice.hidden = false;
}
