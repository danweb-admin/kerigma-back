using System;

namespace RccManager.Domain.Entities
{
    public class Repasse : BaseEntity
    {
        public Guid EventoId { get; set; }

        public Guid WalletId { get; set; }

        public decimal Valor { get; set; }

        public string Status { get; set; }

        public string Banco { get; set; }

        public string Agencia { get; set; }

        public string Conta { get; set; }

        public string TipoConta { get; set; }

        public string ChavePix { get; set; }

        public string TipoChavePix { get; set; }

        public string Observacao { get; set; }

        public DateTime DataSolicitacao { get; set; }

        public DateTime? DataAprovacao { get; set; }

        public DateTime? DataPagamento { get; set; }

        public Guid? UsuarioAprovacaoId { get; set; }

        public string Comprovante { get; set; }

        public virtual Evento Evento { get; set; }

        public virtual Wallet Wallet { get; set; }

        public virtual User UsuarioAprovacao { get; set; }
    }
}