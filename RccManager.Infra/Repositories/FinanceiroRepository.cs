using Microsoft.EntityFrameworkCore;
using RccManager.Domain.Entities;
using RccManager.Domain.Interfaces.Repositories;
using RccManager.Infra.Context;

namespace RccManager.Infra.Repositories
{
    public class FinanceiroRepository : BaseRepository<Financeiro>, IFinanceiroRepository
    {
        private readonly DbSet<Financeiro> dbSet;

        public FinanceiroRepository(AppDbContext context)
            : base(context)
        {
            dbSet = context.Set<Financeiro>();
        }

        public async Task<Financeiro> GetByInscricao(Guid inscricaoId)
        {
            return await dbSet
                .Include(x => x.Inscricao)
                .FirstOrDefaultAsync(x => x.InscricaoId == inscricaoId);
        }

        public async Task<Financeiro> GetByOrderId(string orderId)
        {
            return await dbSet
                .FirstOrDefaultAsync(x => x.OrderId == orderId);
        }

        public async Task<Financeiro> GetByChargeId(string chargeId)
        {
            return await dbSet
                .FirstOrDefaultAsync(x => x.ChargeId == chargeId);
        }

        public async Task<IEnumerable<Financeiro>> GetPendentesRecebimento()
        {
            return await dbSet
                .Where(x =>
                    x.StatusFinanceiro == "AGUARDANDO_RECEBIMENTO" &&
                    x.DataPrevistaRecebimento <= DateTime.Now)
                .ToListAsync();
        }

        public async Task<IEnumerable<Financeiro>> GetByOrganizador(Guid organizadorId)
        {
            return await dbSet
                .Include(x => x.Inscricao)
                .Where(x => x.OrganizadorId == organizadorId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> ExistsByInscricao(Guid inscricaoId)
        {
            return await dbSet.AnyAsync(x => x.InscricaoId == inscricaoId);

        }

        public Task<IEnumerable<Financeiro>> GetAll()
        {
            throw new NotImplementedException();
        }
    }
}