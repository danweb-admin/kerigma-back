using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RccManager.Domain.Entities;

namespace RccManager.Infra.Mappings
{
    public class RepasseMap : IEntityTypeConfiguration<Repasse>
    {
        public void Configure(EntityTypeBuilder<Repasse> builder)
        {
            builder.ToTable("Repasse");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .IsRequired();

            builder.Property(x => x.EventoId)
                .IsRequired();

            builder.Property(x => x.WalletId)
                .IsRequired();

            builder.Property(x => x.Valor)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.Status)
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(x => x.Banco)
                .HasMaxLength(100);

            builder.Property(x => x.Agencia)
                .HasMaxLength(20);

            builder.Property(x => x.Conta)
                .HasMaxLength(30);

            builder.Property(x => x.TipoConta)
                .HasMaxLength(20);

            builder.Property(x => x.ChavePix)
                .HasMaxLength(200);

            builder.Property(x => x.TipoChavePix)
                .HasMaxLength(30);

            builder.Property(x => x.Observacao)
                .HasMaxLength(500);

            builder.Property(x => x.Comprovante)
                .HasMaxLength(500);

            builder.Property(x => x.DataSolicitacao)
                .IsRequired();

            builder.Property(x => x.DataAprovacao);

            builder.Property(x => x.DataPagamento);

            builder.Property(x => x.UsuarioAprovacaoId);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasColumnName("createdAt");

            builder.Property(x => x.UpdatedAt)
                .HasColumnName("updatedAt");

            builder.Property(x => x.Active)
                .IsRequired()
                .HasColumnName("active");

            builder.HasOne(x => x.Evento)
                .WithMany()
                .HasForeignKey(x => x.EventoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Wallet)
                .WithMany()
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.UsuarioAprovacao)
                .WithMany()
                .HasForeignKey(x => x.UsuarioAprovacaoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.EventoId);

            builder.HasIndex(x => x.WalletId);

            builder.HasIndex(x => x.Status);

            builder.HasIndex(x => x.DataSolicitacao);
        }
    }
}