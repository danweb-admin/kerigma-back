using Microsoft.EntityFrameworkCore;
using RccManager.Domain.Entities;
using RccManager.Domain.Interfaces.Repositories;
using RccManager.Infra.Context;

namespace RccManager.Infra.Repositories
{
    public class WalletMovimentoRepository : BaseRepository<WalletMovimento>, IWalletMovimentoRepository
    {
        private readonly DbSet<WalletMovimento> dbSet;

        public WalletMovimentoRepository(AppDbContext context)
            : base(context)
        {
            dbSet = context.Set<WalletMovimento>();
        }

        public async Task<IEnumerable<WalletMovimento>> GetByWallet(Guid walletId)
        {
            return await dbSet
                .Where(x => x.WalletId == walletId)
                .OrderByDescending(x => x.DataMovimento)
                .ToListAsync();
        }

        public async Task<IEnumerable<WalletMovimento>> GetByEvento(Guid eventoId)
        {
            return await dbSet
                .Where(x => x.EventoId == eventoId)
                .OrderByDescending(x => x.DataMovimento)
                .ToListAsync();
        }

        public async Task<IEnumerable<WalletMovimento>> GetExtrato(Guid eventoId,
            DateTime? dataInicial,
            DateTime? dataFinal)
        {
            var query = dbSet.Where(x => x.EventoId == eventoId);

            if (dataInicial.HasValue)
                query = query.Where(x => x.DataMovimento >= dataInicial.Value);

            if (dataFinal.HasValue)
                query = query.Where(x => x.DataMovimento <= dataFinal.Value);

            return await query
                .OrderByDescending(x => x.DataMovimento)
                .ToListAsync();
        }
    }
}