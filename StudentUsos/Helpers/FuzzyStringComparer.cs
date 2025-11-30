using System.Globalization;
using System.Text;

namespace StudentUsos.Helpers;

public static class FuzzyStringComparer
{
    public static int DamerauLevenshtein(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        var dp = new int[n + 1, m + 1];

        for (int i = 0; i <= n; i++) dp[i, 0] = i;
        for (int j = 0; j <= m; j++) dp[0, j] = j;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = s[i - 1] == t[j - 1] ? 0 : 1;

                dp[i, j] = Math.Min(
                    Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                    dp[i - 1, j - 1] + cost);

                if (i > 1 && j > 1 &&
                    s[i - 1] == t[j - 2] &&
                    s[i - 2] == t[j - 1])
                {
                    dp[i, j] = Math.Min(dp[i, j], dp[i - 2, j - 2] + cost);
                }
            }
        }

        return dp[n, m];
    }

    public static double SimilarityScore(string s, string t)
    {
        if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(t))
            return 0;

        int distance = DamerauLevenshtein(s, t);

        return 1.0 - (double)distance / Math.Max(s.Length, t.Length);
    }

    public static string Normalize(string text)
    {
        return new string(text
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray());
    }

    public static string GetAcronym(string name)
    {
        return string.Join("", name
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(w => char.ToUpper(w[0])));
    }

    public static string GetAcronymStartingWithUpperOnly(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "";

        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var acronymLetters = words
            .Where(w => char.IsUpper(w[0]))
            .Select(w => w[0]);

        return string.Concat(acronymLetters);
    }
}
