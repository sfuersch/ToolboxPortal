using System.Globalization;
using System.Text;
using ToolboxPortal.Models;

namespace ToolboxPortal.Services;

public class ThgCsvImportService
{
    public async Task<List<ThgCustomer>> ParseAsync(Stream fileStream, string userId)
    {
        var customers = new List<ThgCustomer>();

        using var reader = new StreamReader(fileStream, Encoding.GetEncoding("Windows-1252"));

        var headerLine = await reader.ReadLineAsync();

        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return customers;
        }

        var headers = SplitCsvLine(headerLine);

        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {

            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = SplitCsvLine(line);

            string Get(string columnName)
            {
                var index = headers.IndexOf(columnName);

                if (index < 0 || index >= values.Count)
                {
                    return "";
                }

                return values[index].Trim();
            }

            var privateEmail = Get("St.-EMail_1 (P)");
            var businessEmail = Get("St.-EMail_2 (G)");

            var selectedEmail = !string.IsNullOrWhiteSpace(privateEmail)
                ? privateEmail
                : businessEmail;

            var emailSource = !string.IsNullOrWhiteSpace(privateEmail)
                ? "Privat"
                : !string.IsNullOrWhiteSpace(businessEmail)
                    ? "Geschäftlich"
                    : "";

            DateTime? firstRegistrationDate = null;

            var dateValue = Get("Fa.-Datum_EZ");

            if (DateTime.TryParseExact(
                    dateValue,
                    "dd.MM.yyyy",
                    CultureInfo.GetCultureInfo("de-DE"),
                    DateTimeStyles.None,
                    out var parsedDate))
            {
                firstRegistrationDate = parsedDate;
            }

            var customer = new ThgCustomer
            {
                UserId = userId,
                FirstName = Get("St.-Vorname"),
                LastName = Get("St.-Name"),
                Company = Get("St.-Firma"),
                Vin = Get("Fa.-Fahrgestellnummer"),
                LicensePlate = Get("Fa.-Kennzeichen"),
                FirstRegistrationDate = firstRegistrationDate,
                Email = selectedEmail,
                EmailSource = emailSource,
                IsRegistered = false,
                FollowUpCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            if (!string.IsNullOrWhiteSpace(customer.Vin))
            {
                customers.Add(customer);
            }
        }

        return customers;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var current = new StringBuilder();
        var insideQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];

            if (c == '"')
            {
                insideQuotes = !insideQuotes;
                continue;
            }

            if (c == ';' && !insideQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
                continue;
            }

            current.Append(c);
        }

        result.Add(current.ToString());

        return result;
    }
}