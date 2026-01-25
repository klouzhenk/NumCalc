import * as math from 'mathjs';
import Highcharts from 'highcharts';

export const MathHelper = {
    initMathField: (mathFieldElement, dotNetRef) => {
        if (!mathFieldElement) return;

        mathFieldElement.smartMode = true;
        mathFieldElement.mathVirtualKeyboardPolicy = "manual";

        mathFieldElement.addEventListener('input', (evt) => {
            dotNetRef.invokeMethodAsync('UpdateValue', mathFieldElement.value);
        });
    },

    drawPlot: (containerId, expressionAscii, min, max) => {
        if (!expressionAscii) return;

        try {
            if (typeof min !== 'number' || isNaN(min)) min = -10;
            if (typeof max !== 'number' || isNaN(max)) max = 10;
            if (min >= max) {
                const center = min;
                min = center - 10;
                max = center + 10;
            }

            const expr = math.compile(expressionAscii);
            const data = [];

            const pointsCount = 200;
            const step = (max - min) / pointsCount;

            for (let i = 0; i <= pointsCount; i++) {
                const x = min + (i * step);

                try {
                    let y = expr.evaluate({ x: x });

                    if (typeof y === 'number' && isFinite(y)) {
                        data.push([x, y]);
                    }
                } catch (calcError) {
                    // skip the points where function is not defined
                }
            }

            const chartOptions = {
                title: { text: null },
                chart: {
                    backgroundColor: 'transparent',
                    style: { fontFamily: 'IBM Plex Mono' }
                },
                xAxis: {
                    min: min,
                    max: max,
                    gridLineWidth: 1
                },
                yAxis: { title: { text: null } },
                series: [{
                    name: 'f(x)',
                    data: data,
                    lineWidth: 2,
                    marker: { enabled: false }
                }],
                legend: { enabled: false },
                credits: { enabled: false }
            };

            const existingChart = Highcharts.charts.find(c => c && c.renderTo.id === containerId);

            if (existingChart) {
                existingChart.series[0].setData(data, true);
                existingChart.xAxis[0].setExtremes(min, max);
            } else {
                Highcharts.chart(containerId, chartOptions);
            }

        } catch (e) {
            // ignore
        }
    },

    getAsciiFromMathField: (element) => {
        return element ? element.getValue("ascii-math") : "";
    }
};