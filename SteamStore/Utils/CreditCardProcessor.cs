// <copyright file="CreditCardProcessor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class CreditCardProcessor
{
    private const int DELAYFORPAYMENT = 200;
    private const int MINIMUMLENGTHCARDNUMBER = 13;
    private const int MAXIMUMLENGTHCARDNUMBER = 19;

    public async Task<bool> ProcessPaymentAsync(string cardNumber, string expirationDate, string cvv, string ownerName)
    {
        if (this.IsValidCardNumber(cardNumber) &&
            this.IsValidExpirationDate(expirationDate) &&
            this.IsValidCvv(cvv) &&
            this.IsValidOwnerName(ownerName))
        {
            await Task.Delay(DELAYFORPAYMENT);
            return true;
        }

        return false;
    }

    private bool IsValidCardNumber(string cardNumber)
    {
        if (string.IsNullOrWhiteSpace(cardNumber))
        {
            return false;
        }

        cardNumber = Regex.Replace(cardNumber, @"[^\d]", string.Empty);

        if (cardNumber.Length < MINIMUMLENGTHCARDNUMBER || cardNumber.Length > MAXIMUMLENGTHCARDNUMBER)
        {
            return false;
        }

        return true;
    }

    private bool IsValidExpirationDate(string expirationDate)
    {
        if (string.IsNullOrWhiteSpace(expirationDate))
        {
            return false;
        }

        if (!Regex.IsMatch(expirationDate, @"^\d{2}/\d{2}$"))
        {
            return false;
        }

        string[] parts = expirationDate.Split('/');
        int month = int.Parse(parts[0]);
        int year = int.Parse(parts[1]);

        int currentYear = DateTime.Now.Year % 100;
        int currentMonth = DateTime.Now.Month;

        if (year < currentYear || (year == currentYear && month < currentMonth))
        {
            return false;
        }

        return true;
    }

    private bool IsValidCvv(string cvv)
    {
        if (string.IsNullOrWhiteSpace(cvv))
        {
            return false;
        }

        return Regex.IsMatch(cvv, @"^\d{3,4}$");
    }

    private bool IsValidOwnerName(string ownerName)
    {
        if (string.IsNullOrWhiteSpace(ownerName))
        {
            return false;
        }

        return Regex.IsMatch(ownerName, @"^[a-zA-Z\s]+$");
    }
}