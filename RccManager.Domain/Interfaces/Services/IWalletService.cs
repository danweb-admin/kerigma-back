using RccManager.Domain.Dtos.Wallet;
using RccManager.Domain.Entities;

namespace RccManager.Domain.Interfaces.Services
{
    public interface IWalletService
    {
        Task CreditarPix(Financeiro financeiro);

        Task CreditarCartao(Financeiro financeiro);

        Task LiberarSaldo(Financeiro financeiro);

        Task DebitarRepasse(Guid organizadorId, decimal valor);

        Task<Wallet> GetByOrganizador(Guid organizadorId);

        Task<WalletDtoResult> GetByEvento(Guid eventoId);

        Task<IEnumerable<WalletMovimentoDtoResult>> GetExtrato(Guid eventoId);
    }
}