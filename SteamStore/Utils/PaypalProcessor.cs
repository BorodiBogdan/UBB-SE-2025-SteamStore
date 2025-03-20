using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class PaypalProcessor
{
    public async Task<bool> ProcessPaymentAsync(string email, string password, decimal amount)
    {
        if (IsValidEmail(email) && IsValidPassword(password))
        {
            // Simulate a successful payment
            await Task.Delay(2000);
            return true;
        }

        return false;
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Use a simple regex to validate the email format
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }
        catch (RegexMatchTimeoutException)
        {
            return false;
        }
    }

    private bool IsValidPassword(string password)
    {
        return !string.IsNullOrWhiteSpace(password) && password.Length > 8;
    }
}