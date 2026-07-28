using System;


namespace RccManager.Domain.Dtos.Wallet
{
    public class WalletMovimentoDtoResult
    {
        public Guid? Id { get; set; }

        public Guid WalletId { get; set; }

        public Guid OrganizadorId { get; set; }

        public Guid? FinanceiroId { get; set; }

        public Guid? RepasseId { get; set; }

        public DateTime DataMovimento { get; set; }

        public string Tipo { get; set; }

        public string Descricao { get; set; }

        public decimal Entrada { get; set; }

        public decimal Saida { get; set; }

        public decimal SaldoAnterior { get; set; }

        public decimal SaldoAtual { get; set; }

        public string Referencia { get; set; }

        public string Observacao { get; set; }

        public string Origem { get; set; }

        public string NomeParticipante { get; set; }

        public Entities.Financeiro Financeiro { get; set; }

    }
}

