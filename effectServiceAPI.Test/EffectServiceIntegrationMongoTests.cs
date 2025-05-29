using effectServiceAPI.Model;
using effectServiceAPI.Services;
using NUnit.Framework;

namespace effectServiceAPI.Test;

[TestFixture]
[Category("MockMongo")]
public class EffectServiceIntegrationMongoTests
{
    private EffectServiceIntegrationMongo _service;

    [SetUp]
    public void Setup()
    {
        _service = new EffectServiceIntegrationMongo();
    }

    [TearDown]
    public void TearDown()
    {
        _service.Dispose();
    }

    [Test]
    public void CreateEffect_ShouldBeRetrievable()
    {
        // Arrange
        var effect = new Effect
        {
            EffectId = Guid.NewGuid(),
            Title = "Test Effekt",
            Description = "Dette er en testeffekt",
            EffectStatus = EffectStatus.InStock,
            AppraisalId = Guid.NewGuid(),
            Seller = Guid.NewGuid(),
            MinimumPrice = 1000
        };

        // Act
        var created = _service.CreateEffect(effect);
        var retrieved = _service.GetEffect(created.EffectId);

        // Assert
        Assert.That(retrieved, Is.Not.Null);
        Assert.That(retrieved!.Title, Is.EqualTo("Test Effekt"));
    }

    [Test]
    public void DeleteEffect_ShouldRemoveIt()
    {
        // Arrange
        var effect = _service.CreateEffect(new Effect
        {
            EffectId = Guid.NewGuid(),
            Title = "Slet mig",
            Description = "Denne effekt skal slettes",
            EffectStatus = EffectStatus.InStock,
            AppraisalId = Guid.NewGuid(),
            Seller = Guid.NewGuid(),
            MinimumPrice = 1500
        });

        // Act
        _service.DeleteEffect(effect.EffectId);
        var result = _service.GetEffect(effect.EffectId);

        // Assert
        Assert.That(result, Is.Null);
    }
}