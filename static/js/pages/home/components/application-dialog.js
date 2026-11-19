import { applyToTeam } from "../../../api/teams-api.js";
import { createLink, textElement } from "../../../shared/dom.js";
import { clearError, showError } from "../../../shared/ui.js";

const dialog = document.getElementById("application-dialog");
const form = document.getElementById("application-form");
const positions = document.getElementById("application-positions");
const letter = document.getElementById("application-letter");
const submit = form.querySelector("button[type=submit]");
const close = document.getElementById("close-application");
const error = document.getElementById("application-error");
const status = document.getElementById("application-status");

close.addEventListener("click", () => dialog.close());
dialog.addEventListener("cancel", (event) => {
    if (submit.disabled) {
        event.preventDefault();
    }
});

function positionChoice(position, selected) {
    const radio = document.createElement("input");
    radio.type = "radio";
    radio.name = "positionId";
    radio.value = position.id;
    radio.disabled = position.hasPendingApplication;
    radio.required = true;
    radio.checked = selected;

    const label = document.createElement("label");
    label.className = "application-position";
    label.append(
        radio,
        textElement("span", `${position.title} · ${position.category}`),
        textElement(
            "small",
            position.skills.join(", ") || "No required skills",
        ),
    );
    if (position.hasPendingApplication) {
        label.append(textElement("small", "Application pending"));
    }
    return label;
}

function setSending(sending) {
    positions.disabled = sending;
    letter.readOnly = sending;
    submit.disabled = sending;
    close.disabled = sending;
    submit.textContent = sending ? "Sending…" : "Send application";
}

export function openApplicationDialog(team, trigger, refreshTeams) {
    const available = team.applicationPositions.filter(
        (position) => !position.hasPendingApplication,
    );
    const legend = positions.querySelector("legend");
    positions.replaceChildren(
        legend,
        ...team.applicationPositions.map((position) =>
            positionChoice(
                position,
                available.length === 1 && position === available[0],
            ),
        ),
    );
    document.getElementById("application-heading").textContent =
        `Apply to join ${team.name}`;
    form.action = `/api/teams/${team.id}/applications`;
    form.hidden = false;
    letter.value = "";
    setSending(false);
    clearError(error);
    status.replaceChildren();
    form.onsubmit = submitApplication;
    dialog.showModal();

    async function submitApplication(event) {
        event.preventDefault();
        if (submit.disabled) {
            return;
        }

        const id = Number(form.elements.positionId.value);
        const position = available.find((item) => item.id === id);
        if (!position) {
            return;
        }

        clearError(error);
        setSending(true);

        try {
            await applyToTeam(team.id, {
                positionId: id,
                coverLetter: letter.value,
            });
            position.hasPendingApplication = true;
            form.hidden = true;
            const profile = createLink("View in profile", "/profile");
            status.append(`Application sent for ${position.title}. `, profile);
            profile.focus();

            if (available.length === 1) {
                dialog.addEventListener("close", () => {
                    const link = createLink("Application pending", "/profile");
                    trigger.replaceWith(link);
                    link.focus();
                }, { once: true });
            }
        } catch (cause) {
            showError(error, cause.message);
            if (cause.status === 409 || cause.status === 401) {
                form.hidden = true;
                dialog.addEventListener("close", refreshTeams, { once: true });
            }
        } finally {
            setSending(false);
        }
    }
}
