import 'virtual:svg-icons-register';
import 'mathlive';
import { MathHelper } from './math-input.js';
import { ImageHelper } from './image-helper.js';
import { PdfHelper } from './pdf-helper.js';
import { TooltipHelper } from './tooltip-helper.js';

window.NumCalc = {
    ...MathHelper,
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
