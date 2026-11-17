export function renderTeams(container, teams) {
    for (const team of teams) {
        const row = document.createElement("article");
        row.className = "team-row";

        const heading = document.createElement("h3");
        heading.textContent = team.name;

        const description = document.createElement("p");
        description.className = "team-description";
        description.textContent = team.description;

        const positions = document.createElement("p");
        positions.textContent = `Looking for: ${team.positions.join(", ")}`;

        const skills = document.createElement("p");
        skills.className = "team-skills";
        skills.textContent = `Required skills: ${team.requiredSkills.join(", ") || "Not specified"}`;

        const members = document.createElement("p");
        members.className = "team-meta";
        members.textContent = `${team.memberCount} ${team.memberCount === 1 ? "member" : "members"} · ${team.positions.length} ${team.positions.length === 1 ? "open position" : "open positions"}`;

        row.append(heading, description, positions, skills, members);
        container.append(row);
    }
}
