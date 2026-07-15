using RccManager.Domain.Entities;

namespace RccManager.Domain.Interfaces.Repositories
{
    public interface IWalletMovimentoRepository : IRepository<WalletMovimento>
    {
        Task<IEnumerable<WalletMovimento>> GetByWallet(Guid walletId);

        Task<IEnumerable<WalletMovimento>> GetByEvento(Guid eventoId);

        Task<IEnumerable<WalletMovimento>> GetExtrato(Guid eventoId, DateTime? dataInicial, DateTime? dataFinal);
    }
}