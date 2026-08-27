using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using RccManager.Domain.Entities;
using RccManager.Domain.Interfaces.Repositories;
using RccManager.Infra.Context;

namespace RccManager.Infra.Repositories
{
    public class UsuarioCheckinRepository : BaseRepository<UsuariosCheckin>, IUsuarioCheckinRepository
    {
        private readonly DbSet<UsuariosCheckin> dbSet;

        public UsuarioCheckinRepository(AppDbContext _context) : base(_context)
        {
            dbSet = _context.Set<UsuariosCheckin>();
        }

        public new async Task<IEnumerable<UsuariosCheckin>> GetAll(string email, Guid userId)
        {
            var eventos = context.EventoUsuarios
                           .Where(x => x.UserId == userId)
                           .Select(x => x.EventoId).ToList();

            if (string.IsNullOrEmpty(email))
                return await dbSet.AsNoTracking()
                    .Include(x => x.Evento)
                    .Where(x => eventos.Contains(x.EventoId))
                    .ToListAsync();

            return await dbSet.Where(x => x.Email == email).ToListAsync();
        }

        public async Task<bool> GetByEmail(string email, Guid eventoId)
        {
            return await dbSet.AnyAsync(x => x.Email == email && x.Active && x.EventoId == eventoId);
        }

        public async Task<UsuariosCheckin> Login(string email, string senha)
        {
            return await dbSet.FirstOrDefaultAsync(x => x.Email == email && x.Senha == senha && x.Active);
        }
    }
}

