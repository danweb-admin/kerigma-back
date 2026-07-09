using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RccManager.Domain.Entities;

namespace RccManager.Infra.Mappings
{
    public class FinanceiroMap : IEntityTypeConfiguration<Financeiro>
    {
        public void Configure(EntityTypeBuilder<Financeiro> builder)
        {
            builder.ToTable("Financeiro");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired();

            builder.Property(x => x.InscricaoId)
                .IsRequired();

            builder.Property(x => x.OrganizadorId)
                .IsRequired();

            builder.Property(x => x.OrderId)
                .HasMaxLength(100);

            builder.Property(x => x.ChargeId)
                .HasMaxLength(100);

            builder.Property(x => x.CheckoutId)
                .HasMaxLength(100);

            builder.Property(x => x.ReferenceId)
                .HasMaxLength(100);

            builder.Property(x => x.NSU)
                .HasMaxLength(50);

            builder.Property(x => x.FormaPagamento)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(x => x.Parcelas)
                .HasDefaultValue(1);

            builder.Property(x => x.ValorBruto)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(x => x.TaxaServico)
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0);

            builder.Property(x => x.TaxaFinanceira)
                .HasColumnType("decimal(10,2)")
                .HasDefaultValue(0);

            builder.Property(x => x.ValorLiquido)
                .HasColumnType("decimal(10,2)")
                .IsRequired();

            builder.Property(x => x.StatusFinanceiro)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(x => x.DataPagamento);

            builder.Property(x => x.DataPrevistaRecebimento);

            builder.Property(x => x.DataRecebimento);

            builder.Property(x => x.DataRepasse);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnName("createdAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt");

            builder.Property(x => x.Active)
                .IsRequired()
                .HasColumnName("active");

            builder.HasOne(x => x.Inscricao)
                .WithOne(x => x.Financeiro)
                .HasForeignKey<Financeiro>(x => x.InscricaoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.OrderId)
                .IsUnique()
                .HasFilter("[OrderId] IS NOT NULL");

            builder.HasIndex(x => x.ChargeId)
                .IsUnique()
                .HasFilter("[ChargeId] IS NOT NULL");

            builder.HasIndex(x => x.StatusFinanceiro);

            builder.HasIndex(x => x.OrganizadorId);
        }
    }
}