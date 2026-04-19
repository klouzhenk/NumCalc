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
        const container = document.getElementById(containerId);
        if (!container) return;

        const generatedSeries = series
            .filter(s => s.isVisible)
            .map(s => processSeries(s, xAxis))
            .filter(s => s !== null);

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
                valueDecimals: 6,
                shared: true
            },

            legend: { enabled: showLegend },
            credits: { enabled: false },

            series: generatedSeries
        };

        console.log("pass chart creation");

        const existingChart = Highcharts.charts.find(c => c && c.renderTo.id === containerId);

        console.log("chart creation", existingChart);
        
        if (existingChart) {
            while (existingChart.series.length > 0) {
                existingChart.series[0].remove(false);
            }
            generatedSeries.forEach(s => existingChart.addSeries(s, false));
            existingChart.update(chartOptions);
        } else {
            Highcharts.chart(containerId, chartOptions);
        }
    },

    getAsciiFromMathField: (element) => {
        return element ? element.getValue("ascii-math") : "";
    },

    setLatexInMathField: (element, latex) => {
        if (element) {
            element.value = latex;
            element.dispatchEvent(new Event('input', { bubbles: true }));
        }
    }
};

function processSeries(seriesItem, xAxis){
    try {
        if (seriesItem.type === 'scatter' || (seriesItem.data && seriesItem.data.length > 0)) {
            const hasFill = seriesItem.fillLowerBound != null && seriesItem.fillUpperBound != null;
            return {
                name: seriesItem.name,
                data: seriesItem.data,
                color: seriesItem.color,
                type: hasFill ? 'area' : (seriesItem.type ? seriesItem.type.toLowerCase() : 'line'),
                lineWidth: seriesItem.lineWidth || 2,
                marker: { enabled: seriesItem.type === 'scatter', radius: 4 },
                ...(hasFill && {
                    fillOpacity: 0.25,
                    zoneAxis: 'x',
                    zones: [
                        { value: seriesItem.fillLowerBound, fillColor: 'transparent' },
                        { value: seriesItem.fillUpperBound },
                        { fillColor: 'transparent' }
                    ]
                })
            };
        }

        if (!seriesItem.expression) return null;

        const expr = math.compile(seriesItem.expression);
        const data = [];
        const xMin = xAxis.min ?? -10;
        const xMax = xAxis.max ?? 10;
        const pointsCount = 250;
        const step = (xMax - xMin) / pointsCount;

        for(let i = 0; i < pointsCount; i++) {
            const x = xMin + (i * step);
            try {
                const y = expr.evaluate({ x: x });
                if (typeof y === 'number' && isFinite(y)) {
                    data.push([x, y]);
                }
            }
            catch {
            }
        }

        return {
            name: seriesItem.name,
            data: data,
            color: seriesItem.color,
            type: seriesItem.type ? seriesItem.type.toLowerCase() : 'line',
            dashStyle: seriesItem.dashStyle || 'solid',
            lineWidth: seriesItem.lineWidth || 2,
            marker: { enabled: false }
        };
    }
    catch (err) {
        return null;
    }
}