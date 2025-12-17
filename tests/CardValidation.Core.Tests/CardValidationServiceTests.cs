using System.Globalization;
using CardValidation.Core.Enums;
using CardValidation.Core.Services;
using NUnit.Framework;

namespace CardValidation.Core.Tests
{
    [TestFixture]
    public class CardValidationServiceTests
    {
        private CardValidationService _sut = null!;

        [SetUp]
        public void SetUp()
        {
            _sut = new CardValidationService();
        }

        [TestCase("John")]
        [TestCase("John Doe")]
        [TestCase("John R Doe")]
        public void ValidateOwner_WithAlphabeticNames_ReturnsTrue(string owner)
        {
            Assert.That(_sut.ValidateOwner(owner), Is.True);
        }

        [TestCase("John123")]
        [TestCase("John  Doe  Smith Jr")]
        [TestCase("")]
        public void ValidateOwner_WithInvalidNames_ReturnsFalse(string owner)
        {
            Assert.That(_sut.ValidateOwner(owner), Is.False);
        }

        [Test]
        public void ValidateIssueDate_WithFutureShortYearFormat_ReturnsTrue()
        {
            var futureDate = DateTime.UtcNow.AddMonths(6);
            var formatted = futureDate.ToString("MM/yy", CultureInfo.InvariantCulture);

            Assert.That(_sut.ValidateIssueDate(formatted), Is.True);
        }

        [Test]
        public void ValidateIssueDate_WithFutureLongYearFormat_ReturnsTrue()
        {
            var futureDate = DateTime.UtcNow.AddYears(2);
            var formatted = futureDate.ToString("MM/yyyy", CultureInfo.InvariantCulture);

            Assert.That(_sut.ValidateIssueDate(formatted), Is.True);
        }

        [Test]
        public void ValidateIssueDate_WithExpiredDate_ReturnsFalse()
        {
            var pastDate = DateTime.UtcNow.AddMonths(-3);
            var formatted = pastDate.ToString("MM/yy", CultureInfo.InvariantCulture);

            Assert.That(_sut.ValidateIssueDate(formatted), Is.False);
        }

        [Test]
        public void ValidateIssueDate_WithInvalidMonth_ReturnsFalse()
        {
            Assert.That(_sut.ValidateIssueDate("13/30"), Is.False);
        }

        [Test]
        public void ValidateIssueDate_WithInvalidFormat_ReturnsFalse()
        {
            Assert.That(_sut.ValidateIssueDate("2025-12"), Is.False);
        }

        [TestCase("123", ExpectedResult = true)]
        [TestCase("9876", ExpectedResult = true)]
        [TestCase("12", ExpectedResult = false)]
        [TestCase("abcd", ExpectedResult = false)]
        [TestCase("12345", ExpectedResult = false)]
        public bool ValidateCvc_ValidatesLengthAndDigits(string cvc)
        {
            return _sut.ValidateCvc(cvc);
        }

        [TestCase("4111111111111111", ExpectedResult = true, TestName = nameof(CardValidationService) + "_AcceptsVisaNumbers")]
        [TestCase("5555555555554444", ExpectedResult = true, TestName = nameof(CardValidationService) + "_AcceptsMasterCardNumbers")]
        [TestCase("371449635398431", ExpectedResult = true, TestName = nameof(CardValidationService) + "_AcceptsAmericanExpressNumbers")]
        [TestCase("6011111111111117", ExpectedResult = false, TestName = nameof(CardValidationService) + "_RejectsUnsupportedNumbers")]
        public bool ValidateNumber_ReturnsExpectedResult(string number) => _sut.ValidateNumber(number);

        [Test]
        public void GetPaymentSystemType_ForVisa_ReturnsVisa()
        {
            var result = _sut.GetPaymentSystemType("4111111111111111");
            Assert.That(result, Is.EqualTo(PaymentSystemType.Visa));
        }

        [Test]
        public void GetPaymentSystemType_ForMasterCard_ReturnsMasterCard()
        {
            var result = _sut.GetPaymentSystemType("5555555555554444");
            Assert.That(result, Is.EqualTo(PaymentSystemType.MasterCard));
        }

        [Test]
        public void GetPaymentSystemType_ForAmericanExpress_ReturnsAmericanExpress()
        {
            var result = _sut.GetPaymentSystemType("371449635398431");
            Assert.That(result, Is.EqualTo(PaymentSystemType.AmericanExpress));
        }

        [Test]
        public void GetPaymentSystemType_WithUnsupportedNumber_ThrowsNotImplementedException()
        {
            Assert.That(() => _sut.GetPaymentSystemType("1234567890123456"), Throws.TypeOf<NotImplementedException>());
        }
    }
}
