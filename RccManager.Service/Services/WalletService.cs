using AutoMapper;
using RccManager.Domain.Dtos.Wallet;
using RccManager.Domain.Entities;
using RccManager.Domain.Interfaces.Repositories;
using RccManager.Domain.Interfaces.Services;

namespace RccManager.Service.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository walletRepository;
        private readonly IWalletMovimentoService walletMovimentoService;
        private readonly IRepasseRepository repasseRepository; 
        private readonly IMapper mapper;


        public WalletService(
            IWalletRepository walletRepository,
            IWalletMovimentoService walletMovimentoService,
            IMapper mapper,
            IRepasseRepository repasseRepository)
        {
            this.walletRepository = walletRepository;
            this.walletMovimentoService = walletMovimentoService;
            this.mapper = mapper;
            this.repasseRepository = repasseRepository;
        }

        public async Task<Wallet> GetByOrganizador(Guid organizadorId)
        {
            return await walletRepository.GetByOrganizador(organizadorId);
        }

        public async Task CreditarPix(Financeiro financeiro)
        {
            var wallet = await walletRepository.GetOrCreate(financeiro.OrganizadorId);

            var saldoAnterior = wallet.SaldoDisponivel;

            wallet.SaldoDisponivel += financeiro.ValorLiquido;

            await walletRepository.Update(wallet);

            await walletMovimentoService.RegistrarMovimento(
                wallet,
                financeiro,
                financeiro.ValorLiquido,
                WalletTipoMovimento.CreditoPix,
                "Pagamento recebido via PIX",
                "WEBHOOK",
                saldoAnterior,
                wallet.SaldoDisponivel,
                financeiro.DataPagamento.Value);
        }

        public async Task CreditarCartao(Financeiro financeiro)
        {
            var wallet = await walletRepository.GetOrCreate(financeiro.OrganizadorId);

            var saldoAnterior = wallet.SaldoPendente;

            wallet.SaldoPendente += financeiro.ValorLiquido;

            await walletRepository.Update(wallet);

            await walletMovimentoService.RegistrarMovimento(
                wallet,
                financeiro,
                financeiro.ValorLiquido,
                WalletTipoMovimento.CreditoCartao,
                "Pagamento recebido via Cartão",
                "WEBHOOK",
                saldoAnterior,
                wallet.SaldoPendente,
                financeiro.DataPagamento.Value);
        }

        public async Task LiberarSaldo(Financeiro financeiro)
        {
            var wallet = await walletRepository.GetByOrganizador(financeiro.OrganizadorId);

            if (wallet == null)
                return;

            var saldoAnterior = wallet.SaldoDisponivel;

            wallet.SaldoPendente -= financeiro.ValorLiquido;
            wallet.SaldoDisponivel += financeiro.ValorLiquido;

            await walletRepository.Update(wallet);

            await walletMovimentoService.RegistrarMovimento(
                wallet,
                financeiro,
                financeiro.ValorLiquido,
                WalletTipoMovimento.LiberacaoCartao,
                "Liberação automática D+14",
                "JOB",
                saldoAnterior,
                wallet.SaldoDisponivel,
                financeiro.DataPagamento.Value);
        }

        public async Task DebitarRepasse(Guid organizadorId, decimal valor)
        {
            var wallet = await walletRepository.GetByOrganizador(organizadorId);

            if (wallet == null)
                throw new Exception("Wallet não encontrada.");

            if (wallet.SaldoDisponivel < valor)
                throw new Exception("Saldo insuficiente.");

            var saldoAnterior = wallet.SaldoDisponivel;

            wallet.SaldoDisponivel -= valor;
            wallet.SaldoRepassado += valor;

            await walletRepository.Update(wallet);

            await walletMovimentoService.RegistrarMovimento(
                wallet,
                null,
                -valor,
                WalletTipoMovimento.Repasse,
                "Repasse realizado",
                "REPASSE",
                saldoAnterior,
                wallet.SaldoDisponivel,
                DateTime.Now);
        }

        public async Task<WalletDtoResult> GetByEvento(Guid eventoId)
        {
            return mapper.Map<WalletDtoResult>(
                await walletRepository.GetByEvento(eventoId));
        }

        public async Task<IEnumerable<WalletMovimentoDtoResult>> GetExtrato(Guid eventoId)
        {
            var movimentos = mapper.Map<IEnumerable<WalletMovimentoDtoResult>>(await walletRepository.GetExtrato(eventoId));
            decimal saldo = 0;

            foreach(var mov in movimentos.OrderBy(x => x.DataMovimento))
            {
                mov.SaldoAnterior = saldo;

                saldo += mov.Entrada;
                if (mov.Saida < 0)
                {
                    mov.Saida = mov.Saida * -1;
                    saldo -= mov.Saida ;
                }
                

                mov.SaldoAtual = saldo;

                if (mov.Financeiro != null)
                {
                    mov.Referencia = mov.Financeiro.ReferenceId;
                    mov.NomeParticipante = mov.Financeiro.Inscricao.Nome;
                }

                if (mov.Origem == "REPASSE")
                {
                    var repasses = await repasseRepository.GetByEvento(eventoId);
                    var repasse = repasses.FirstOrDefault(x => x.Valor == mov.Saida);

                    if (repasse != null)
                    {
                        mov.Comprovante = repasse.Comprovante;
                    }
                }
                
                
            }

            return movimentos;
        }
    }
}