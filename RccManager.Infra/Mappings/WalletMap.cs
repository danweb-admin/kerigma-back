using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RccManager.Domain.Entities;

namespace RccManager.Infra.Mappings
{
    public class WalletMap : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.ToTable("Wallet");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired();

            builder.Property(x => x.OrganizadorId)
                .IsRequired();

            builder.Property(x => x.SaldoDisponivel)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(x => x.SaldoPendente)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(x => x.SaldoRepassado)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnName("createdAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt");

            builder.Property(x => x.Active)
                .IsRequired()
                .HasColumnName("active");

            builder.HasOne(x => x.Organizador)
                .WithOne(x => x.Wallet)
                .HasForeignKey<Wallet>(x => x.OrganizadorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.OrganizadorId)
                .IsUnique();
        }
    }
}