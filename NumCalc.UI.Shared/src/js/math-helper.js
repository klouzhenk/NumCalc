export const MathValidationHelper = {
    validateExpression: (expression) => {
        try {
            const normalizedExpression = expression.replace(/(\d)([a-zA-Z])/g, '$1*$2');
            const mathNode = math.parse(normalizedExpression);
            
            const variables = new Set();
            mathNode.traverse(n => {
                if (n.isSymbolNode && !math[n.name]) {
                    variables.add(n.name);
                }
            });
            
            return { valid: true, variables: Array.from(variables) };
        } catch {
            return { valid: false, variables: [] };
        }
    }
};
