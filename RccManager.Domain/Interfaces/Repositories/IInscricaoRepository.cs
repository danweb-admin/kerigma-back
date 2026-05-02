using System;
using RccManager.Domain.Entities;

namespace RccManager.Domain.Interfaces.Repositories
{
	public interface IInscricaoRepository : IRepository<Inscricao>
    {
        Task<IEnumerable<Inscricao>> GetAll(Guid eventoId);
        Task<Inscricao> CheckByCpf(Guid eventId, string cpf);
        Task<Inscricao> GetByCodigo(string codigoInscricao);
        Task<bool> AnyInscricao(string codigoInscricao);
        Task<Inscricao> GetByInscricao(string codigoInscricao);
        Task InsertCamposDinamicos(InscricaoCampoValores camposDinamicos);
        Task<IEnumerable<Inscricao>> GetAllPending(DateTime dataBase);



    }
}

