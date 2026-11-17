import { plural, textElement } from "../../../shared/dom.js";

export function renderTeams(container, teams) {
    for (const team of teams) {
        const row = document.createElement("article");
        row.className = "team-row";
        row.append(
            textElement("h3", team.name),
            textElement("p", team.description, "team-description"),
            textElement("p", `Looking for: ${team.positions.join(", ")}`),
            textElement(
                "p",
                `Required skills: ${team.requiredSkills.join(", ") || "Not specified"}`,
                "team-skills",
            ),
            textElement(
                "p",
                `${plural(team.memberCount, "member", "members")} · ${plural(team.positions.length, "open position", "open positions")}`,
                "team-meta",
            ),
        );
        container.append(row);
    }
}
