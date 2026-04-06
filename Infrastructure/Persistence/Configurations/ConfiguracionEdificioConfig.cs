using AdaByron.Domain.Aggregates.SpaceAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AdaByron.Infrastructure.Persistence.Configurations;

public class ConfiguracionEdificioConfig : IEntityTypeConfiguration<EdificioConfig>
{
    public void Configure(EntityTypeBuilder<EdificioConfig> builder)
    {
        builder.ToTable("edificio_config");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.PorcentajeOcupacion).IsRequired();
    }
}
