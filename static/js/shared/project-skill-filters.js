const SKILLS_STORAGE_KEY = "skill-issue-project-skills";

export function loadProjectSkillFilters() {
    return JSON.parse(localStorage.getItem(SKILLS_STORAGE_KEY));
}

export function saveProjectSkillFilters(ids) {
    localStorage.setItem(SKILLS_STORAGE_KEY, JSON.stringify(ids));
}
