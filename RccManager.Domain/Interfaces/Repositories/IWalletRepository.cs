using RccManager.Domain.Entities;

namespace RccManager.Domain.Interfaces.Repositories
{
    public interface IWalletRepository : IRepository<Wallet>
    {
        Task<Wallet> GetByOrganizador(Guid eventoId);
        Task<bool> Exists(Guid eventoId);
        Task<Wallet> GetOrCreate(Guid eventoId);
    }
}