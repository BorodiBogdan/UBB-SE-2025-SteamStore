// <copyright file="CreditCardProcessor.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class CreditCardProcessor
{
    private const int DelayForPayment = 200;
    private const int MinimumLenthCardNumber = 13;
    private const int MaximumLengthCardNumber = 19;
    private const string ValidExpirationDatePattern = @"^\d{2}/\d{2}$";
    private const char Separator = '/';
    private const string ReplaceCardNumberRegex = @"[^\d]";
    private const int FirstPartOfExpirationDateIndex = 0;
    private const int SecondPartOfExpirationDateIndex = 1;
    private const string ValidCVVPattern = @"^\d{3,4}$";
    private const int GetLastTwoDigits = 100;
    private const string ValidOwnerNamePattern = @"^[a-zA-Z\s]+$";

    public async Task<bool> ProcessPaymentAsync(string cardNumber, string expirationDate, string cvv, string ownerName)
    {
        if (this.IsValidCardNumber(cardNumber) &&
            this.IsValidExpirationDate(expirationDate) &&
            this.IsValidCvv(cvv) &&
            this.IsValidOwnerName(ownerName))
        {
            await Task.Delay(DelayForPayment);
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

        cardNumber = Regex.Replace(cardNumber, ReplaceCardNumberRegex, string.Empty);

        if (cardNumber.Length < MinimumLenthCardNumber || cardNumber.Length > MaximumLengthCardNumber)
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

        if (!Regex.IsMatch(expirationDate, ValidExpirationDatePattern))
        {
            return false;
        }

        string[] parts = expirationDate.Split(Separator);
        int month = int.Parse(parts[FirstPartOfExpirationDateIndex]);
        int year = int.Parse(parts[SecondPartOfExpirationDateIndex]);

        int currentYear = DateTime.Now.Year % GetLastTwoDigits;
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

        return Regex.IsMatch(cvv, ValidCVVPattern);
    }

    private bool IsValidOwnerName(string ownerName)
    {
        if (string.IsNullOrWhiteSpace(ownerName))
        {
            return false;
        }

        return Regex.IsMatch(ownerName, ValidOwnerNamePattern);
    }
}