export const TooltipHelper = {
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

function getPopupForIcon(icon) {
    const wrapper = icon.closest('.tooltip__wrapper');
    if (!wrapper) return null;

    if (wrapper._tooltipPopup) {
        return wrapper._tooltipPopup;
    }

    const popup = wrapper.querySelector('.tooltip__popup');
    if (!popup) return null;

    wrapper._tooltipPopup = popup;
    document.body.appendChild(popup); // escapes all stacking contexts
    return wrapper._tooltipPopup;
}

function positionPopup(icon, popup) {
    const rect = icon.getBoundingClientRect();

    requestAnimationFrame(() => {
        const height = popup.offsetHeight;
        const width = popup.offsetWidth;
        const gap = 4;

        const top  = rect.top - height - gap;
        const left = rect.right - width;

        popup.style.top  = top < 8  ? `${rect.bottom + gap}px` : `${top}px`;
        popup.style.left = left < 8 ? '8px'                    : `${left}px`;
    });
}
