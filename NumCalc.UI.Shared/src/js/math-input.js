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

    drawPlot: (config) => {
        const { containerId, title, xAxis, yAxis, series, showLegend, tooltipSuffix } = config;

        if (!document.getElementById(containerId)) return;

        const highchartsSeries = [];

        const xMin = xAxis.min ?? -10;
        const xMax = xAxis.max ?? 10;

        const pointsCount = 200;
        const step = (xMax - xMin) / pointsCount;
        
        console.log("pass consts");

        series.forEach(item => {
            if (!item.expression || !item.isVisible) return;

            console.log("pass serieeeeeeeeee");

            try {
                const expr = math.compile(item.expression);
                const data = [];

                for (let i = 0; i <= pointsCount; i++) {
                    const x = xMin + (i * step);
                    try {
                        let y = expr.evaluate({ x: x });
                        if (typeof y === 'number' && isFinite(y)) {
                            data.push([x, y]);
                        }
                    } catch (e) {}
                }

                highchartsSeries.push({
                    name: item.name || 'Series',
                    data: data,
                    color: item.color,
                    type: (item.type || 'line').toLowerCase(),
                    dashStyle: item.dashStyle || 'solid',
                    lineWidth: item.lineWidth,
                    marker: { enabled: false }
                });

            } catch (err) {
                console.warn(`Failed to plot series: ${item.name}`, err);
            }
        });

        console.log("pass series");

        const chartOptions = {
            chart: {
                backgroundColor: 'transparent',
                style: { fontFamily: 'IBM Plex Mono' }
            },
            title: { text: title || null },

            xAxis: {
                title: { text: xAxis.title || null },
                min: xAxis.min,
                max: xAxis.max,
                gridLineWidth: xAxis.showGrid ? 1 : 0,
                plotLines: xAxis.plotLines || []
            },

            yAxis: {
                title: { text: yAxis.title || null },
                gridLineWidth: yAxis.showGrid ? 1 : 0,
                plotLines: yAxis.plotLines || []
            },

            tooltip: {
                valueSuffix: tooltipSuffix || '',
                shared: true
            },

            legend: { enabled: showLegend },
            credits: { enabled: false },

            series: highchartsSeries
        };

        console.log("pass chart creation");

        const existingChart = Highcharts.charts.find(c => c && c.renderTo.id === containerId);

        console.log("chart creation", existingChart);
        
        if (existingChart) {
            while (existingChart.series.length > 0) {
                existingChart.series[0].remove(false);
            }
            highchartsSeries.forEach(s => existingChart.addSeries(s, false));
            existingChart.update(chartOptions);
        } else {
            Highcharts.chart(containerId, chartOptions);
        }
    },

    getAsciiFromMathField: (element) => {
        return element ? element.getValue("ascii-math") : "";
    }
};