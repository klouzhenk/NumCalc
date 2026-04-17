import renderMathInElement from 'katex/contrib/auto-render';

export const TooltipHelper = {
    renderLatexInContainer: (containerId) => {
        const el = document.getElementById(containerId);
        if (!el) return;
        renderMathInElement(el, {
            delimiters: [
                { left: '$$', right: '$$', display: true },
                { left: '$', right: '$', display: false }
            ],
            throwOnError: false
        });
    }
};
