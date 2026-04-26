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

    private void RemoveVariable(int index)
    {
        if (_initialPoint.Count <= 1) return;
        _initialPoint.RemoveAt(index);
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

    public async Task SetFormDataAsync(OptimizationFormData data)
    {
        _lowerBound = data.LowerBound;
        _upperBound = data.UpperBound;
        _points = data.Points;
        _tolerance = data.Tolerance;
        _initialPoint.Clear();
        _initialPoint.AddRange(data.InitialPoint);
        _learningRate = data.LearningRate;
        _maxIterations = data.MaxIterations;
        StateHasChanged();
        if (!string.IsNullOrEmpty(data.FunctionExpression))
            await (_mathInput?.SetLatexValue(data.FunctionExpression) ?? Task.CompletedTask);
    }
}
