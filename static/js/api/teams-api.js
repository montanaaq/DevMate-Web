import { api } from "./client.js";

export async function getTeamCatalog() {
    const { data } = await api.get("/teams/catalog");
    return data;
}

export async function searchProjects(skillIds, before) {
    const { data } = await api.post("/teams/search", { skillIds, before });
    return data;
}

export function createTeam(data) {
    return api.post("/teams", data);
}

export function applyToTeam(teamId, data) {
    return api.post(`/teams/${teamId}/applications`, data);
}

export function resolveTeamApplication(teamId, applicationId, decision) {
    return api.post(`/teams/${teamId}/applications/${applicationId}`, { decision });
}
