import katex from 'katex';
import html2canvas from 'html2canvas';

export const PdfHelper = {
    getChartImage: async (containerId) => {
        const container = document.getElementById(containerId);
        if (!container) return null;
        try {
            const canvas = await html2canvas(container, { backgroundColor: '#ffffff' });
            return canvas.toDataURL('image/png');
        } catch {
            return null;
        }
    },

    renderLatexToPng: async (latexString) => {
        const div = document.createElement('div');
        div.style.cssText = 'position:absolute;left:-9999px;top:-9999px;background:#fff;padding:8px;font-size:13px';
        document.body.appendChild(div);
        try {
            katex.render(latexString, div, { throwOnError: false, displayMode: true });
            const canvas = await html2canvas(div, { backgroundColor: '#ffffff', scale: 1.5 });
            return canvas.toDataURL('image/png');
        } catch {
            return null;
        } finally {
            document.body.removeChild(div);
        }
    },

    downloadFile: (filename, contentType, base64) => {
        const link = document.createElement('a');
        link.href = `data:${contentType};base64,${base64}`;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
};
