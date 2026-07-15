using System;

namespace RccManager.Domain.Entities
{
    public class WalletMovimento : BaseEntity
    {
        public Guid WalletId { get; set; }

        public Guid? FinanceiroId { get; set; }

        public Guid EventoId { get; set; }

        public decimal Valor { get; set; }

        public string Tipo { get; set; }

        public string Descricao { get; set; }

        public decimal SaldoAnterior { get; set; }

        public decimal SaldoAtual { get; set; }

        public DateTime DataMovimento { get; set; }

        public string Origem { get; set; }

        public virtual Wallet Wallet { get; set; }

        public virtual Financeiro Financeiro { get; set; }

        public virtual Evento Evento { get; set; }
    }
}