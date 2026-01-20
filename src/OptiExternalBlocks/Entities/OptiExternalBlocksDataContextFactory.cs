using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace OptiExternalBlocks.Entities;

public class OptiExternalBlocksDataContextFactory : IDesignTimeDbContextFactory<OptiExternalBlocksDataContext>
{
    public OptiExternalBlocksDataContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OptiExternalBlocksDataContext>();
        optionsBuilder.UseSqlServer("Server=.;Database=OptiGraphExtensions_DesignTime;Integrated Security=true;TrustServerCertificate=true;");

        return new OptiExternalBlocksDataContext(optionsBuilder.Options);
    }
}
