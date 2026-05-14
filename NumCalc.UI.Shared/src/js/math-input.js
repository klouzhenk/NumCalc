export const MathHelper = {
    initMathField: (mathFieldElement, dotNetRef) => {
        if (!mathFieldElement) return;

        mathFieldElement.smartMode = true;
        mathFieldElement.mathVirtualKeyboardPolicy = "manual";

        mathFieldElement.addEventListener('input', (evt) => {
            dotNetRef.invokeMethodAsync('UpdateValue', mathFieldElement.value);
        });
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
