export const MathValidationHelper = {
    validateExpression: (expression) => {
        try {
            const normalized = expression.replace(/(\d)([a-zA-Z])/g, '$1*$2');
            const node = math.parse(normalized);
            const vars = new Set();
            node.traverse(n => {
                if (n.isSymbolNode && !math[n.name]) {
                    vars.add(n.name);
                }
            });
            return { valid: true, variables: Array.from(vars) };
        } catch {
            return { valid: false, variables: [] };
        }
    }
};
