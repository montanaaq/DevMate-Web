import { createLink, plural, textElement } from "../../../shared/dom.js";
import { openApplicationDialog } from "./application-dialog.js";

export function renderTeams(container, teams, isAuthenticated, refreshTeams) {
    for (const team of teams) {
        const row = document.createElement("article");
        row.className = "team-row";
        const heading = textElement("h3", team.name);
        heading.id = `team-heading-${team.id}`;
        row.append(
            heading,
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
        if (!isAuthenticated) {
            row.append(createLink("Sign in to apply", "/signin"));
        } else if (team.isMember) {
            row.append(textElement("p", "Your team", "team-meta"));
        } else if (
            team.applicationPositions.some(
                (position) => !position.hasPendingApplication,
            )
        ) {
            const button = textElement(
                "button",
                "Apply to join",
                "btn btn-primary",
            );
            button.type = "button";
            button.setAttribute("aria-haspopup", "dialog");
            button.setAttribute("aria-describedby", heading.id);
            button.addEventListener("click", () =>
                openApplicationDialog(team, button, refreshTeams),
            );
            row.append(button);
        } else {
            row.append(createLink("Application pending", "/profile"));
        }
        container.append(row);
    }
}
