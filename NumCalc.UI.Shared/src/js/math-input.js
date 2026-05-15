let outsideClickListenerAttached = false;

function attachOutsideClickListener() {
    if (outsideClickListenerAttached) return;
    outsideClickListenerAttached = true;

    document.addEventListener('pointerdown', (evt) => {
        const keyboard = window.mathVirtualKeyboard;
        if (!keyboard?.visible) return;

        const path = evt.composedPath();
        const insideField = path.some(el => el.tagName === 'MATH-FIELD');
        const insideKeyboard = path.some(el => el.classList?.contains('ML__keyboard'));

        if (!insideField && !insideKeyboard) {
            keyboard.hide();
        }
    });
}

export const MathHelper = {
    initMathField: (mathFieldElement, dotNetRef) => {
        if (!mathFieldElement) return;

        mathFieldElement.smartMode = true;
        mathFieldElement.mathVirtualKeyboardPolicy = "manual";

        mathFieldElement.addEventListener('input', (evt) => {
            dotNetRef.invokeMethodAsync('UpdateValue', mathFieldElement.value);
        });

        attachOutsideClickListener();
    },

    getAsciiFromMathField: (element) => {
        return element ? element.getValue("ascii-math") : "";
    },

    setLatexInMathField: (element, latex) => {
        if (!element) return;
        
        element.value = latex;
        element.dispatchEvent(new Event('input', { bubbles: true }));
    }
};
