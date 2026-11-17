import { getCurrentUser, signOut } from "../../api/auth-api.js";
import { errorMessages } from "../../shared/error-messages.js";
import { clearError, showError } from "../../shared/ui.js";

const errorSummary = document.getElementById("account-error");
const logout = document.querySelector("[data-signout]");

async function loadProfile() {
    try {
        const user = await getCurrentUser();
        if (user === null) {
            window.location.replace("/signin");
            return;
        }

        document.querySelector("[data-profile-name]").textContent = user.fullName;
        document.querySelector("[data-profile-email]").textContent = user.email;
        document.querySelector("[data-profile-role]").textContent = user.role;
        document.querySelector("[data-profile]").hidden = false;
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

logout.disabled = false;
loadProfile();
