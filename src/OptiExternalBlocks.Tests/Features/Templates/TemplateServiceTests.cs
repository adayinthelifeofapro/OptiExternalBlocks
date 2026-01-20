using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using OptiExternalBlocks.Entities;
using OptiExternalBlocks.Features.Templates.Models;
using OptiExternalBlocks.Features.Templates.Services;

namespace OptiExternalBlocks.Tests.Features.Templates;

[TestFixture]
public class TemplateServiceTests
{
    private OptiExternalBlocksDataContext _context = null!;
    private TemplateService _service = null!;
    private IMemoryCache _cache = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<OptiExternalBlocksDataContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new OptiExternalBlocksDataContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _service = new TemplateService(_context, _cache);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _cache.Dispose();
    }

    [Test]
    public async Task GetAllTemplatesAsync_ReturnsEmptyList_WhenNoTemplates()
    {
        // Act
        var result = await _service.GetAllTemplatesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task CreateTemplateAsync_CreatesTemplate_WithCorrectValues()
    {
        // Arrange
        var model = new TemplateModel
        {
            ContentTypeName = "Article",
            DisplayName = "Article Block",
            Description = "Display article content",
            EditModeTemplate = "<div>{{title}}</div>",
            RenderTemplate = "<article>{{title}}</article>",
            GraphQLQuery = "query { articles { items { title } } }",
            IconClass = "epi-iconDocument",
            IsActive = true,
            SortOrder = 1
        };

        // Act
        var result = await _service.CreateTemplateAsync(model, "testuser");

        // Assert
        result.Id.Should().NotBeEmpty();
        result.ContentTypeName.Should().Be("Article");
        result.DisplayName.Should().Be("Article Block");
        result.CreatedBy.Should().Be("testuser");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task GetTemplateByIdAsync_ReturnsTemplate_WhenExists()
    {
        // Arrange
        var model = new TemplateModel
        {
            ContentTypeName = "Product",
            DisplayName = "Product Block",
            EditModeTemplate = "<div>{{name}}</div>",
            RenderTemplate = "<div>{{name}}</div>",
            GraphQLQuery = "query { products { items { name } } }",
            IsActive = true
        };

        var created = await _service.CreateTemplateAsync(model, "testuser");

        // Act
        var result = await _service.GetTemplateByIdAsync(created.Id);

        // Assert
        result.Should().NotBeNull();
        result!.ContentTypeName.Should().Be("Product");
    }

    [Test]
    public async Task GetTemplateByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Act
        var result = await _service.GetTemplateByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task UpdateTemplateAsync_UpdatesTemplate_WithNewValues()
    {
        // Arrange
        var model = new TemplateModel
        {
            ContentTypeName = "News",
            DisplayName = "News Block",
            EditModeTemplate = "<div>{{headline}}</div>",
            RenderTemplate = "<div>{{headline}}</div>",
            GraphQLQuery = "query { news { items { headline } } }",
            IsActive = true
        };

        var created = await _service.CreateTemplateAsync(model, "testuser");

        // Act
        created.DisplayName = "Updated News Block";
        created.Description = "Updated description";
        var result = await _service.UpdateTemplateAsync(created, "updater");

        // Assert
        result.DisplayName.Should().Be("Updated News Block");
        result.Description.Should().Be("Updated description");
        result.ModifiedBy.Should().Be("updater");
        result.ModifiedAt.Should().NotBeNull();
    }

    [Test]
    public async Task DeleteTemplateAsync_RemovesTemplate()
    {
        // Arrange
        var model = new TemplateModel
        {
            ContentTypeName = "Event",
            DisplayName = "Event Block",
            EditModeTemplate = "<div>{{title}}</div>",
            RenderTemplate = "<div>{{title}}</div>",
            GraphQLQuery = "query { events { items { title } } }",
            IsActive = true
        };

        var created = await _service.CreateTemplateAsync(model, "testuser");

        // Act
        await _service.DeleteTemplateAsync(created.Id);
        var result = await _service.GetTemplateByIdAsync(created.Id);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetAllTemplatesAsync_ReturnsOrderedByDisplayName()
    {
        // Arrange
        await _service.CreateTemplateAsync(new TemplateModel
        {
            ContentTypeName = "Zebra",
            DisplayName = "Zebra",
            EditModeTemplate = "t",
            RenderTemplate = "t",
            GraphQLQuery = "q",
            SortOrder = 0
        }, "test");

        await _service.CreateTemplateAsync(new TemplateModel
        {
            ContentTypeName = "Apple",
            DisplayName = "Apple",
            EditModeTemplate = "t",
            RenderTemplate = "t",
            GraphQLQuery = "q",
            SortOrder = 0
        }, "test");

        await _service.CreateTemplateAsync(new TemplateModel
        {
            ContentTypeName = "Banana",
            DisplayName = "Banana",
            EditModeTemplate = "t",
            RenderTemplate = "t",
            GraphQLQuery = "q",
            SortOrder = 0
        }, "test");

        // Act
        var result = await _service.GetAllTemplatesAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].DisplayName.Should().Be("Apple");
        result[1].DisplayName.Should().Be("Banana");
        result[2].DisplayName.Should().Be("Zebra");
    }
}
