import { api } from "./client.js";

export async function getTeamCatalog() {
    const { data } = await api.get("/teams/catalog");
    return data;
}

export async function getTeams(skillIds, before) {
    const params = new URLSearchParams({ skills: skillIds.join(",") });

    if (before !== undefined) {
        params.set("before", String(before));
    }

    const { data } = await api.get(`/teams?${params}`);
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
