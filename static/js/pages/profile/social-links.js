import { MAX_SOCIAL_LINKS } from "../../shared/constants/limits.js";
import { saveSocialLinks } from "../../api/auth-api.js";
import { errorMessages } from "../../shared/constants/error-messages.js";
import { clearError, showError } from "../../shared/ui.js";
import { $, createLink } from "../../shared/dom.js";

const form = $("social-form");
const fields = $("social-fields");
const rows = $("social-rows");
const add = $("add-social-link");
const error = $("links-error");
const status = $("links-status");

function isValidLink(value) {
    try {
        const url = new URL(value);
        return (
            value.length <= 500 &&
            url.protocol === "https:" &&
            Boolean(url.hostname) &&
            !url.port &&
            !url.username &&
            !url.password
        );
    } catch {
        return false;
    }
}

function clearNetworkErrors() {
    for (const input of rows.querySelectorAll('[name="network"]')) {
        input.setCustomValidity("");
    }
}

function addLinkRow(network = "", url = "") {
    const row = $("social-row-template").content.firstElementChild.cloneNode(
        true,
    );
    row.querySelector('[name="network"]').value = network;
    row.querySelector('[name="url"]').value = url;
    row.querySelector("button").addEventListener("click", () => {
        row.remove();
        clearNetworkErrors();
        status.textContent = "";
        add.disabled = rows.children.length >= MAX_SOCIAL_LINKS;
        add.focus();
    });
    rows.append(row);
    add.disabled = rows.children.length >= MAX_SOCIAL_LINKS;
}

export function renderLinks(links) {
    const anchors = [];
    rows.replaceChildren();
    for (const { network, url } of links) {
        addLinkRow(network, url);
        if (isValidLink(url)) {
            const anchor = createLink(network, url);
            anchor.target = "_blank";
            anchor.rel = "noopener noreferrer";
            anchors.push(anchor);
        }
    }
    add.disabled = links.length >= MAX_SOCIAL_LINKS;
    $("profile-links").replaceChildren(...anchors);
}

function readLinks() {
    return Array.from(rows.children, (row) => ({
        network: row.querySelector('[name="network"]').value.trim(),
        url: row.querySelector('[name="url"]').value.trim(),
    }));
}

function validate(links) {
    const networks = new Set();
    Array.from(rows.children).forEach((row, index) => {
        const { network, url } = links[index];
        const nameInput = row.querySelector('[name="network"]');
        const urlInput = row.querySelector('[name="url"]');
        const key = network.toLowerCase();
        nameInput.setCustomValidity(
            network && !networks.has(key) ? "" : errorMessages.invalidField,
        );
        urlInput.setCustomValidity(
            isValidLink(url) ? "" : errorMessages.invalidField,
        );
        networks.add(key);
    });
    return form.reportValidity();
}

export function initSocialLinks() {
    add.addEventListener("click", () => {
        if (rows.children.length < MAX_SOCIAL_LINKS) {
            addLinkRow();
            rows.lastElementChild.querySelector("input").focus();
        }
    });

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        clearError(error);
        status.textContent = "";
        const links = readLinks();
        if (!validate(links)) {
            return;
        }
        fields.disabled = true;
        try {
            await saveSocialLinks({ links });
            renderLinks(links);
            status.textContent = "Links saved.";
        } catch {
            showError(error, errorMessages.requestFailed);
        } finally {
            fields.disabled = false;
        }
    });

    form.addEventListener("input", (event) => {
        if (event.target instanceof HTMLInputElement) {
            event.target.setCustomValidity("");
        }
        clearNetworkErrors();
        status.textContent = "";
    });
}
