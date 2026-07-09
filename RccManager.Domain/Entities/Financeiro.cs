using System;

namespace RccManager.Domain.Entities
{
    public class Financeiro : BaseEntity
    {
        // Relacionamentos
        public Guid InscricaoId { get; set; }
        public Guid OrganizadorId { get; set; }

        // Dados PagBank
        public string OrderId { get; set; }
        public string ChargeId { get; set; }
        public string CheckoutId { get; set; }
        public string ReferenceId { get; set; }
        public string NSU { get; set; }

        // Pagamento
        public string FormaPagamento { get; set; }
        public int Parcelas { get; set; }

        // Valores
        public decimal ValorBruto { get; set; }
        public decimal TaxaServico { get; set; }
        public decimal TaxaFinanceira { get; set; }
        public decimal ValorLiquido { get; set; }

        // Status
        public string StatusFinanceiro { get; set; }

        // Datas
        public DateTime? DataPagamento { get; set; }
        public DateTime? DataPrevistaRecebimento { get; set; }
        public DateTime? DataRecebimento { get; set; }
        public DateTime? DataRepasse { get; set; }

        // Navegação
        public virtual Inscricao Inscricao { get; set; }

        public Financeiro(Inscricao inscricao)
        {
            InscricaoId = inscricao.Id;

            ValorBruto = inscricao.ValorInscricao;
            ValorLiquido = inscricao.ValorLiquido;
            TaxaServico = inscricao.TaxaServico;
            TaxaFinanceira = inscricao.TaxaFinanceira;

            FormaPagamento = inscricao.TipoPagamento;
            DataPagamento = inscricao.DataPagamento;
        }
    }
}