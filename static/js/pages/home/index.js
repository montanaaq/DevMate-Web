import { getTeamCatalog } from "../../api/teams-api.js";
import { errorMessages } from "../../shared/constants/error-messages.js";
import { saveProjectSkillFilters } from "../../shared/project-skill-filters.js";
import { initCreateProject } from "./components/create-project/create-project-dialog.js";
import { createSkillPicker } from "./components/skill-picker.js";

const searchForm = document.getElementById("search-form");
const searchInput = document.getElementById("skill-query");
const searchSubmit = document.getElementById("search-submit");
const searchSkills = document.getElementById("search-skills");
const searchStatus = searchSkills.querySelector("[data-skill-status]");
let searchPicker;
let catalogRequest;

async function loadCatalog() {
    const catalog = await getTeamCatalog();
    searchPicker = createSkillPicker(searchSkills, catalog.skills, () => {});
    searchSkills.disabled = false;
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

document.getElementById("reset-search").addEventListener("click", () => {
    searchPicker?.clear();
    searchInput.focus();
});

searchForm.addEventListener("submit", (event) => {
    event.preventDefault();
    if (!searchPicker) {
        return;
    }

    saveProjectSkillFilters(searchPicker.ids);
    window.location.assign(searchForm.action);
});

initCreateProject(ensureCatalog);
ensureCatalog().catch((cause) => {
    searchStatus.textContent = cause.message || errorMessages.requestFailed;
});
