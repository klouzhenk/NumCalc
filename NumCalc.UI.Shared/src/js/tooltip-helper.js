import katex from 'katex';
import renderMathInElement from 'katex/contrib/auto-render';

const KATEX_DELIMITERS = [
    { left: '$$', right: '$$', display: true },
    { left: '$',  right: '$',  display: false }
];

function getPopupForIcon(icon) {
    const wrapper = icon.closest('.tooltip__wrapper');
    if (!wrapper) return null;

    if (!wrapper._tooltipPopup) {
        const popup = wrapper.querySelector('.tooltip__popup');
        if (!popup) return null;
        wrapper._tooltipPopup = popup;
        document.body.appendChild(popup); // portal: escapes all stacking contexts
    }

    return wrapper._tooltipPopup;
}

function positionPopup(icon, popup) {
    const rect = icon.getBoundingClientRect();

    requestAnimationFrame(() => {
        const h = popup.offsetHeight;
        const w = popup.offsetWidth;
        const gap = 4;

        const top  = rect.top - h - gap;
        const left = rect.right - w;

        popup.style.top  = top < 8  ? `${rect.bottom + gap}px` : `${top}px`;
        popup.style.left = left < 8 ? '8px'                    : `${left}px`;
    });
}

export const TooltipHelper = {
    renderLatexInContainer(containerId) {
        const el = document.getElementById(containerId);
        if (!el) return;
        renderMathInElement(el, { delimiters: KATEX_DELIMITERS, throwOnError: false });
    },

    renderLatexById(elementId, latex) {
        const el = document.getElementById(elementId);
        if (!el) return;
        katex.render(latex, el, { throwOnError: false, displayMode: true });
    },

    renderStepFormulas(containerId) {
        const el = document.getElementById(containerId);
        if (!el) return;
        el.querySelectorAll('.solution-steps__item-formula[data-latex]').forEach(div => {
            const latex = div.dataset.latex;
            if (!latex) return;
            katex.render(latex, div, { throwOnError: false, displayMode: true });
        });
        el.querySelectorAll('.solution-steps__item-value[data-latex]').forEach(span => {
            const text = span.dataset.latex;
            if (!text) return;
            if (/\\[a-zA-Z]/.test(text)) {
                katex.render(text, span, { throwOnError: false, displayMode: false, strict: false });
            } else {
                span.textContent = text;
            }
        });
    },

    init() {
        document.addEventListener('mouseover', (e) => {
            if (!e.target.classList.contains('tooltip__icon')) return;

            const icon  = e.target;
            const popup = getPopupForIcon(icon);
            if (!popup) return;

            popup.style.display = 'block';
            positionPopup(icon, popup);
        });

        document.addEventListener('mouseout', (e) => {
            if (!e.target.classList.contains('tooltip__icon')) return;
            if (e.target.contains(e.relatedTarget)) return;

            const popup = e.target.closest('.tooltip__wrapper')?._tooltipPopup;
            if (popup) popup.style.display = 'none';
        });
    }
};
