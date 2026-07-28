using Microsoft.EntityFrameworkCore;
using RccManager.Domain.Entities;
using RccManager.Domain.Interfaces.Repositories;
using RccManager.Infra.Context;

namespace RccManager.Infra.Repositories
{
    public class WalletRepository : BaseRepository<Wallet>, IWalletRepository
    {
        private readonly DbSet<Wallet> dbSet;

        public WalletRepository(AppDbContext context)
            : base(context)
        {
            dbSet = context.Set<Wallet>();
        }

        public async Task<Wallet> GetByOrganizador(Guid organizadorId)
        {
            return await dbSet
                .FirstOrDefaultAsync(x => x.OrganizadorId == organizadorId);
        }

        public async Task<bool> Exists(Guid organizadorId)
        {
            return await dbSet
                .AnyAsync(x => x.OrganizadorId == organizadorId);
        }

        public async Task<Wallet> GetOrCreate(Guid organizadorId)
        {
            var wallet = await GetByOrganizador(organizadorId);

            if (wallet != null)
                return wallet;

            wallet = new Wallet
            {
                OrganizadorId = organizadorId,
                SaldoDisponivel = 0,
                SaldoPendente = 0,
                SaldoRepassado = 0
            };

            await Insert(wallet);

            return wallet;
        }

        public async Task<Wallet> GetByEvento(Guid eventoId)
        {
            return await dbSet
                .FirstOrDefaultAsync(x => x.OrganizadorId == eventoId);
        }

        public async Task<IEnumerable<WalletMovimento>> GetExtrato(Guid eventoId)
        {
            return await context.WalletMovimentos
                .Include(x => x.Financeiro)
                .ThenInclude(x => x.Inscricao)
                .Where(x => x.EventoId == eventoId && x.Tipo != "LIBERACAO_CARTAO") 
                .OrderByDescending(x => x.DataMovimento)
                .ToListAsync();

        }
    }
}