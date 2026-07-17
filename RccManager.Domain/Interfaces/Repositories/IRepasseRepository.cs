using RccManager.Domain.Entities;

namespace RccManager.Domain.Interfaces.Repositories
{
    public interface IRepasseRepository : IRepository<Repasse>
    {
        Task<IEnumerable<Repasse>> GetByEvento(Guid eventoId);

        Task<IEnumerable<Repasse>> GetPendentes();

        Task<Repasse> GetById(Guid id);

        Task<IEnumerable<Repasse>> GetByStatus(string status);

        Task<decimal> GetTotalPendente(Guid eventoId);

        Task<decimal> GetTotalPago(Guid eventoId);
    }
}