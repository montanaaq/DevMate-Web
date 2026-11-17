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
    let frame;
    let lastTime = 0;
    let time = 0;

    function draw() {
        pointer.x += (pointer.targetX - pointer.x) * 0.12;
        pointer.y += (pointer.targetY - pointer.y) * 0.12;
        pointer.strength +=
            ((pointer.active ? 1 : 0) - pointer.strength) * 0.12;

        for (let y = 0; y < canvas.height; y++) {
            const waveY = Math.sin(y * cellSize * 0.008 - time * 0.4);

            for (let x = 0; x < canvas.width; x++) {
                const dx = x * cellSize - pointer.x;
                const dy = y * cellSize - pointer.y;
                const influence =
                    pointer.strength > 0.001
                        ? Math.max(0, 1 - Math.hypot(dx, dy) / 240) *
                          pointer.strength
                        : 0;
                const wave = Math.sin(
                    x * cellSize * 0.006 +
                        waveY * 1.8 +
                        time * 0.3 +
                        influence * 3,
                );
                const brightness = 0.45 + wave * 0.25 + influence * 0.25;
                const threshold =
                    (thresholds[(y % 4) * 4 + (x % 4)] + 0.5) / 16;
                const offset = (y * canvas.width + x) * 4;
                pixels.data[offset] = 160;
                pixels.data[offset + 1] = 180;
                pixels.data[offset + 2] = 200;
                pixels.data[offset + 3] = brightness > threshold ? 255 : 0;
            }
        }

        context.putImageData(pixels, 0, 0);
    }

    function animate(timestamp) {
        if (timestamp - lastTime >= 1000 / 30) {
            time += Math.min((timestamp - lastTime) / 1000, 0.1);
            lastTime = timestamp;
            draw();
        }

        frame = requestAnimationFrame(animate);
    }

    function updateAnimation() {
        cancelAnimationFrame(frame);
        if (!document.hidden && !reducedMotion.matches) {
            lastTime = performance.now();
            frame = requestAnimationFrame(animate);
        }
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
        },
        { passive: true },
    );
    document.documentElement.addEventListener("pointerleave", () => {
        pointer.active = false;
    });
    window.addEventListener("blur", () => {
        pointer.active = false;
    });
    window.addEventListener("resize", resize);
    document.addEventListener("visibilitychange", updateAnimation);
    reducedMotion.addEventListener("change", () => {
        pointer.active = false;
        pointer.strength = 0;
        draw();
        updateAnimation();
    });

    resize();
    updateAnimation();
}
