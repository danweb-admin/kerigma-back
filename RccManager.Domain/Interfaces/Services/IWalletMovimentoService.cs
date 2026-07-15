using RccManager.Domain.Entities;

namespace RccManager.Domain.Interfaces.Services
{
    public interface IWalletMovimentoService
    {
        Task RegistrarMovimento(
                Wallet wallet,
                Financeiro financeiro,
                decimal valor,
                string tipo,
                string descricao,
                string origem,
                decimal saldoAnterior,
                decimal saldoAtual);
    }
}