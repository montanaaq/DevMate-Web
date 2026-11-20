const canvas = document.createElement("canvas");
const context = canvas.getContext("2d");

if (context) {
    canvas.className = "dither-background";
    canvas.setAttribute("aria-hidden", "true");
    document.body.prepend(canvas);

    const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
    const thresholds = [0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5];
    const pointer = {
        x: 0,
        y: 0,
        targetX: 0,
        targetY: 0,
        strength: 0,
        active: false,
    };
    let pixels;
    let cellSize;
    let frame = 0;
    let lastTime = 0;

    function draw() {
        for (let y = 0; y < canvas.height; y++) {
            const waveY = Math.sin(y * cellSize * 0.008);
            const dy = y * cellSize - pointer.y;

            for (let x = 0; x < canvas.width; x++) {
                const dx = x * cellSize - pointer.x;
                const influence =
                    pointer.strength > 0
                        ? Math.max(0, 1 - (dx * dx + dy * dy) / (300 * 300)) **
                              2 *
                          pointer.strength
                        : 0;
                const wave = Math.sin(
                    x * cellSize * 0.006 + waveY * 1.8 + influence * 4,
                );
                const brightness = 0.4 + wave * 0.3 + influence * 0.4;
                const threshold =
                    (thresholds[(y % 4) * 4 + (x % 4)] + 0.5) / 16;
                const offset = (y * canvas.width + x) * 4;
                pixels.data[offset] = 160;
                pixels.data[offset + 1] = 180;
                pixels.data[offset + 2] = 200;
                pixels.data[offset + 3] =
                    brightness > threshold ? 112 + influence * 48 : 0;
            }
        }

        context.putImageData(pixels, 0, 0);
    }

    function animate(timestamp) {
        frame = 0;
        if (timestamp - lastTime >= 1000 / 30) {
            lastTime = timestamp;

            const dx = pointer.targetX - pointer.x;
            const dy = pointer.targetY - pointer.y;
            const strength = pointer.active ? 1 : 0;
            const delta = strength - pointer.strength;
            const settled =
                Math.abs(dx) < 1 && Math.abs(dy) < 1 && Math.abs(delta) < 0.01;

            pointer.x = settled ? pointer.targetX : pointer.x + dx * 0.24;
            pointer.y = settled ? pointer.targetY : pointer.y + dy * 0.24;
            pointer.strength = settled
                ? strength
                : pointer.strength + delta * 0.24;
            draw();

            if (settled) {
                return;
            }
        }

        requestDraw();
    }

    function requestDraw() {
        if (!frame && !document.hidden && !reducedMotion.matches) {
            frame = requestAnimationFrame(animate);
        }
    }

    function releasePointer() {
        pointer.active = false;
        requestDraw();
    }

    function resetPointer() {
        cancelAnimationFrame(frame);
        frame = 0;
        pointer.active = false;
        pointer.strength = 0;
        draw();
    }

    function resize() {
        cellSize = Math.max(
            5,
            Math.sqrt((window.innerWidth * window.innerHeight) / 90000),
        );
        canvas.width = Math.ceil(window.innerWidth / cellSize);
        canvas.height = Math.ceil(window.innerHeight / cellSize);
        pixels = context.createImageData(canvas.width, canvas.height);
        draw();
    }

    window.addEventListener(
        "pointermove",
        (event) => {
            if (event.pointerType === "touch" || reducedMotion.matches) {
                return;
            }

            if (!pointer.active) {
                pointer.x = event.clientX;
                pointer.y = event.clientY;
            }

            pointer.targetX = event.clientX;
            pointer.targetY = event.clientY;
            pointer.active = true;
            requestDraw();
        },
        { passive: true },
    );
    document.documentElement.addEventListener("pointerleave", releasePointer);
    window.addEventListener("blur", releasePointer);
    window.addEventListener("resize", resize);
    document.addEventListener("visibilitychange", resetPointer);
    reducedMotion.addEventListener("change", resetPointer);

    resize();
}
