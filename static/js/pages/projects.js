import { getProfile } from "../api/auth-api.js";
import { getTeamCatalog, searchProjects } from "../api/teams-api.js";
import { errorMessages } from "../shared/constants/error-messages.js";
import {
    loadProjectSkillFilters,
    saveProjectSkillFilters,
} from "../shared/project-skill-filters.js";
import { clearError, showError } from "../shared/ui.js";
import { openApplicationDialog } from "./home/components/application-dialog.js";
import { initCreateProject } from "./home/components/create-project/create-project-dialog.js";
import { createSkillPicker } from "./home/components/skill-picker.js";

const form = document.getElementById("project-filters");
const fields = form.querySelector("fieldset");
const pickerRoot = document.getElementById("project-skill-picker");
const results = document.getElementById("project-results");
const heading = document.getElementById("teams-heading");
const status = document.getElementById("projects-status");
const error = document.getElementById("projects-error");
const pages = document.getElementById("project-pages");
const firstPage = document.getElementById("first-project-page");
const nextPage = document.getElementById("next-project-page");
let appliedSkills = [];
let currentBefore = null;
let nextBefore = null;
let requestVersion = 0;
let picker;
let catalogRequest;

function ensureCatalog() {
    catalogRequest ??= getTeamCatalog().catch((cause) => {
        catalogRequest = undefined;
        throw cause;
    });
    return catalogRequest;
}

async function loadProjects(skillIds, before = null, focusHeading = false) {
    const version = ++requestVersion;
    clearError(error);
    firstPage.disabled = true;
    nextPage.disabled = true;
    status.classList.remove("visually-hidden");
    status.textContent = "Loading projects…";

    try {
        const data = await searchProjects(skillIds, before);
        if (version !== requestVersion) {
            return;
        }

        results.innerHTML = data.html;
        for (const button of results.querySelectorAll("[data-apply-team]")) {
            button.hidden = false;
        }
        appliedSkills = skillIds;
        currentBefore = before;
        nextBefore = data.nextBefore;
        firstPage.hidden = before === null;
        nextPage.hidden = nextBefore === null;
        pages.hidden = firstPage.hidden && nextPage.hidden;
        const count = results.querySelectorAll("article").length;
        status.classList.toggle("visually-hidden", count === 0);
        status.textContent = count
            ? `Projects on this page: ${count}.`
            : "No projects found.";
        if (focusHeading) {
            heading.focus();
        }
    } catch (cause) {
        if (version === requestVersion) {
            status.textContent = "";
            showError(error, cause.message || errorMessages.requestFailed);
        }
    } finally {
        if (version === requestVersion) {
            firstPage.disabled = false;
            nextPage.disabled = false;
        }
    }
}

function applyFilters() {
    const skillIds = picker.ids;
    saveProjectSkillFilters(skillIds);
    return loadProjects(skillIds);
}

async function initFilters() {
    status.textContent = "Loading projects…";

    try {
        const savedIds = loadProjectSkillFilters();
        const profileRequest =
            savedIds === null && results.dataset.signedIn === "True"
                ? getProfile()
                : null;
        const [catalog, profile] = await Promise.all([
            ensureCatalog(),
            profileRequest,
        ]);
        picker = createSkillPicker(
            pickerRoot,
            catalog.skills,
            () => {},
            savedIds ?? profile?.skillIds ?? [],
        );
        fields.disabled = false;
        await loadProjects(picker.ids);
    } catch (cause) {
        status.textContent = "";
        showError(error, cause.message || errorMessages.requestFailed);
    }
}

form.addEventListener("submit", (event) => {
    event.preventDefault();
    if (picker) {
        applyFilters();
    }
});

document
    .getElementById("reset-project-filters")
    .addEventListener("click", () => {
        picker.clear();
        pickerRoot.querySelector("input").focus();
        applyFilters();
    });

firstPage.addEventListener("click", () =>
    loadProjects(appliedSkills, null, true),
);
nextPage.addEventListener("click", () =>
    loadProjects(appliedSkills, nextBefore, true),
);

results.addEventListener("click", (event) => {
    const button = event.target.closest("[data-apply-team]");
    if (!button) {
        return;
    }

    const projectHeading = document.getElementById(
        button.getAttribute("aria-describedby"),
    );
    const team = {
        id: Number(button.dataset.applyTeam),
        name: projectHeading.textContent,
        applicationPositions: JSON.parse(button.dataset.positions),
    };
    openApplicationDialog(team, button, () =>
        loadProjects(appliedSkills, currentBefore, true),
    );
});

initCreateProject(ensureCatalog);
initFilters();
