using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RccManager.Domain.Entities;

namespace RccManager.Infra.Mappings
{
    public class WalletMovimentoMap : IEntityTypeConfiguration<WalletMovimento>
    {
        public void Configure(EntityTypeBuilder<WalletMovimento> builder)
        {
            builder.ToTable("WalletMovimento");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired();

            builder.Property(x => x.WalletId)
                .IsRequired();

            builder.Property(x => x.EventoId)
                .IsRequired();

            builder.Property(x => x.FinanceiroId);

            builder.Property(x => x.Valor)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.Tipo)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Descricao)
                .HasMaxLength(500);

            builder.Property(x => x.SaldoAnterior)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.SaldoAtual)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.DataMovimento)
                .IsRequired();

            builder.Property(x => x.Origem)
                .HasMaxLength(50);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnName("createdAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt");

            builder.Property(x => x.Active)
                .IsRequired()
                .HasColumnName("active");

            builder.HasOne(x => x.Wallet)
                .WithMany(x => x.Movimentos)
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Financeiro)
                .WithMany(x => x.WalletMovimentos)
                .HasForeignKey(x => x.FinanceiroId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Evento)
                .WithMany()
                .HasForeignKey(x => x.EventoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.WalletId);

            builder.HasIndex(x => x.EventoId);

            builder.HasIndex(x => x.FinanceiroId);

            builder.HasIndex(x => x.DataMovimento);
        }
    }
}