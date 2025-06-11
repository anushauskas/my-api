using CleanArchitecture.Application.Common.Exceptions;
using FluentValidation.Results;
using NUnit.Framework;

namespace CleanArchitecture.Application.UnitTests.Common.Exceptions;

public class ValidationExceptionTests
{
    [Test]
    public void DefaultConstructorCreatesAnEmptyErrorDictionary()
    {
        var actual = new ValidationException().Errors;

        Assert.That(actual.Keys, Is.EquivalentTo(Array.Empty<string>()));
    }

    [Test]
    public void SingleValidationFailureCreatesASingleElementErrorDictionary()
    {
        var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Age", "must be over 18"),
            };

        var actual = new ValidationException(failures).Errors;

        Assert.That(actual.Keys, Is.EquivalentTo(["Age"]));
        Assert.That(actual["Age"], Is.EquivalentTo(["must be over 18"]));
    }

    [Test]
    public void MulitpleValidationFailureForMultiplePropertiesCreatesAMultipleElementErrorDictionaryEachWithMultipleValues()
    {
        var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Age", "must be 18 or older"),
                new ValidationFailure("Age", "must be 25 or younger"),
                new ValidationFailure("Password", "must contain at least 8 characters"),
                new ValidationFailure("Password", "must contain a digit"),
                new ValidationFailure("Password", "must contain upper case letter"),
                new ValidationFailure("Password", "must contain lower case letter"),
            };

        var actual = new ValidationException(failures).Errors;

        Assert.That(actual.Keys, Is.EquivalentTo(["Password", "Age"]));

        Assert.That(actual["Age"], Is.EquivalentTo(
        [
                "must be 25 or younger",
                "must be 18 or older",
        ]));

        Assert.That(actual["Password"], Is.EquivalentTo(
        [
                "must contain lower case letter",
                "must contain upper case letter",
                "must contain at least 8 characters",
                "must contain a digit",
        ]));
    }
}
