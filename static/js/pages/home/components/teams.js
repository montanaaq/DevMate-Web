import { applyToTeam } from "../../../api/teams-api.js";
import { createLink, plural, textElement } from "../../../shared/dom.js";
import { clearError, showError } from "../../../shared/ui.js";

function createApplicationForm(team, headingId) {
    const details = document.createElement("details");
    const summary = textElement("summary", "Apply to join");
    summary.id = `apply-to-team-${team.id}`;

    const form = document.createElement("form");
    form.method = "post";
    form.action = `/api/teams/${team.id}/applications`;
    form.setAttribute("aria-labelledby", `${summary.id} ${headingId}`);

    const position = document.createElement("select");
    position.id = `application-position-${team.id}`;
    position.name = "positionId";
    position.className = "form-select";
    position.required = true;
    position.append(new Option("Choose a position", "", true, true));
    for (const [index, title] of team.positions.entries()) {
        position.append(new Option(title, team.positionIds[index]));
    }

    const letter = document.createElement("textarea");
    letter.id = `application-letter-${team.id}`;
    letter.name = "coverLetter";
    letter.className = "form-control";
    letter.maxLength = 2000;
    letter.rows = 3;

    const positionLabel = textElement("label", "Position (required)", "form-label");
    positionLabel.htmlFor = position.id;
    const letterLabel = textElement(
        "label",
        "Cover letter (optional, up to 2,000 characters)",
        "form-label",
    );
    letterLabel.htmlFor = letter.id;

    const submit = textElement("button", "Send application", "btn btn-primary");
    submit.type = "submit";
    const error = document.createElement("div");
    error.className = "alert alert-danger";
    error.setAttribute("role", "alert");
    error.tabIndex = -1;
    error.hidden = true;

    const status = document.createElement("div");
    status.setAttribute("role", "status");
    status.tabIndex = -1;

    form.append(positionLabel, position, letterLabel, letter, submit, error);
    details.append(summary, form, status);

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        if (submit.disabled) {
            return;
        }

        clearError(error);
        status.replaceChildren();
        submit.disabled = true;
        const selected = position.selectedOptions[0];
        position.disabled = true;
        letter.readOnly = true;

        try {
            await applyToTeam(team.id, {
                positionId: Number(selected.value),
                coverLetter: letter.value,
            });
            selected.remove();
            form.reset();
            form.hidden = position.options.length === 1;
            status.append(
                `Application sent for ${selected.text}. `,
                createLink("View in profile", "/profile"),
            );
            status.focus();
        } catch (cause) {
            showError(error, cause.message);
            error.focus();
        } finally {
            position.disabled = false;
            letter.readOnly = false;
            submit.disabled = false;
        }
    });

    return details;
}

export function renderTeams(container, teams, user) {
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
        if (user) {
            row.append(createApplicationForm(team, heading.id));
        } else if (user === null) {
            row.append(createLink("Sign in to apply", "/signin"));
        }
        container.append(row);
    }
}
