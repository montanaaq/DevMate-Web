import { $ } from "../../../shared/dom.js";

export function renderAccount(user) {
    const account = $("account");
    $("user-name", account).textContent = user?.fullName ?? "";
    account.hidden = user === null;
    $("guest-links").hidden = user !== null;
}
