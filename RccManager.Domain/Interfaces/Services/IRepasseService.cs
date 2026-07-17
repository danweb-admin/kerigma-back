using RccManager.Domain.Entities;

namespace RccManager.Domain.Interfaces.Services
{
    public interface IRepasseService
    {
        Task<Repasse> SolicitarRepasse(Guid eventoId, decimal valor);

        Task AprovarRepasse(Guid repasseId, Guid usuarioId);

        Task PagarRepasse(Guid repasseId, Guid usuarioId, string comprovante = null);

        Task CancelarRepasse(Guid repasseId, string motivo);

        Task<IEnumerable<Repasse>> GetByEvento(Guid eventoId);

        Task<IEnumerable<Repasse>> GetPendentes();

        Task<Repasse> GetById(Guid id);
    }
}