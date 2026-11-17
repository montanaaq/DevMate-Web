import { getCurrentUser } from "../../api/auth-api.js";
import { errorMessages } from "../../shared/error-messages.js";
import { showError } from "../../shared/ui.js";
import { renderAccount } from "./components/header.js";

const errorSummary = document.getElementById("account-error");

async function loadAccount() {
    try {
        renderAccount(await getCurrentUser());
    } catch {
        showError(errorSummary, errorMessages.requestFailed);
    }
}

loadAccount();
