import katex from "katex";
import renderMathInElement from 'katex/contrib/auto-render';


const KATEX_DELIMITERS = [
    { left: '$$', right: '$$', display: true },
    { left: '$',  right: '$',  display: false }
];

export const LatexHelper = {
    renderLatexInContainer(containerId) {
        const element = document.getElementById(containerId);
        if (!element) return;
        renderMathInElement(element, { delimiters: KATEX_DELIMITERS, throwOnError: false });
    },

    renderLatexById(elementId, latex) {
        const element = document.getElementById(elementId);
        if (!element) return;
        katex.render(latex, element, { throwOnError: false, displayMode: true });
    },

    renderStepFormulas(containerId) {
        const element = document.getElementById(containerId);
        if (!element) return;

        element.querySelectorAll('.solution-steps__item-formula[data-latex]').forEach(div => {
            const latex = div.dataset.latex;
            if (!latex) return;
            katex.render(latex, div, { throwOnError: false, displayMode: true });
        });

        element.querySelectorAll('.solution-steps__item-value[data-latex]').forEach(span => {
            const text = span.dataset.latex;
            if (!text) return;
            if (/\\[a-zA-Z]/.test(text)) {
                katex.render(text, span, { throwOnError: false, displayMode: false, strict: false });
            } else {
                span.textContent = text;
            }
        });
    },
};