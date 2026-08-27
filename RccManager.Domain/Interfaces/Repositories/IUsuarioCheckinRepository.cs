using System;
using RccManager.Domain.Entities;

namespace RccManager.Domain.Interfaces.Repositories
{
    public interface IUsuarioCheckinRepository : IRepository<UsuariosCheckin>
    {
        new Task<IEnumerable<UsuariosCheckin>> GetAll(string email, Guid userId);
        Task<UsuariosCheckin> Login(string email, string senha);
        Task<bool> GetByEmail(string email, Guid eventoId);



    }
}

