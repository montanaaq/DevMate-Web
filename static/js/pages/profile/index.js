import { getCurrentUser, getProfile, signOut } from "../../api/auth-api.js";
import { errorMessages } from "../../shared/constants/error-messages.js";
import { clearError, showError } from "../../shared/ui.js";
import { initAccent } from "./accent.js";
import { $ } from "../../shared/dom.js";
import { renderApplications } from "./render.js";
import { initSocialLinks, renderLinks } from "./social-links.js";

const errorSummary = document.getElementById("account-error");
const logout = $("signout");
const retry = $("retry");
const loading = $("loading");

function selectSection() {
    const selected =
        window.location.hash === "#settings" ? "settings" : "overview";
    for (const section of document.querySelectorAll("[data-section]")) {
        section.hidden = section.dataset.section !== selected;
    }
    for (const link of document.querySelectorAll("[data-section-link]")) {
        if (link.dataset.sectionLink === selected) {
            link.setAttribute("aria-current", "page");
        } else {
            link.removeAttribute("aria-current");
        }
    }
}

function renderProfile(user, profile) {
    $("profile-name").textContent = user.fullName;
    $("profile-email").textContent = user.email;

    const skills = $("profile-skills");
    skills.textContent = profile.skills.join(" · ");
    skills.hidden = profile.skills.length === 0;

    renderLinks(profile.links);
    renderApplications(profile.applications);
    $("profile").hidden = false;
}

async function loadProfile() {
    retry.hidden = true;
    loading.hidden = false;
    clearError(errorSummary);
    try {
        const user = await getCurrentUser();
        if (user === null) {
            window.location.replace("/signin");
            return;
        }
        logout.disabled = false;
        $("profile-role").textContent = user.role;
        renderProfile(user, await getProfile());
    } catch {
        showError(errorSummary, errorMessages.requestFailed);
        retry.hidden = false;
    } finally {
        loading.hidden = true;
    }
}

async function handleLogout() {
    logout.disabled = true;
    clearError(errorSummary);
    try {
        await signOut();
        window.location.replace("/");
    } catch {
        showError(errorSummary, errorMessages.requestFailed);
        logout.disabled = false;
    }
}

logout.addEventListener("click", handleLogout);
retry.addEventListener("click", loadProfile);
window.addEventListener("hashchange", selectSection);

initSocialLinks();
initAccent();
selectSection();
loadProfile();
