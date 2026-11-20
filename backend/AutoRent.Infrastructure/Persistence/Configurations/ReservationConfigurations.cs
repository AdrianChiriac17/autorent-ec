using AutoRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoRent.Infrastructure.Persistence.Configurations;

// 04 | Reservations & reviews

internal sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> b)
    {
        b.ToTable("Reservations", t =>
            t.HasCheckConstraint("CK_Reservations_Dates", "\"StartDate\" < \"EndDate\""));
        b.Property(x => x.BookingReference).HasMaxLength(30).IsRequired();
        b.HasIndex(x => x.BookingReference).IsUnique();
        b.HasIndex(x => x.OrderItemId).IsUnique();
        b.HasIndex(x => new { x.CarId, x.StartDate, x.EndDate });
        b.HasIndex(x => x.Status);

        b.HasOne(x => x.OrderItem).WithOne(x => x.Reservation)
            .HasForeignKey<Reservation>(x => x.OrderItemId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Car).WithMany(x => x.Reservations)
            .HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class MaintenanceBlockConfiguration : IEntityTypeConfiguration<MaintenanceBlock>
{
    public void Configure(EntityTypeBuilder<MaintenanceBlock> b)
    {
        b.ToTable("MaintenanceBlocks", t =>
            t.HasCheckConstraint("CK_MaintenanceBlocks_Dates", "\"StartDate\" < \"EndDate\""));
        b.Property(x => x.Reason).HasColumnType("text");
        b.HasIndex(x => new { x.CarId, x.StartDate, x.EndDate });

        b.HasOne(x => x.Car).WithMany(x => x.MaintenanceBlocks)
            .HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> b)
    {
        b.ToTable("Reviews", t =>
            t.HasCheckConstraint("CK_Reviews_Rating", "\"Rating\" BETWEEN 1 AND 5"));
        b.Property(x => x.Comment).HasColumnType("text");
        b.HasIndex(x => x.ReservationId).IsUnique();

        b.HasOne(x => x.User).WithMany(x => x.Reviews)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Reservation).WithOne(x => x.Review)
            .HasForeignKey<Review>(x => x.ReservationId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CarModel).WithMany(x => x.Reviews)
            .HasForeignKey(x => x.CarModelId).OnDelete(DeleteBehavior.Restrict);
    }
}
