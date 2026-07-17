using Microsoft.EntityFrameworkCore;
using RccManager.Domain.Entities;
using RccManager.Domain.Interfaces.Repositories;
using RccManager.Infra.Context;

namespace RccManager.Infra.Repositories
{
    public class RepasseRepository : BaseRepository<Repasse>, IRepasseRepository
    {
        private readonly DbSet<Repasse> dbSet;

        public RepasseRepository(AppDbContext context)
            : base(context)
        {
            dbSet = context.Set<Repasse>();
        }

        public async Task<Repasse> GetById(Guid id)
        {
            return await dbSet
                .Include(x => x.Evento)
                .Include(x => x.Wallet)
                .Include(x => x.UsuarioAprovacao)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Repasse>> GetByEvento(Guid eventoId)
        {
            return await dbSet
                .Where(x => x.EventoId == eventoId)
                .Include(x => x.Wallet)
                .OrderByDescending(x => x.DataSolicitacao)
                .ToListAsync();
        }

        public async Task<IEnumerable<Repasse>> GetPendentes()
        {
            return await dbSet
                .Where(x => x.Status == "PENDENTE")
                .Include(x => x.Evento)
                .Include(x => x.Wallet)
                .OrderBy(x => x.DataSolicitacao)
                .ToListAsync();
        }

        public async Task<IEnumerable<Repasse>> GetByStatus(string status)
        {
            return await dbSet
                .Where(x => x.Status == status)
                .Include(x => x.Evento)
                .Include(x => x.Wallet)
                .OrderByDescending(x => x.DataSolicitacao)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPendente(Guid eventoId)
        {
            return await dbSet
                .Where(x => x.EventoId == eventoId && x.Status == "PENDENTE")
                .SumAsync(x => (decimal?)x.Valor) ?? 0;
        }

        public async Task<decimal> GetTotalPago(Guid eventoId)
        {
            return await dbSet
                .Where(x => x.EventoId == eventoId && x.Status == "PAGO")
                .SumAsync(x => (decimal?)x.Valor) ?? 0;
        }
    }
}