using Microsoft.AspNetCore.Components;
using NumCalc.UI.Shared.Enums.Optimization;
using NumCalc.UI.Shared.Models.Optimization;

namespace NumCalc.UI.Shared.Components.Optimization;

public partial class OptimizationInput : ComponentBase
{
    [Parameter] public OptimizationMethod Method { get; set; }

    private MathInput? _mathInput;

    // 2D methods fields
    private double _lowerBound;
    private double _upperBound = 1;
    private int _points = 100;
    private double _tolerance = 1e-6;

    // Gradient descent fields
    private readonly List<double> _initialPoint = [0.0];
    private double _learningRate = 0.01;
    private int _maxIterations = 200;

    private void AddVariable() => _initialPoint.Add(0.0);

    private void RemoveVariable()
    {
        if (_initialPoint.Count <= 1) return;
        _initialPoint.RemoveAt(_initialPoint.Count - 1);
    }

    public async Task<OptimizationFormData> GetFormData()
    {
        var expression = _mathInput is not null
            ? await _mathInput.GetAsciiValue()
            : string.Empty;

        return new OptimizationFormData
        {
            FunctionExpression = expression,
            LowerBound = _lowerBound,
            UpperBound = _upperBound,
            Points = _points,
            Tolerance = _tolerance,
            InitialPoint = [.. _initialPoint],
            LearningRate = _learningRate,
            MaxIterations = _maxIterations,
            IsGradientDescent = Method is OptimizationMethod.GradientDescent
        };
    }
}
