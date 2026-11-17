import { getCurrentUser, signOut } from "../../api/auth-api.js";
import { errorMessages } from "../../shared/error-messages.js";
import { clearError, showError } from "../../shared/ui.js";
import { renderAccount } from "./components/header.js";

const errorSummary = document.getElementById("account-error");
const logout = document.querySelector("[data-signout]");

async function loadAccount() {
    try {
        renderAccount(await getCurrentUser());
    } catch {
        showError(errorSummary, errorMessages.requestFailed);
    }
}

logout.addEventListener("click", async () => {
    logout.disabled = true;
    clearError(errorSummary);
    try {
        await signOut();
        window.location.replace("/");
    } catch {
        showError(errorSummary, errorMessages.requestFailed);
        errorSummary.focus();
    } finally {
        logout.disabled = false;
    }
});

loadAccount();
