import katex from 'katex';
import html2canvas from 'html2canvas';

export const PdfHelper = {
    getChartImage: (containerId) => {
        const chart = Highcharts.charts.find(c => c && c.renderTo.id === containerId);
        if (!chart) return null;

        const svg = chart.getSVG();
        const width = chart.chartWidth || 600;
        const height = chart.chartHeight || 400;

        const svgBlob = new Blob([svg], { type: 'image/svg+xml;charset=utf-8' });
        const url = URL.createObjectURL(svgBlob);

        return new Promise((resolve) => {
            const img = new Image();
            img.onload = () => {
                const canvas = document.createElement('canvas');
                canvas.width = width;
                canvas.height = height;
                const ctx = canvas.getContext('2d');
                ctx.fillStyle = '#ffffff';
                ctx.fillRect(0, 0, width, height);
                ctx.drawImage(img, 0, 0, width, height);
                URL.revokeObjectURL(url);
                resolve(canvas.toDataURL('image/png'));
            };
            img.onerror = () => {
                URL.revokeObjectURL(url);
                resolve(null);
            };
            img.src = url;
        });
    },

    renderLatexToPng: async (latexString) => {
        const div = document.createElement('div');
        div.style.cssText = 'position:absolute;left:-9999px;top:-9999px;background:#fff;padding:8px;font-size:18px';
        document.body.appendChild(div);

        katex.render(latexString, div, { throwOnError: false });

        const canvas = await html2canvas(div, { backgroundColor: '#ffffff' });
        document.body.removeChild(div);

        return canvas.toDataURL('image/png');
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
