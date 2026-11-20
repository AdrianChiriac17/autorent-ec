using AutoRent.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AutoRent.Infrastructure.Persistence.Configurations;

// 02 | Users & cart

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("Users");
        b.Property(x => x.Email).HasMaxLength(256).IsRequired();
        b.Property(x => x.PasswordHash).HasColumnType("text").IsRequired();
        b.Property(x => x.FullName).HasMaxLength(150).IsRequired();
        b.Property(x => x.Phone).HasMaxLength(30);
        b.Property(x => x.AddressLine).HasColumnType("text");
        b.Property(x => x.City).HasMaxLength(100);
        b.Property(x => x.County).HasMaxLength(100);
        b.Property(x => x.PostalCode).HasMaxLength(20);
        b.Property(x => x.Country).HasMaxLength(100);

        // Emails are stored normalised (lower-case), so this unique index is case-insensitive in practice.
        b.HasIndex(x => x.Email).IsUnique();
    }
}

internal sealed class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> b)
    {
        b.ToTable("Carts");
        b.HasIndex(x => x.UserId).IsUnique();

        b.HasOne(x => x.User).WithOne(x => x.Cart)
            .HasForeignKey<Cart>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> b)
    {
        b.ToTable("CartItems", t =>
            t.HasCheckConstraint("CK_CartItems_Dates", "\"StartDate\" < \"EndDate\""));

        b.HasOne(x => x.Cart).WithMany(x => x.Items)
            .HasForeignKey(x => x.CartId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Car).WithMany(x => x.CartItems)
            .HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ExtraOptionConfiguration : IEntityTypeConfiguration<ExtraOption>
{
    public void Configure(EntityTypeBuilder<ExtraOption> b)
    {
        b.ToTable("ExtraOptions", t =>
            t.HasCheckConstraint("CK_ExtraOptions_Price", "\"Price\" >= 0"));
        b.Property(x => x.Name).HasMaxLength(100).IsRequired();
        b.HasIndex(x => x.Name).IsUnique();
    }
}

internal sealed class CartItemExtraOptionConfiguration : IEntityTypeConfiguration<CartItemExtraOption>
{
    public void Configure(EntityTypeBuilder<CartItemExtraOption> b)
    {
        b.ToTable("CartItemExtraOptions", t =>
            t.HasCheckConstraint("CK_CartItemExtraOptions_PriceSnapshot", "\"PriceSnapshot\" >= 0"));
        b.HasKey(x => new { x.CartItemId, x.ExtraOptionId });

        b.HasOne(x => x.CartItem).WithMany(x => x.ExtraOptions)
            .HasForeignKey(x => x.CartItemId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ExtraOption).WithMany()
            .HasForeignKey(x => x.ExtraOptionId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class WishlistItemConfiguration : IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> b)
    {
        b.ToTable("WishlistItems");
        b.HasKey(x => new { x.UserId, x.CarId });

        b.HasOne(x => x.User).WithMany(x => x.WishlistItems)
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Car).WithMany(x => x.WishlistItems)
            .HasForeignKey(x => x.CarId).OnDelete(DeleteBehavior.Cascade);
    }
}
