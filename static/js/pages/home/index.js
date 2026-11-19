import { getCurrentUser } from "../../api/auth-api.js";
import { getTeamCatalog, getTeams } from "../../api/teams-api.js";
import { errorMessages } from "../../shared/constants/error-messages.js";
import { showError } from "../../shared/ui.js";
import { initCreateProject } from "./components/create-project/index.js";
import { renderAccount } from "./components/header.js";
import { createSkillPicker } from "./components/skill-picker.js";
import { renderTeams } from "./components/teams.js";

const errorSummary = document.getElementById("account-error");

async function loadAccount() {
    try {
        const user = await getCurrentUser();
        renderAccount(user);
        return user;
    } catch {
        showError(errorSummary, errorMessages.requestFailed);
    }
}

const accountRequest = loadAccount();
const searchForm = document.getElementById("search-form");
const searchInput = document.getElementById("skill-query");
const searchSubmit = document.getElementById("search-submit");
const list = document.getElementById("team-list");
const status = document.getElementById("teams-status");
const error = document.getElementById("teams-error");
const more = document.getElementById("more-teams");
let searchPicker;
let catalogRequest;
let requestVersion = 0;
let before;
let resultCount = 0;

async function loadCatalog() {
    const catalog = await getTeamCatalog();
    searchPicker = createSkillPicker(
        document.getElementById("search-skills"),
        catalog.skills,
        () => loadTeams(),
    );
    searchInput.disabled = false;
    searchSubmit.disabled = false;
    return catalog;
}

function ensureCatalog() {
    catalogRequest ??= loadCatalog().catch((cause) => {
        catalogRequest = undefined;
        throw cause;
    });
    return catalogRequest;
}

async function loadTeams(append = false) {
    const version = ++requestVersion;
    error.hidden = true;
    more.hidden = true;

    if (!append) {
        before = undefined;
        resultCount = 0;
        list.replaceChildren();
    }

    status.textContent = "Loading projects…";

    try {
        await ensureCatalog();
        const data = await getTeams(searchPicker.ids, before);

        if (version !== requestVersion) {
            return;
        }

        renderTeams(list, data.teams, data.isAuthenticated, loadTeams);
        resultCount += data.teams.length;
        before = data.teams.at(-1)?.id;
        more.hidden = !data.hasMore;

        if (resultCount > 0) {
            status.textContent = `Projects found: ${resultCount}.`;
        } else {
            status.textContent = "No projects found.";
        }
    } catch (cause) {
        if (version !== requestVersion) {
            return;
        }

        status.textContent = "";
        document.getElementById("teams-error-message").textContent =
            cause.message || errorMessages.requestFailed;
        error.hidden = false;
    }
}

searchForm.addEventListener("submit", (event) => {
    event.preventDefault();
    if (event.isComposing) {
        return;
    }
    if (searchInput.value.trim()) {
        status.textContent = "Choose a skill from the suggestions first.";
        searchInput.focus();
        return;
    }
    loadTeams();
});

document.getElementById("reset-search").addEventListener("click", () => {
    searchPicker?.clear();
    searchInput.focus();
});
document
    .getElementById("retry-teams")
    .addEventListener("click", () => loadTeams());
more.addEventListener("click", () => loadTeams(true));
document
    .querySelector(".team-nav a")
    .addEventListener("click", () => searchInput.focus());

initCreateProject({
    accountRequest,
    ensureCatalog,
    onCreated: async () => {
        await searchPicker.clear();
        status.textContent = "Project published.";
    },
});

loadTeams();
