using System.Text;

namespace SteamStore.Tests.TestUtils;

public static class CommonTestUtils
{
    private static readonly Random Random = new();

    private static string RandomString(int i, string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")
    {
        var result = new StringBuilder(i);

        for (var j = 0; j < i; j++)
        {
            result.Append(chars[Random.Next(chars.Length)]);
        }

        return result.ToString();
    }

    public static string RandomPath()
    {
        return "/" + RandomString(100, "/123456789abcdefghijklmnopqrstuvwxyz");
    }

    public static string RandomName(int count)
    {
        return RandomString(count);
    }

    public static decimal RandomNumber(int min, int max, int decimals)
    {
        var value = Random.NextDouble() * (max - min) + min;
        var rounded = Convert.ToDecimal(Math.Round(value, decimals));
        return rounded;
    }

    public static T RandomElement<T>(T[] array)
    {
        var index = Random.Next(array.Length);
        return array[index];
    }

    
}