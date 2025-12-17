using CardValidation.Core.Services.Interfaces;
using CardValidation.Infrustructure;
using CardValidation.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using NSubstitute;
using NUnit.Framework;

namespace CardValidation.Core.Tests
{
    [TestFixture]
    public class CreditCardValidationFilterTests
    {
        [Test]
        public void OnActionExecuting_WithMissingFields_AddsRequiredErrors()
        {
            var validator = Substitute.For<ICardValidationService>();
            var filter = new CreditCardValidationFilter(validator);
            var creditCard = new CreditCard();
            var context = CreateContext(creditCard);

            filter.OnActionExecuting(context);

            AssertRequiredError(context, nameof(CreditCard.Owner));
            AssertRequiredError(context, nameof(CreditCard.Date));
            AssertRequiredError(context, nameof(CreditCard.Cvv));
            AssertRequiredError(context, nameof(CreditCard.Number));
        }

        [Test]
        public void OnActionExecuting_WithInvalidFields_AddsWrongValueErrors()
        {
            var validator = Substitute.For<ICardValidationService>();
            validator.ValidateOwner("John").Returns(false);
            validator.ValidateIssueDate("12/25").Returns(false);
            validator.ValidateCvc("123").Returns(false);
            validator.ValidateNumber("4111111111111111").Returns(false);

            var filter = new CreditCardValidationFilter(validator);
            var creditCard = new CreditCard
            {
                Owner = "John",
                Date = "12/25",
                Cvv = "123",
                Number = "4111111111111111"
            };
            var context = CreateContext(creditCard);

            filter.OnActionExecuting(context);

            AssertWrongValueError(context, nameof(CreditCard.Owner));
            AssertWrongValueError(context, nameof(CreditCard.Date));
            AssertWrongValueError(context, nameof(CreditCard.Cvv));
            AssertWrongValueError(context, nameof(CreditCard.Number));
        }

        [Test]
        public void OnActionExecuting_WithValidFields_DoesNotAddErrors()
        {
            var validator = Substitute.For<ICardValidationService>();
            validator.ValidateOwner(Arg.Any<string>()).Returns(true);
            validator.ValidateIssueDate(Arg.Any<string>()).Returns(true);
            validator.ValidateCvc(Arg.Any<string>()).Returns(true);
            validator.ValidateNumber(Arg.Any<string>()).Returns(true);

            var filter = new CreditCardValidationFilter(validator);
            var creditCard = new CreditCard
            {
                Owner = "Jane Doe",
                Date = "12/25",
                Cvv = "123",
                Number = "4111111111111111"
            };
            var context = CreateContext(creditCard);

            filter.OnActionExecuting(context);

            Assert.That(context.ModelState.IsValid, Is.True);
        }

        private static ActionExecutingContext CreateContext(CreditCard card)
        {
            var httpContext = new DefaultHttpContext();
            var modelState = new ModelStateDictionary();
            var actionContext = new ActionContext(httpContext, new RouteData(), new ControllerActionDescriptor(), modelState);
            var actionArguments = new Dictionary<string, object?>
            {
                ["creditCard"] = card
            };

            return new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), actionArguments, new object());
        }

        private static void AssertRequiredError(ActionExecutingContext context, string fieldName)
        {
            Assert.That(context.ModelState.TryGetValue(fieldName, out var entry), Is.True);
            Assert.That(entry!.Errors.Single().ErrorMessage, Is.EqualTo($"{fieldName} is required"));
        }

        private static void AssertWrongValueError(ActionExecutingContext context, string fieldName)
        {
            Assert.That(context.ModelState.TryGetValue(fieldName, out var entry), Is.True);
            Assert.That(entry!.Errors.Single().ErrorMessage, Is.EqualTo($"Wrong {fieldName.ToLowerInvariant()}"));
        }
    }
}
