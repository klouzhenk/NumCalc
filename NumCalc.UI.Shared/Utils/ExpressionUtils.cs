using System.Text.RegularExpressions;

namespace NumCalc.UI.Shared.Utils;

public static class ExpressionUtils
{
    // TODO : this method is hot-fix for parsing latex to string that will be understood by js built function
    // search for another more reliable fix
    public static string NormalizeForChart(this string latex)
    {
        if (string.IsNullOrWhiteSpace(latex)) return "0";

        var expression = latex;

        expression = expression.Replace(@"\left", "").Replace(@"\right", "");

        expression = Regex.Replace(expression, @"\\frac\{(.+?)\}\{(.+?)\}", "($1)/($2)");
        
        expression = expression.Replace(@"\cdot", "*");
        expression = expression.Replace(@"\times", "*");

        expression = Regex.Replace(expression, @"\\sqrt\[(.+?)\]\{(.+?)\}", "nthRoot($2, $1)");
        expression = expression.Replace(@"\sqrt", "sqrt");

        expression = expression.Replace(@"\sin", "sin");
        expression = expression.Replace(@"\cos", "cos");
        expression = expression.Replace(@"\tan", "tan");
        expression = expression.Replace(@"\ctg", "cot");
        expression = expression.Replace(@"\ln", "log");
        expression = expression.Replace(@"\log", "log10");
        expression = expression.Replace(@"\pi", "pi");
        expression = expression.Replace(@"\e", "e");

        expression = Regex.Replace(expression, @"\|(.+?)\|", "abs($1)");

        expression = Regex.Replace(expression, @"\^\{(.+?)\}", "^($1)");

        expression = expression.Replace("{", "(").Replace("}", ")");

        return expression;
    }
}