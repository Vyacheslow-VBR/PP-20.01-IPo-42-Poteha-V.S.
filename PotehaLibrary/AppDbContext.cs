using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PotehaLibrary.Models;
using System;

namespace PotehaLibrary
{
    public class AppDbContext : DbContext
    {
        public DbSet<PartnerType> PartnerTypes { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Sale> Sales { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql("Host=localhost;Database=poteha;Username=app2;Password=123456789;Encoding=UTF8;Search Path=app");
            optionsBuilder.EnableSensitiveDataLogging();
            optionsBuilder.EnableDetailedErrors();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasDefaultSchema("app");

            modelBuilder.Entity<PartnerType>().ToTable("partner_types_poteha");
            modelBuilder.Entity<Partner>().ToTable("partners_poteha");
            modelBuilder.Entity<Product>().ToTable("products_poteha");
            modelBuilder.Entity<Sale>().ToTable("sales_poteha");

            modelBuilder.Entity<PartnerType>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(100);
            });

            modelBuilder.Entity<Partner>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.TypeId).HasColumnName("type_id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
                entity.Property(e => e.DirectorFullname).HasColumnName("director_fullname").HasMaxLength(200);
                entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20);
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(100);
                entity.Property(e => e.Rating).HasColumnName("rating");
                entity.Property(e => e.Discount).HasColumnName("discount");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");

                entity.HasOne(e => e.PartnerType)
                    .WithMany()
                    .HasForeignKey(e => e.TypeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Sales)
                    .WithOne(e => e.Partner)
                    .HasForeignKey(e => e.PartnerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
                entity.Property(e => e.Article).HasColumnName("article").HasMaxLength(50);
                entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(10,2)");
            });

            modelBuilder.Entity<Sale>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.PartnerId).HasColumnName("partner_id");
                entity.Property(e => e.ProductId).HasColumnName("product_id");
                entity.Property(e => e.Quantity).HasColumnName("quantity");
                entity.Property(e => e.SaleDate).HasColumnName("sale_date");
                entity.Property(e => e.TotalAmount).HasColumnName("total_amount").HasColumnType("decimal(10,2)");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at");

                entity.HasOne(e => e.Partner)
                    .WithMany(e => e.Sales)
                    .HasForeignKey(e => e.PartnerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
                v => v.ToUniversalTime(),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            modelBuilder.Entity<Sale>()
                .Property(e => e.SaleDate)
                .HasConversion(dateTimeConverter);

            modelBuilder.Entity<Sale>()
                .Property(e => e.CreatedAt)
                .HasConversion(dateTimeConverter);

            modelBuilder.Entity<Partner>()
                .Property(e => e.CreatedAt)
                .HasConversion(dateTimeConverter);

            modelBuilder.Entity<Partner>()
                .Property(e => e.UpdatedAt)
                .HasConversion(dateTimeConverter);
        }
    }
}