import * as math from 'mathjs';
import Highcharts from 'highcharts';

let _hc3dReady = false;

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

    drawPlot3d: async (config) => {
        if (!_hc3dReady) {
            window._Highcharts = Highcharts;
            await import('highcharts/highcharts-3d');
            _hc3dReady = true;
        }

        const { containerId, series, showLegend, xAxis, yAxis, zAxis } = config;
        const container = document.getElementById(containerId);
        if (!container) return;

        const generatedSeries = series
            .filter(s => s.isVisible)
            .map(s => ({
                name: s.name,
                type: 'scatter3d',
                data: s.data,
                color: s.color,
                marker: {
                    radius: s.type === 1 ? 5 : 2,
                    symbol: 'circle'
                },
                turboThreshold: 0
            }));

        const chartOptions = {
            chart: {
                type: 'scatter3d',
                backgroundColor: 'transparent',
                style: { fontFamily: 'IBM Plex Mono' },
                options3d: {
                    enabled: true,
                    alpha: 15,
                    beta: 30,
                    depth: 350,
                    viewDistance: 5,
                    fitToPlot: false,
                    frame: {
                        bottom: { size: 1, color: 'rgba(0,0,0,0.1)' },
                        back:   { size: 1, color: 'rgba(0,0,0,0.1)' },
                        side:   { size: 1, color: 'rgba(0,0,0,0.1)' }
                    }
                }
            },
            title: { text: null },
            xAxis: { title: { text: xAxis?.title || null } },
            yAxis: { title: { text: yAxis?.title || null } },
            zAxis: { title: { text: zAxis?.title || null } },
            tooltip: { valueDecimals: 4 },
            legend: { enabled: showLegend },
            credits: { enabled: false },
            series: generatedSeries
        };

        const existingChart = Highcharts.charts.find(c => c && c.renderTo.id === containerId);
        if (existingChart) {
            existingChart.destroy();
        }

        const chart = Highcharts.chart(containerId, chartOptions);

        // Mouse drag to rotate
        let startX, startY, startAlpha, startBeta;
        Highcharts.addEvent(chart.container, 'mousedown', (e) => {
            e = chart.pointer.normalize(e);
            startX = e.chartX;
            startY = e.chartY;
            startAlpha = chart.options.chart.options3d.alpha;
            startBeta  = chart.options.chart.options3d.beta;

            const onMove = (e) => {
                e = chart.pointer.normalize(e);
                chart.update({
                    chart: {
                        options3d: {
                            alpha: startAlpha + (e.chartY - startY) / 5,
                            beta:  startBeta  - (e.chartX - startX) / 5
                        }
                    }
                }, undefined, undefined, false);
            };

            const onUp = () => {
                Highcharts.removeEvent(document, 'mousemove', onMove);
                Highcharts.removeEvent(document, 'mouseup', onUp);
            };

            Highcharts.addEvent(document, 'mousemove', onMove);
            Highcharts.addEvent(document, 'mouseup', onUp);
        });
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
                marker: { enabled: seriesItem.type?.toLowerCase() === 'scatter', radius: seriesItem.marker?.radius ?? 8 },
                ...(hasFill && {
                    fillOpacity: 0.25,
                    zoneAxis: 'x',
                    zones: [
                        { value: seriesItem.fillLowerBound, fillColor: 'transparent' },
                        { value: seriesItem.fillUpperBound },
                        { fillColor: 'transparent' }
                    ]
                }),
                ...(seriesItem.step && { step: seriesItem.step })
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