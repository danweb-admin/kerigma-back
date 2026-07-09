using RccManager.Domain.Entities;

namespace RccManager.Domain.Interfaces.Repositories
{
    public interface IFinanceiroRepository : IRepository<Financeiro>
    {
        Task<Financeiro> GetByInscricao(Guid inscricaoId);

        Task<Financeiro> GetByOrderId(string orderId);

        Task<Financeiro> GetByChargeId(string chargeId);

        Task<IEnumerable<Financeiro>> GetPendentesRecebimento();

        Task<IEnumerable<Financeiro>> GetByOrganizador(Guid organizadorId);

        Task<IEnumerable<Financeiro>> GetAll();

        Task<bool> ExistsByInscricao(Guid inscricaoId);
    }
}