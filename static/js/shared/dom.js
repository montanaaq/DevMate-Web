export const $ = (name, root = document) =>
    root.querySelector(`[data-${name}]`);

export function textElement(tag, text, className) {
    const element = document.createElement(tag);
    element.textContent = text;
    if (className) {
        element.className = className;
    }
    return element;
}

export const plural = (count, one, many) =>
    `${count} ${count === 1 ? one : many}`;

export function createLink(label, href) {
    const link = textElement("a", label);
    const icon = document.createElementNS("http://www.w3.org/2000/svg", "svg");
    icon.classList.add("link-icon");
    icon.setAttribute("viewBox", "0 0 24 24");
    icon.setAttribute("fill", "none");
    icon.setAttribute("stroke", "currentColor");
    icon.setAttribute("stroke-width", "2");
    icon.setAttribute("aria-hidden", "true");
    icon.setAttribute("focusable", "false");
    const path = document.createElementNS("http://www.w3.org/2000/svg", "path");
    path.setAttribute("d", "M5 12h14m-6-6 6 6-6 6");
    icon.append(path);
    link.append(" ", icon);
    link.href = href;
    return link;
}
