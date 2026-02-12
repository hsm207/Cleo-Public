using System;
using System.Text.RegularExpressions;
using System.Linq;

public class Program
{
    public static void Main()
    {
        var regex = new Regex(@"^\+\+\+\s+b/(.*)$", RegexOptions.Multiline);
        var input = "+++ /dev/null";
        var match = regex.Match(input);
        Console.WriteLine($"Match success: {match.Success}");
        if (match.Success) Console.WriteLine($"Group 1: '{match.Groups[1].Value}'");
    }
}
