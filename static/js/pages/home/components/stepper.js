export function createStepper(counts, onChange) {
    let currentStep = 1;

    function set(value) {
        const nextStep =
            value === "first" ? 1 : value === "last" ? counts : value;
        if (!Number.isInteger(nextStep) || nextStep < 1 || nextStep > counts) {
            return;
        }
        currentStep = nextStep;
        onChange(currentStep);
    }

    return {
        counts,
        get currentStep() {
            return currentStep;
        },
        get isFirst() {
            return currentStep === 1;
        },
        get isLast() {
            return currentStep === counts;
        },
        next: () => set(currentStep + 1),
        back: () => set(currentStep - 1),
        reset: () => set(1),
        set,
    };
}
