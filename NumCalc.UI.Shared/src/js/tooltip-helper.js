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
    },

    init: () => {
        document.addEventListener('mouseover', (e) => {
            if (!e.target.classList.contains('tooltip__icon')) return;
            const icon = e.target;
            const wrapper = icon.closest('.tooltip__wrapper');
            if (!wrapper) return;

            if (!wrapper._tooltipPopup) {
                const popup = wrapper.querySelector('.tooltip__popup');
                if (!popup) return;
                wrapper._tooltipPopup = popup;
                document.body.appendChild(popup);
            }

            const popup = wrapper._tooltipPopup;
            TooltipHelper._position(icon, popup);
            popup.style.display = 'block';
        });

        document.addEventListener('mouseout', (e) => {
            if (!e.target.classList.contains('tooltip__icon')) return;
            const icon = e.target;
            if (icon.contains(e.relatedTarget)) return;
            const popup = icon.closest('.tooltip__wrapper')?._tooltipPopup;
            if (popup) popup.style.display = 'none';
        });
    },

    _position: (icon, popup) => {
        const rect = icon.getBoundingClientRect();
        popup.style.top = `${rect.top - 120}px`;
        popup.style.left = `${Math.max(8, rect.right - 320)}px`;
        requestAnimationFrame(() => {
            const h = popup.offsetHeight || 80;
            const w = popup.offsetWidth || 320;
            const top = rect.top - h - 4;
            popup.style.top = top < 8 ? `${rect.bottom + 4}px` : `${top}px`;
            popup.style.left = `${Math.max(8, rect.right - w)}px`;
        });
    }
};
