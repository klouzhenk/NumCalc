namespace NumCalc.Calculation.Api.Entities;

public class PythonResponse<T>
{
    public T? Success { get; set; }
    public PythonFailureData? Failure { get; set; }
}