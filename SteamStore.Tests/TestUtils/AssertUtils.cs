using System.Reflection;
using Xunit.Sdk;

namespace SteamStore.Tests.TestUtils;

public static class AssertUtils
{
    public static void AssertAllPropertiesEqual<T>(T expected, T actual)
    {
        if (expected == null || actual == null)
            throw new ArgumentException("Objects cannot be null");

        var type = typeof(T);
        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            var expectedValue = property.GetValue(expected);
            var actualValue = property.GetValue(actual);
            try
            {
                Assert.Equal(expectedValue, actualValue);
            }
            catch (EqualException)
            {
                throw EqualException.ForMismatchedValues(expectedValue, actualValue, property.ToString());
            }

        }
    }
}