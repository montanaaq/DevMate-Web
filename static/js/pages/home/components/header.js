export function renderAccount(user) {
    const account = document.querySelector("[data-account]");
    account.querySelector("[data-user-name]").textContent = user?.fullName ?? "";
    account.hidden = user === null;
    document.querySelector("[data-guest-links]").hidden = user !== null;
}
