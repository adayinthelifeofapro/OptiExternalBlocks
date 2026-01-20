using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OptiExternalBlocks.Entities;
using OptiExternalBlocks.Features.ExternalContent.Models;
using OptiExternalBlocks.Features.Templates.Services;

namespace OptiExternalBlocks.Tests.Features.Templates;

[TestFixture]
public class TemplateRenderingServiceTests
{
    private OptiExternalBlocksDataContext _context = null!;
    private TemplateRenderingService _service = null!;
    private IMemoryCache _cache = null!;
    private Mock<ILogger<TemplateRenderingService>> _loggerMock = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<OptiExternalBlocksDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new OptiExternalBlocksDataContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<TemplateRenderingService>>();
        _service = new TemplateRenderingService(_context, _cache, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _cache.Dispose();
    }

    [Test]
    public void ValidateTemplate_ReturnsValid_ForCorrectTemplate()
    {
        // Arrange
        var template = "<div>{{title}}</div>";

        // Act
        var result = _service.ValidateTemplate(template);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public void ValidateTemplate_ReturnsInvalid_ForEmptyTemplate()
    {
        // Act
        var result = _service.ValidateTemplate("");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain("Template cannot be empty.");
    }

    [Test]
    public void ValidateTemplate_ReturnsValid_ForComplexTemplate()
    {
        // Arrange - valid complex template with sections
        var template = "<div>{{#items}}<span>{{name}}</span>{{/items}}</div>";

        // Act
        var result = _service.ValidateTemplate(template);

        // Assert - well-formed templates should validate
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Test]
    public void PreviewTemplate_RendersCorrectly_WithSimpleData()
    {
        // Arrange
        var template = "<h1>{{title}}</h1><p>{{description}}</p>";
        var sampleData = @"{""title"": ""Test Title"", ""description"": ""Test Description""}";

        // Act
        var result = _service.PreviewTemplate(template, sampleData);

        // Assert
        result.Should().Contain("<h1>Test Title</h1>");
        result.Should().Contain("<p>Test Description</p>");
    }

    [Test]
    public void PreviewTemplate_RendersSection_WhenDataExists()
    {
        // Arrange
        var template = "{{#hasImage}}<img src=\"{{imageUrl}}\" />{{/hasImage}}";
        var sampleData = @"{""hasImage"": true, ""imageUrl"": ""https://example.com/image.jpg""}";

        // Act
        var result = _service.PreviewTemplate(template, sampleData);

        // Assert
        result.Should().Contain("<img src=\"https://example.com/image.jpg\" />");
    }

    [Test]
    public void PreviewTemplate_DoesNotRenderSection_WhenDataMissing()
    {
        // Arrange
        var template = "{{#hasImage}}<img src=\"{{imageUrl}}\" />{{/hasImage}}";
        var sampleData = @"{""hasImage"": false}";

        // Act
        var result = _service.PreviewTemplate(template, sampleData);

        // Assert
        result.Should().NotContain("<img");
    }

    [Test]
    public void PreviewTemplate_ReturnsError_ForInvalidJson()
    {
        // Arrange
        var template = "<div>{{title}}</div>";
        var invalidJson = "not valid json";

        // Act
        var result = _service.PreviewTemplate(template, invalidJson);

        // Assert
        result.Should().Contain("epi-error");
        result.Should().Contain("JSON parsing error");
    }

    [Test]
    public async Task RenderEditModeAsync_ReturnsError_WhenTemplateNotFound()
    {
        // Arrange
        var content = new ExternalContentItem
        {
            Id = "123",
            Title = "Test",
            ContentType = "Article"
        };

        // Act
        var result = await _service.RenderEditModeAsync(Guid.NewGuid(), content);

        // Assert
        result.Should().Contain("epi-error");
        result.Should().Contain("Template not found");
    }

    [Test]
    public async Task RenderEditModeAsync_RendersContent_WithValidTemplate()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = new ExternalContentTemplate
        {
            Id = templateId,
            ContentTypeName = "Article",
            DisplayName = "Article",
            EditModeTemplate = "<div class=\"preview\">{{title}} - {{_contentType}}</div>",
            RenderTemplate = "<article>{{title}}</article>",
            GraphQLQuery = "query { }"
        };

        _context.Templates.Add(template);
        await _context.SaveChangesAsync();

        var content = new ExternalContentItem
        {
            Id = "123",
            Title = "My Article",
            ContentType = "Article",
            Data = new Dictionary<string, object>
            {
                ["title"] = "My Article"
            }
        };

        // Act
        var result = await _service.RenderEditModeAsync(templateId, content);

        // Assert
        result.Should().Contain("My Article");
        result.Should().Contain("Article");
        result.Should().Contain("preview");
    }

    [Test]
    public void PreviewTemplate_HandlesNestedObjects()
    {
        // Arrange
        var template = "<p>{{author.name}}</p>";
        var sampleData = @"{""author"": {""name"": ""John Doe""}}";

        // Act
        var result = _service.PreviewTemplate(template, sampleData);

        // Assert
        result.Should().Contain("<p>John Doe</p>");
    }

    [Test]
    public void PreviewTemplate_HandlesArrays()
    {
        // Arrange
        var template = "{{#tags}}<span>{{.}}</span>{{/tags}}";
        var sampleData = @"{""tags"": [""news"", ""featured"", ""tech""]}";

        // Act
        var result = _service.PreviewTemplate(template, sampleData);

        // Assert
        result.Should().Contain("<span>news</span>");
        result.Should().Contain("<span>featured</span>");
        result.Should().Contain("<span>tech</span>");
    }
}
