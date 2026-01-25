import * as math from 'mathjs';
import Highcharts from 'highcharts';

export const MathHelper = {
    initMathField: (mathFieldElement, dotNetRef, chartContainerId, startElem, endElem) => {
        if (!mathFieldElement) return;

        mathFieldElement.smartMode = true;
        mathFieldElement.mathVirtualKeyboardPolicy = "manual";

        MathHelper.initChart(chartContainerId);

        const updateGraph = () => {
            const asciiMath = mathFieldElement.getValue("ascii-math");
            if (!asciiMath) return;
            
            let min = -10;
            let max = 10;

            if (startElem && startElem.value) min = parseFloat(startElem.value);
            if (endElem && endElem.value) max = parseFloat(endElem.value);

            if (min >= max) max = min + 1;

            window.NumCalc.drawPlot(chartContainerId, asciiMath, min, max);
        };

        mathFieldElement.addEventListener('input', (evt) => {
            dotNetRef.invokeMethodAsync('UpdateValue', mathFieldElement.value);
            updateGraph();
        });

        if (startElem) startElem.addEventListener('input', updateGraph);
        if (endElem) endElem.addEventListener('input', updateGraph);
    },

    initChart: (containerId) => {
        Highcharts.chart(containerId, {
            title: { text: null },
            chart: {
                style: { fontFamily: 'IBM Plex Mono' }
            },
            xAxis: {
                title: { text: 'x' },
                gridLineWidth: 1
            },
            yAxis: {
                title: { text: 'f(x)' }
            },
            series: [{
                name: 'Function',
                data: [],
                lineWidth: 2,
                marker: { enabled: false }
            }],
            credits: { enabled: false },
            legend: { enabled: false }
        });
    },

    drawPlot: (chartContainerId, expression, min, max) => {
        try {
            const step = (max - min) / 100;
            const expr = math.compile(expression);
            const xValues = math.range(min, max, step).toArray();
            
            const data = xValues.map(x => {
                try {
                    return [x, expr.evaluate({ x: x })];
                } catch (e) {
                    return null;
                }
            }).filter(p => p !== null);
            
            const chart = Highcharts.charts.find(c => c.renderTo.id === chartContainerId);
            if (chart) {
                chart.series[0].setData(data);
                chart.xAxis[0].setExtremes(min, max);
            }
        } catch (e) { console.log(e); }
    },

    setMathFieldValue: (element, value) => {
        if (element && element.value !== value) {
            element.value = value;
        }
    }
};