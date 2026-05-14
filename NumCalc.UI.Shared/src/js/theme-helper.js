export const ThemeHelper = {
    applyTheme: (mode) => {
        const theme = mode.toString().toLowerCase();
        document.documentElement.setAttribute('data-theme', theme);
    }
};