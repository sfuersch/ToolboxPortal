using System.Globalization;
using System.Text;
using ToolboxPortal.Models;

namespace ToolboxPortal.Services;

public class ThgCsvImportService
{
    public async Task<List<ThgCustomer>> ParseAsync(
        Stream fileStream,
        string userId)
    {
        var customers = new List<ThgCustomer>();

        using var reader = new StreamReader(
            fileStream,
            Encoding.GetEncoding("Windows-1252"));

        var headerLine = await reader.ReadLineAsync();

        if (string.IsNullOrWhiteSpace(headerLine))
        {
            return customers;
        }

        var headers = SplitCsvLine(headerLine)
            .Select(x => x.Trim())
            .ToList();

        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var values = SplitCsvLine(line);

            string Get(params string[] columnNames)
            {
                foreach (var columnName in columnNames)
                {
                    var index = headers.FindIndex(x =>
                        string.Equals(
                            NormalizeHeader(x),
                            NormalizeHeader(columnName),
                            StringComparison.OrdinalIgnoreCase));

                    if (index >= 0 && index < values.Count)
                    {
                        return values[index].Trim();
                    }
                }

                return "";
            }

            var privateEmail = Get(
                "St.-EMail_1 (P)",
                "EMail_1 (P)",
                "E-Mail_1 (P)",
                "Email_1 (P)");

            var businessEmail = Get(
                "St.-EMail_2 (G)",
                "EMail_2 (G)",
                "E-Mail_2 (G)",
                "Email_2 (G)");

            var selectedEmail =
                !string.IsNullOrWhiteSpace(privateEmail)
                    ? privateEmail
                    : businessEmail;

            var emailSource =
                !string.IsNullOrWhiteSpace(privateEmail)
                    ? "Privat"
                    : !string.IsNullOrWhiteSpace(businessEmail)
                        ? "Geschäftlich"
                        : "";

            DateTime? firstRegistrationDate = null;

            var dateValue = Get(
                "Fa.-Datum_EZ",
                "Datum_EZ",
                "Datum EZ",
                "Erstzulassung");

            if (DateTime.TryParseExact(
                    dateValue,
                    "dd.MM.yyyy",
                    CultureInfo.GetCultureInfo("de-DE"),
                    DateTimeStyles.None,
                    out var parsedDate))
            {
                firstRegistrationDate = parsedDate;
            }
            else if (DateTime.TryParse(
                         dateValue,
                         CultureInfo.GetCultureInfo("de-DE"),
                         DateTimeStyles.None,
                         out parsedDate))
            {
                firstRegistrationDate = parsedDate;
            }

            var customer = new ThgCustomer
            {
                UserId = userId,

                Salutation = Get(
                    "St.-Anrede",
                    "Anrede"),

                FirstName = Get(
                    "St.-Vorname",
                    "Vorname"),

                LastName = Get(
                    "St.-Name",
                    "Name",
                    "Nachname"),

                Company = Get(
                    "St.-Firma",
                    "Firma"),

                Vin = Get(
                    "Fa.-Fahrgestellnummer",
                    "Fahrgestellnummer",
                    "VIN",
                    "FIN"),

                LicensePlate = Get(
                    "Fa.-Kennzeichen",
                    "Kennzeichen"),

                FirstRegistrationDate = firstRegistrationDate,

                Email = selectedEmail,
                EmailSource = emailSource,

                IsRegistered = false,
                FollowUpCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            var hasMinimumData =
                !string.IsNullOrWhiteSpace(customer.Vin)
                || !string.IsNullOrWhiteSpace(customer.LicensePlate)
                || !string.IsNullOrWhiteSpace(customer.Email)
                || !string.IsNullOrWhiteSpace(customer.LastName)
                || !string.IsNullOrWhiteSpace(customer.Company);

            if (hasMinimumData)
            {
                customers.Add(customer);
            }
        }

        return customers;
    }

    private static string NormalizeHeader(string value)
    {
        return value
            .Trim()
            .Replace("\uFEFF", "")
            .Replace("\"", "")
            .Replace(" ", "")
            .Replace("-", "")
            .Replace(".", "")
            .Replace("_", "")
            .ToLowerInvariant();
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