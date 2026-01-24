export const MathHelper = {

    initMathField: (element, dotNetRef) => {
        if (!element) return;

        element.smartMode = true;
        element.mathVirtualKeyboardPolicy = "manual";

        element.addEventListener('input', (evt) => {
            dotNetRef.invokeMethodAsync('UpdateValue', evt.target.value);
        });
    },

    setMathFieldValue: (element, value) => {
        if (element && element.value !== value) {
            element.value = value;
        }
    }
};