using AutoRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoRent.Infrastructure.Persistence.Configurations;

// 01 | Catalog & fleet

internal sealed class CarBrandConfiguration : IEntityTypeConfiguration<CarBrand>
{
    public void Configure(EntityTypeBuilder<CarBrand> b)
    {
        b.ToTable("CarBrands");
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
    }
}

internal sealed class CarModelConfiguration : IEntityTypeConfiguration<CarModel>
{
    public void Configure(EntityTypeBuilder<CarModel> b)
    {
        b.ToTable("CarModels", t =>
        {
            t.HasCheckConstraint("CK_CarModels_Seats", "\"Seats\" BETWEEN 1 AND 9");
            t.HasCheckConstraint("CK_CarModels_BasePricePerDay", "\"BasePricePerDay\" >= 0");
            t.HasCheckConstraint("CK_CarModels_ModelYear", "\"ModelYear\" BETWEEN 1990 AND 2100");
        });
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.Property(x => x.Description).HasColumnType("text");
        b.HasIndex(x => x.Name);

        b.HasOne(x => x.CarBrand).WithMany(x => x.CarModels)
            .HasForeignKey(x => x.CarBrandId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CarModelImageConfiguration : IEntityTypeConfiguration<CarModelImage>
{
    public void Configure(EntityTypeBuilder<CarModelImage> b)
    {
        b.ToTable("CarModelImages");
        b.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
        b.HasIndex(x => new { x.CarModelId, x.SortOrder });

        b.HasOne(x => x.CarModel).WithMany(x => x.Images)
            .HasForeignKey(x => x.CarModelId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class FeatureDefinitionConfiguration : IEntityTypeConfiguration<FeatureDefinition>
{
    public void Configure(EntityTypeBuilder<FeatureDefinition> b)
    {
        b.ToTable("FeatureDefinitions");
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
        b.HasIndex(x => x.FeatureGroup);
    }
}

internal sealed class CarFeatureConfiguration : IEntityTypeConfiguration<CarFeature>
{
    public void Configure(EntityTypeBuilder<CarFeature> b)
    {
        b.ToTable("CarFeatures", t =>
            t.HasCheckConstraint("CK_CarFeatures_ExtraPricePerDay", "\"ExtraPricePerDay\" >= 0"));

        // A global feature can be priced at most once per model.
        b.HasIndex(x => new { x.CarModelId, x.FeatureDefinitionId }).IsUnique();

        b.HasOne(x => x.CarModel).WithMany(x => x.CarFeatures)
            .HasForeignKey(x => x.CarModelId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.FeatureDefinition).WithMany(x => x.CarFeatures)
            .HasForeignKey(x => x.FeatureDefinitionId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> b)
    {
        b.ToTable("Cars");
        b.Property(x => x.Vin).HasColumnName("VIN").HasMaxLength(17).IsRequired();
        b.Property(x => x.LicensePlate).HasMaxLength(15).IsRequired();
        b.HasIndex(x => x.Vin).IsUnique();
        b.HasIndex(x => x.LicensePlate).IsUnique();
        b.HasIndex(x => x.Status);

        b.HasOne(x => x.CarModel).WithMany(x => x.Cars)
            .HasForeignKey(x => x.CarModelId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class CarFeatureAssignmentConfiguration : IEntityTypeConfiguration<CarFeatureAssignment>
{
    public void Configure(EntityTypeBuilder<CarFeatureAssignment> b)
    {
        b.ToTable("CarFeatureAssignments");
        b.HasKey(x => new { x.CarId, x.CarFeatureId });

        b.HasOne(x => x.Car).WithMany(x => x.FeatureAssignments)
            .HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.CarFeature).WithMany(x => x.Assignments)
            .HasForeignKey(x => x.CarFeatureId).OnDelete(DeleteBehavior.Restrict);
    }
}
