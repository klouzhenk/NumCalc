namespace NumCalc.Calculation.Api.Controllers;


public static class CastExtensions
{
    public static T CastTo<T>(this object obj)
    {
        return (T)obj;
    }
}