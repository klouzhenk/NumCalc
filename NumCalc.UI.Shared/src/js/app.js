import 'virtual:svg-icons-register';
import 'katex/dist/katex.min.css';
import 'mathlive';
import { MathHelper } from './math-input.js';
import { ImageHelper } from './image-helper.js';
import { PdfHelper } from './pdf-helper.js';
import { TooltipHelper } from './tooltip-helper.js';
import { MathValidationHelper } from './math-helper.js';
import { ChartHelper } from './chart-helper.js';
import { LatexHelper } from './latex-helper.js';
import { ThemeHelper } from './theme-helper.js';

window.NumCalc = {
    ...MathHelper,
    ...MathValidationHelper,
    ...ChartHelper,
};

window.ImageHelper = {
    ...ImageHelper,
}

window.ThemeHelper = {
    ...ThemeHelper,
}

window.PdfHelper = {
    ...PdfHelper,
}

window.TooltipHelper = {
    ...TooltipHelper,
}

window.LatexHelper = {
    ...LatexHelper,
}

TooltipHelper.init();
