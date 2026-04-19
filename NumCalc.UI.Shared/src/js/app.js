import 'virtual:svg-icons-register';
import 'katex/dist/katex.min.css';
import 'mathlive';
import { MathHelper } from './math-input.js';
import { ImageHelper } from './image-helper.js';
import { PdfHelper } from './pdf-helper.js';
import { TooltipHelper } from './tooltip-helper.js';
import { MathValidationHelper } from './math-helper.js';

window.NumCalc = {
    ...MathHelper,
    ...MathValidationHelper,
};

window.ImageHelper = {
    ...ImageHelper,
}

window.PdfHelper = {
    ...PdfHelper,
}

window.TooltipHelper = {
    ...TooltipHelper,
}

TooltipHelper.init();
