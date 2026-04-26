using Microsoft.AspNetCore.Components;

namespace NumCalc.UI.Shared.Components;

public partial class NodeTable : ComponentBase
{
    [Parameter] public bool ShowY { get; set; }
    [Parameter] public int InitialNodeCount { get; set; } = 4;

    private int _nodeCount;
    private List<double> _xNodes = [];
    private List<double> _yValues = [];

    protected override void OnInitialized()
    {
        _nodeCount = InitialNodeCount;
        _xNodes = new List<double>(Enumerable.Repeat(0.0, _nodeCount));
        _yValues = new List<double>(Enumerable.Repeat(0.0, _nodeCount));
    }

    private void AddNode()
    {
        _nodeCount++;
        _xNodes.Add(0.0);
        _yValues.Add(0.0);
    }

    private void RemoveNode(int index)
    {
        if (_nodeCount <= 2) return;
        _nodeCount--;
        _xNodes.RemoveAt(index);
        _yValues.RemoveAt(index);
    }

    public List<double> GetXNodes() => [.. _xNodes];
    public List<double> GetYValues() => [.. _yValues];

    public void SetValues(List<double> xNodes, List<double>? yValues = null)
    {
        _nodeCount = xNodes.Count;
        _xNodes = [.. xNodes];
        _yValues = yValues is not null ? [.. yValues] : new List<double>(Enumerable.Repeat(0.0, _nodeCount));
        StateHasChanged();
    }
}
