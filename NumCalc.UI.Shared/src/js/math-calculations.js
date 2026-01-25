export const ChartHelper = {
    plotPreview: (inputId) => {
        try {
            const mf = document.getElementById(inputId);
            if (!mf) return;
            const expressionText = mf.getValue("ascii-math");

            if (!expressionText) return;

            

            const chart = Highcharts.charts[0];
            if (chart) {
                chart.series[0].setData(data, true);
                chart.setTitle({ text: `Preview: ${expressionText}` });
            }

        } catch (err) {
            // ignore it
        }
    }    
};