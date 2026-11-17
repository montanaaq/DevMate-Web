import { ACCENT_PRESETS, ACCENT_STORAGE_KEY } from "./constants/accent.js";

export function applyAccent(value = "blue") {
    const preset = ACCENT_PRESETS.find(({ name }) => name === value);
    const isCustom = /^#[\da-f]{6}$/i.test(value);
    const color = preset?.color ?? (isCustom ? value.toLowerCase() : ACCENT_PRESETS[0].color);
    const root = document.documentElement;
    root.dataset.accent = preset?.name ?? (isCustom ? "custom" : "blue");
    root.dataset.accentColor = color;
    root.style.setProperty("--dm-accent", color);
}

try {
    applyAccent(localStorage.getItem(ACCENT_STORAGE_KEY));
} catch {
    applyAccent();
    console.error("Could not load the accent color.");
}
