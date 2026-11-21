import { APPLICATION_STATUSES } from "@shared/constants/applications.js";
import { $, createLink, plural, textElement } from "@shared/dom.js";

function createRow({ name, meta, status, className, body }) {
    const row = document.createElement("details");
    row.className = className;
    const heading = document.createElement("span");
    heading.className = "profile-row-heading";
    heading.append(
        textElement("span", name, "profile-row-title"),
        textElement("span", status, "profile-status"),
    );
    const summary = document.createElement("summary");
    summary.append(heading, textElement("span", meta, "profile-meta"));
    row.append(summary, ...body);
    return row;
}

function renderList({ container, counter, items, empty, toRow }) {
    counter.textContent = items.length;
    container.replaceChildren(...(items.length ? items.map(toRow) : [empty]));
}

export function renderProjects(teams, renderIncoming, openProjectIds) {
    renderList({
        container: $("profile-teams"),
        counter: $("team-count"),
        items: teams,
        empty: textElement("p", "No projects yet.", "profile-empty"),
        toRow: (team) => {
            const body = [textElement("p", team.description, "project-body")];
            if (team.incomingApplications) {
                body.push(renderIncoming(team));
            }

            const row = createRow({
                name: team.name,
                meta: [
                    team.role,
                    ...team.positions,
                    plural(team.memberCount, "member", "members"),
                ].join(" · "),
                status: team.status,
                className: "project-row",
                body,
            });
            row.id = `project-${team.id}`;
            row.open = openProjectIds.has(row.id);
            return row;
        },
    });
}

export function formatDate(value) {
    return new Date(value).toLocaleDateString("en", {
        day: "numeric",
        month: "short",
        year: "numeric",
    });
}

function createEmptyApplications() {
    const empty = textElement("p", "No applications yet. ", "profile-empty");
    empty.append(createLink("Find a team", "/"));
    return empty;
}

export function renderApplications(applications) {
    renderList({
        container: $("profile-applications"),
        counter: $("application-count"),
        items: applications,
        empty: createEmptyApplications(),
        toRow: (application) => {
            const body = [
                textElement("p", application.description, "project-body"),
            ];
            if (application.resolutionReason) {
                body.push(
                    textElement("p", application.resolutionReason, "project-body"),
                );
            }
            if (application.coverLetter) {
                body.push(
                    textElement("h3", "Your message", "profile-meta"),
                    textElement("p", application.coverLetter, "project-body"),
                );
            }
            return createRow({
                name: application.teamName,
                meta: `${application.position} · ${formatDate(application.createdAt)}`,
                status:
                    APPLICATION_STATUSES[application.status] ??
                    application.status,
                className: "application-row",
                body,
            });
        },
    });
}
