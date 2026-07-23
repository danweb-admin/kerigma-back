using RccManager.Domain.Entities;
using RccManager.Domain.Interfaces.Repositories;
using RccManager.Domain.Interfaces.Services;

namespace RccManager.Service.Services
{
    public class WalletMovimentoService : IWalletMovimentoService
    {
        private readonly IWalletMovimentoRepository repository;

        public WalletMovimentoService(
            IWalletMovimentoRepository repository)
        {
            this.repository = repository;
        }

        public async Task RegistrarMovimento(
            Wallet wallet,
            Financeiro financeiro,
            decimal valor,
            string tipo,
            string descricao,
            string origem,
            decimal saldoAnterior,
            decimal saldoAtual,
            DateTime dataMovimento)
        {
            var movimento = new WalletMovimento
            {
                WalletId = wallet.Id,
                EventoId = wallet.OrganizadorId,
                FinanceiroId = financeiro?.Id,

                Valor = valor,
                Tipo = tipo,
                Descricao = descricao,
                Origem = origem,

                SaldoAnterior = saldoAnterior,
                SaldoAtual = saldoAtual,

                DataMovimento = dataMovimento
            };

            await repository.Insert(movimento);
        }
    }
}