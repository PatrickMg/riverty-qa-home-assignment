using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using CardValidation.Core.Enums;
using CardValidation.Web;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace CardValidation.Web.IntegrationTests;

[TestFixture]
    public class CreditCardApiTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task ValidateCreditCard_WithValidPayload_ReturnsPaymentSystemType()
    {
        var payload = new
        {
            Owner = "Jane Doe",
            Number = "4111111111111111",
            Date = DateTime.UtcNow.AddYears(1).ToString("MM/yy", CultureInfo.InvariantCulture),
            Cvv = "123"
        };

        var response = await _client.PostAsJsonAsync("/CardValidation/card/credit/validate", payload);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var paymentSystemType = await response.Content.ReadFromJsonAsync<PaymentSystemType>();
        Assert.That(paymentSystemType, Is.EqualTo(PaymentSystemType.Visa));
    }

    [Test]
    public async Task ValidateCreditCard_WhenFieldsMissing_ReturnsValidationErrors()
    {
        var payload = new { Owner = string.Empty, Number = string.Empty, Date = string.Empty, Cvv = string.Empty };

        var response = await _client.PostAsJsonAsync("/CardValidation/card/credit/validate", payload);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var errors = await ReadValidationErrorsAsync(response);
        Assert.Multiple(() =>
        {
            Assert.That(errors.Keys, Does.Contain("Owner"));
            Assert.That(errors.Keys, Does.Contain("Date"));
            Assert.That(errors.Keys, Does.Contain("Cvv"));
            Assert.That(errors.Keys, Does.Contain("Number"));
        });
    }

    [Test]
    public async Task ValidateCreditCard_WithInvalidCardNumber_ReturnsWrongNumberMessage()
    {
        var payload = new
        {
            Owner = "Jane Doe",
            Number = "1234567890123456",
            Date = DateTime.UtcNow.AddYears(1).ToString("MM/yy", CultureInfo.InvariantCulture),
            Cvv = "123"
        };

        var response = await _client.PostAsJsonAsync("/CardValidation/card/credit/validate", payload);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        var errors = await ReadValidationErrorsAsync(response);
        Assert.That(errors, Does.ContainKey("Number"));
        Assert.That(errors["Number"].First(), Is.EqualTo("Wrong number"));
    }

    private static async Task<IDictionary<string, string[]>> ReadValidationErrorsAsync(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };

        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        using var document = JsonDocument.Parse(json, options);
        var root = document.RootElement;

        if (root.TryGetProperty("errors", out var errorsProperty) && errorsProperty.ValueKind == JsonValueKind.Object)
        {
            foreach (var error in errorsProperty.EnumerateObject())
            {
                errors[error.Name] = error.Value.EnumerateArray().Select(v => v.GetString() ?? string.Empty).ToArray();
            }

            return errors;
        }

        if (root.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in root.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    errors[property.Name] = property.Value.EnumerateArray().Select(v => v.GetString() ?? string.Empty).ToArray();
                }
            }
        }

        return errors;
    }
}
