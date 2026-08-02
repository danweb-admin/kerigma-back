using RccManager.Domain.Dtos.Repasse;
using RccManager.Domain.Entities;
using RccManager.Domain.Interfaces.Repositories;
using RccManager.Domain.Interfaces.Services;

namespace RccManager.Service.Services
{
    public class RepasseService : IRepasseService
    {
        private readonly IRepasseRepository repasseRepository;
        private readonly IWalletRepository walletRepository;
        private readonly IWalletService walletService;

        public RepasseService(
            IRepasseRepository repasseRepository,
            IWalletRepository walletRepository,
            IWalletService walletService)
        {
            this.repasseRepository = repasseRepository;
            this.walletRepository = walletRepository;
            this.walletService = walletService;
        }

        public async Task<Repasse> SolicitarRepasse(RepasseDto repasse)
        {
            var wallet = await walletRepository.GetByOrganizador(repasse.EventoId);

            if (wallet == null)
                throw new Exception("Wallet não encontrada.");

            if (wallet.SaldoDisponivel < repasse.Valor)
                throw new Exception("Saldo insuficiente para solicitar o repasse.");

            var repasse_ = new Repasse
            {
                EventoId = repasse.EventoId,
                WalletId = wallet.Id,
                Valor = repasse.Valor,
                Status = "PENDENTE",
                DataSolicitacao = DateTime.Now,
                NomeBeneficiario = repasse.NomeBeneficiario.ToUpper(),
                EmailBeneficiario = repasse.EmailBeneficiario,
                ChavePix = repasse.ChavePix,
                TipoChavePix = repasse.TipoChavePix

            };

            return await repasseRepository.Insert(repasse_);
        }

        public async Task AprovarRepasse(Guid repasseId, Guid usuarioId)
        {
            var repasse = await repasseRepository.GetById(repasseId);

            if (repasse == null)
                throw new Exception("Repasse não encontrado.");

            if (repasse.Status != "PENDENTE")
                throw new Exception("Repasse já processado.");

            repasse.Status = "APROVADO";
            repasse.DataAprovacao = DateTime.Now;
            repasse.UsuarioAprovacaoId = usuarioId;

            await repasseRepository.Update(repasse);
        }

        public async Task PagarRepasse(Guid repasseId, Guid usuarioId, string comprovante = null)
        {
            var repasse = await repasseRepository.GetById(repasseId);

            if (repasse == null)
                throw new Exception("Repasse não encontrado.");

            if (repasse.Status != "APROVADO")
                throw new Exception("O repasse precisa estar aprovado.");

            await walletService.DebitarRepasse(repasse.EventoId, repasse.Valor);

            repasse.Status = "PAGO";
            repasse.DataPagamento = DateTime.Now;
            repasse.UsuarioAprovacaoId = usuarioId;
            repasse.Comprovante = comprovante;

            await repasseRepository.Update(repasse);
        }

        public async Task CancelarRepasse(Guid repasseId, string motivo)
        {
            var repasse = await repasseRepository.GetById(repasseId);

            if (repasse == null)
                throw new Exception("Repasse não encontrado.");

            if (repasse.Status == "PAGO")
                throw new Exception("Não é possível cancelar um repasse pago.");

            repasse.Status = "CANCELADO";
            repasse.Observacao = motivo;

            await repasseRepository.Update(repasse);
        }

        public async Task<IEnumerable<Repasse>> GetByEvento(Guid eventoId)
        {
            return await repasseRepository.GetByEvento(eventoId);
        }

        public async Task<IEnumerable<Repasse>> GetPendentes()
        {
            return await repasseRepository.GetPendentes();
        }

        public async Task<Repasse> GetById(Guid id)
        {
            return await repasseRepository.GetById(id);
        }

    }
}