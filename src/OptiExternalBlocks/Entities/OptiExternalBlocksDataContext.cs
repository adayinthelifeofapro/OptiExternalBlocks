using Microsoft.EntityFrameworkCore;

namespace OptiExternalBlocks.Entities;

public class OptiExternalBlocksDataContext : DbContext, IOptiExternalBlocksDataContext
{
    public OptiExternalBlocksDataContext(DbContextOptions<OptiExternalBlocksDataContext> options)
        : base(options)
    {
    }

    public DbSet<ExternalContentTemplate> Templates { get; set; } = null!;
    public DbSet<ExternalContentReference> References { get; set; } = null!;
    public DbSet<GraphEndpointConfiguration> Endpoints { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ExternalContentTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ContentTypeName).IsUnique();
            entity.HasIndex(e => e.IsActive);
        });

        modelBuilder.Entity<ExternalContentReference>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ExternalId);
            entity.HasIndex(e => e.TemplateId);
            entity.HasOne(e => e.Template)
                  .WithMany()
                  .HasForeignKey(e => e.TemplateId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<GraphEndpointConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IsDefault);
            entity.HasIndex(e => e.IsActive);
        });
    }
}
