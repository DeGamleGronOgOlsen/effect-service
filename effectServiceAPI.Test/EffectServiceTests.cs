using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using auctionServiceAPI.Controllers;
using auctionServiceAPI.Services;
using effectServiceAPI.Model;

namespace auctionServiceAPI.Tests.Services
{
    [TestFixture]
    public class EffectServiceTests
    {
        private Mock<ILogger<EffectController>> _mockLogger;
        private Mock<IEffectService> _mockEffectService;
        private Mock<IConfiguration> _mockConfig;
        private EffectController _controller;

        [SetUp]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<EffectController>>();
            _mockEffectService = new Mock<IEffectService>();
            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["EffectImagePath"]).Returns("/fake/path");

            _controller = new EffectController(_mockLogger.Object, _mockEffectService.Object, _mockConfig.Object);
        }

        [Test]
        public async Task GetAllEffects_ShouldReturnOkWithEffects()
        {
            var effects = new List<Effect>
            {
                new() { EffectId = Guid.NewGuid(), Title = "Effect 1" }
            };
            _mockEffectService.Setup(s => s.GetAllEffectsAsync()).ReturnsAsync(effects);

            var result = await _controller.GetAllEffects();

            var okResult = result.Result as OkObjectResult;
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            Assert.AreEqual(effects, okResult?.Value);
        }

        [Test]
        public async Task GetEffect_ShouldReturnEffect_WhenExists()
        {
            var id = Guid.NewGuid();
            var effect = new Effect { EffectId = id, Title = "Test" };
            _mockEffectService.Setup(s => s.GetEffectAsync(id)).ReturnsAsync(effect);

            var result = await _controller.GetEffect(id);

            var okResult = result.Result as OkObjectResult;
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            Assert.AreEqual(effect, okResult?.Value);
        }

        [Test]
        public async Task GetEffect_ShouldReturnNotFound_WhenMissing()
        {
            var id = Guid.NewGuid();
            _mockEffectService.Setup(s => s.GetEffectAsync(id)).ReturnsAsync((Effect?)null);

            var result = await _controller.GetEffect(id);

            Assert.IsInstanceOf<NotFoundResult>(result.Result);
        }

        [Test]
        public async Task CreateEffect_ShouldReturnCreated_WhenValidEffect()
        {
            var effect = new Effect
            {
                EffectId = Guid.NewGuid(),
                Title = "New Effect",
                Description = "Test Description"
            };

            _mockEffectService
                .Setup(s => s.CreateEffectAsync(It.IsAny<Effect>()))
                .ReturnsAsync(effect.EffectId);

            var result = await _controller.CreateEffect(effect, null);

            var createdResult = result.Result as CreatedAtActionResult;
            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
            Assert.AreEqual(nameof(_controller.GetEffect), createdResult?.ActionName);
            Assert.AreEqual(effect, createdResult?.Value);
        }

        [Test]
        public async Task TransferToAuction_ShouldReturnOk_WhenSuccess()
        {
            var id = Guid.NewGuid();
            var effect = new Effect
            {
                EffectId = id,
                EffectStatus = EffectStatus.InStock
            };

            _mockEffectService.Setup(s => s.GetEffectAsync(id)).ReturnsAsync(effect);
            _mockEffectService.Setup(s => s.TransferToAuctionAsync(id)).ReturnsAsync(true);

            var result = await _controller.TransferToAuction(id);

            var okResult = result as OkObjectResult;
            Assert.IsInstanceOf<OkObjectResult>(result);
            Assert.AreEqual("Effect successfully transferred to auction", okResult?.Value);
        }
    }
}
