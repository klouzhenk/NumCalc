import Highcharts from 'highcharts/esm/highcharts';
import 'highcharts/esm/highcharts-3d';
import * as math from "mathjs";

const CURVE_RESOLUTION = 250;

const OPTIONS_3D_DEFAULTS = {
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
};

export const ChartHelper = {
    drawPlot: (config) => {
        const container = document.getElementById(config.containerId);
        if (!container) return;

        const generatedSeries = config.series
            .map(s => processSeries(s, config.xAxis))
            .filter(s => s !== null);

        const chartOptions = build2dChartOptions(config, generatedSeries);
        upsertChart(config.containerId, chartOptions, generatedSeries);
    },

    drawPlot3d: (config) => {
        const container = document.getElementById(config.containerId);
        if (!container) return;

        const series = config.series.map(build3dSeries);
        const chartOptions = build3dChartOptions(config, series);

        findChart(config.containerId)?.destroy();
        const chart = Highcharts.chart(config.containerId, chartOptions);
        enableMouseRotation(chart);
    },
}

function build3dSeries(seriesItem) {
    return {
        name: seriesItem.name,
        type: 'scatter3d',
        data: seriesItem.data,
        color: seriesItem.color,
        marker: {
            radius: seriesItem.type === 1 ? 5 : 2,
            symbol: 'circle'
        },
        turboThreshold: 0
    };
}

function build2dChartOptions(config, series) {
    const { title, xAxis, yAxis, showLegend, tooltipSuffix, decimals } = config;
    const tooltipDecimals = (decimals != null && decimals >= 0) ? decimals : 5;

    return {
        chart: {
            backgroundColor: 'transparent',
            style: { fontFamily: 'IBM Plex Mono' }
        },
        title: { text: title || null },
        xAxis: {
            title: { 
                text: xAxis.title || null,
                style: {
                    color: 'rgb(var(--black-rgb))'
                }
            },
            min: xAxis.min,
            max: xAxis.max,
            gridLineWidth: xAxis.showGrid ? 1 : 0,
            plotLines: xAxis.plotLines || [],
            labels: {
                style: {
                    color: 'rgb(var(--black-rgb))'
                }
            },
        },
        yAxis: {
            title: {
                text: yAxis.title || null,
                style: {
                    color: 'rgb(var(--black-rgb))'
                }
            },
            gridLineWidth: yAxis.showGrid ? 1 : 0,
            plotLines: yAxis.plotLines || [],
            labels: {
                style: {
                    color: 'rgb(var(--black-rgb))'
                }
            },
        },
        tooltip: {
            shared: true,
            useHTML: true,
            formatter: buildTooltipFormatter(tooltipSuffix, tooltipDecimals)
        },
        legend: {
            enabled: showLegend,
            itemStyle: {
                color: 'rgb(var(--black-rgb))'
            }
        },
        credits: { enabled: false },
        series: series
    };
}

function buildTooltipFormatter(tooltipSuffix, tooltipDecimals) {
    const suffix = tooltipSuffix || '';
    return function () {
        const x = formatNumber(this.x, tooltipDecimals);
        const rows = this.points.map(p =>
            `<span style="color:${p.color}">●</span> ${p.series.name}: <b>${formatNumber(p.y, tooltipDecimals)}${suffix}</b>`
        ).join('<br/>');
        return `<small>x = ${x}</small><br/>${rows}`;
    };
}

function build3dChartOptions(config, series) {
    const { xAxis, yAxis, zAxis, showLegend, decimals } = config;
    const tooltipDecimals = (decimals != null && decimals >= 0) ? decimals : 4;

    return {
        chart: {
            type: 'scatter3d',
            backgroundColor: 'transparent',
            style: { fontFamily: 'IBM Plex Mono' },
            options3d: OPTIONS_3D_DEFAULTS
        },
        title: { text: null },
        xAxis: { title: { text: xAxis?.title || null } },
        yAxis: { title: { text: yAxis?.title || null } },
        zAxis: { title: { text: zAxis?.title || null } },
        tooltip: { valueDecimals: tooltipDecimals },
        legend: { enabled: showLegend },
        credits: { enabled: false },
        series: series
    };
}

function enableMouseRotation(chart) {
    Highcharts.addEvent(chart.container, 'mousedown', (e) => {
        e = chart.pointer.normalize(e);
        const startX = e.chartX;
        const startY = e.chartY;
        const startAlpha = chart.options.chart.options3d.alpha;
        const startBeta  = chart.options.chart.options3d.beta;

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
}

function formatNumber(value, tooltipDecimals){
    return Number(value).toFixed(tooltipDecimals).replace(/\.?0+$/, '');   
}

function findChart(containerId) {
    const container = document.getElementById(containerId);
    if (!container) return undefined;

    // SPA navigation leaves charts bound to detached elements that still carry
    // the same id — destroy those so they aren't updated instead of the live one.
    Highcharts.charts
        .filter(c => c && c.renderTo.id === containerId && c.renderTo !== container)
        .forEach(c => c.destroy());

    return Highcharts.charts.find(c => c && c.renderTo === container);
}

function upsertChart(containerId, chartOptions, generatedSeries){
    const existingChart = findChart(containerId);

    if (!existingChart) {
        Highcharts.chart(containerId, chartOptions);
        return;        
    }
    
    if (existingChart.options?.chart?.options3d?.enabled) {
        existingChart.destroy();
        Highcharts.chart(containerId, chartOptions);
        return;
    } 
    
    updateChart(existingChart, chartOptions, generatedSeries);
}

function updateChart(existingChart, chartOptions, generatedSeries) {
    while (existingChart.series.length > 0) {
        existingChart.series[0].remove(false);
    }
    generatedSeries.forEach(s => existingChart.addSeries(s, false));
    existingChart.update(chartOptions);    
}

function processSeries(seriesItem, xAxis) {
    try {
        if (seriesItem.type === 'scatter' || (seriesItem.data && seriesItem.data.length > 0)) {
            return processScatterItem(seriesItem);
        }

        if (!seriesItem.expression) return null;

        const data = sampleCurve(seriesItem.expression, xAxis.min, xAxis.max);
        return buildCurveSeries(seriesItem, data);
    }
    catch {
        return null;
    }
}

function sampleCurve(expression, xMin, xMax) {
    const expr = math.compile(expression);
    const step = (xMax - xMin) / CURVE_RESOLUTION;
    const data = [];

    for (let i = 0; i < CURVE_RESOLUTION; i++) {
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

    return data;
}

function buildCurveSeries(seriesItem, data) {
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

function processScatterItem(seriesItem) {
    const hasFill = seriesItem.fillLowerBound != null && seriesItem.fillUpperBound != null;

    return {
        name: seriesItem.name,
        data: seriesItem.data,
        color: seriesItem.color,
        type: hasFill ? 'area' : (seriesItem.type ? seriesItem.type.toLowerCase() : 'line'),
        lineWidth: seriesItem.lineWidth || 2,
        marker: {
            symbol: seriesItem.marker?.symbol ?? 'circle',
            enabled: seriesItem.type?.toLowerCase() === 'scatter',
            radius: seriesItem.marker?.radius ?? 8
        },
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