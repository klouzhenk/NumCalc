using NumCalc.Shared.EquationsSystems.Requests;
using NumCalc.UI.Shared.Models.EquationSystems;

namespace NumCalc.UI.Shared.Utils.Calculation;

public static class EquationSystemsUtils
{
    public static List<string> BuildEquationStrings(double[,] coefficients, double[] rhs, List<string> variables)
    {
        var size = variables.Count;
        var equations = new List<string>(size);

        for (var row = 0; row < size; row++)
        {
            var terms = Enumerable.Range(0, size)
                .Select(col => $"{coefficients[row, col]}*{variables[col]}");
            equations.Add($"{string.Join(" + ", terms)} = {rhs[row]}");
        }

        return equations;
    }

    public static NonLinearSystemRequest BuildNonLinearSystemRequest(NonLinearSystemFormData formData)
    {
        return new NonLinearSystemRequest
        {
            IterationFunctions = formData.IterationFunctions.ToList(),
            Variables = formData.Variables.ToList(),
            InitialGuess = formData.InitialGuess.ToList(),
            Tolerance = formData.Tolerance,
            MaxIterations = formData.MaxIterations
        };
    }
}
