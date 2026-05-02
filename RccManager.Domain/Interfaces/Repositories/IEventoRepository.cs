using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using RccManager.Domain.Entities;

namespace RccManager.Domain.Interfaces.Repositories
{
    public interface IEventoRepository : IRepository<Evento>
    {
        Task<IEnumerable<Evento>> GetAll(string status);
        Task<IEnumerable<Evento>> GetAllHome();
        Task<IEnumerable<Evento>> GetEventsByEmail(string email);
        Task<IEnumerable<Evento>> GetAll(Guid userId);
        Task<Evento> GetById(Guid id);
        Task<string> GetSlug(Guid id);
        Task<Evento> GetSlug(string slug);
        Task<Evento> Update(Evento evento);
        Task<IEnumerable<Inscricao>> GetAllInscricoesByEvento(Guid eventoId);
        Task<IEnumerable<EventoCampos>> GetCamposByEvento(Guid eventoId);
        Task<int> GetLimiteParticipantes(Guid eventoId);
        Task<DataTable> ExportarInscricoes(Guid eventoId);
    }
}
