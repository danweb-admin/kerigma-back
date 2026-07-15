using System.Net;
using AutoMapper;
using RccManager.Domain.Dtos.Financeiro;
using RccManager.Domain.Entities;
using RccManager.Domain.Interfaces.Repositories;
using RccManager.Domain.Interfaces.Services;
using RccManager.Domain.Responses;
using RccManager.Service.Enum;

namespace RccManager.Service.Services
{
    public class FinanceiroService : IFinanceiroService
    {
        private readonly IMapper mapper;
        private readonly IFinanceiroRepository repository;
        private readonly IHistoryRepository history;
        private readonly IWalletService walletService;


        public FinanceiroService(
            IMapper mapper,
            IFinanceiroRepository repository,
            IHistoryRepository history,
            IWalletService walletService)
        {
            this.mapper = mapper;
            this.repository = repository;
            this.history = history;
            this.walletService = walletService;
        }

        public async Task<HttpResponse> Create(FinanceiroDto financeiroDto)
        {
            var entity = mapper.Map<Financeiro>(financeiroDto);

            var result = await repository.Insert(entity);

            if (result == null)
            {
                return new HttpResponse
                {
                    Message = "Houve um problema ao criar o financeiro.",
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }

            await history.Add(TableEnum.Financeiro.ToString(),
                result.Id,
                OperationEnum.Criacao.ToString());

            return new HttpResponse
            {
                Message = "Financeiro criado com sucesso.",
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        public async Task<HttpResponse> Update(FinanceiroDto financeiroDto, Guid id)
        {
            var entity = mapper.Map<Financeiro>(financeiroDto);
            entity.Id = id;

            var result = await repository.Update(entity);

            if (result == null)
            {
                return new HttpResponse
                {
                    Message = "Houve um problema ao atualizar o financeiro.",
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }

            await history.Add(TableEnum.Financeiro.ToString(),
                result.Id,
                OperationEnum.Alteracao.ToString());

            return new HttpResponse
            {
                Message = "Financeiro atualizado com sucesso.",
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        public async Task<HttpResponse> Delete(Guid id)
        {
            var result = await repository.Delete(id);

            if (result)
            {
                return new HttpResponse
                {
                    Message = "Financeiro removido com sucesso.",
                    StatusCode = (int)HttpStatusCode.OK
                };
            }

            return new HttpResponse
            {
                Message = "Erro ao remover o financeiro.",
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        public async Task<HttpResponse> ActivateDeactivate(Guid id)
        {
            var entity = await repository.GetById(id);

            entity.Active = !entity.Active;

            var result = await repository.Update(entity);

            if (result == null)
            {
                return new HttpResponse
                {
                    Message = "Erro ao alterar o status.",
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }

            return new HttpResponse
            {
                Message = "Status alterado com sucesso.",
                StatusCode = (int)HttpStatusCode.OK
            };
        }

        public async Task<IEnumerable<FinanceiroDtoResult>> GetAll()
        {
            return mapper.Map<IEnumerable<FinanceiroDtoResult>>(await repository.GetAll());
        }

        public async Task RegistrarPagamento(Financeiro financeiro)
        {
            var existe = await repository.ExistsByInscricao(financeiro.InscricaoId);

            if (existe)
                return;

            await repository.Insert(financeiro);

            await history.Add(
                TableEnum.Financeiro.ToString(),
                financeiro.Id,
                OperationEnum.Criacao.ToString());
        }

        public async Task RegistrarFinanceiro(Inscricao inscricao,PagSeguroWebhook webhook)
        {
            var exists = await repository.ExistsByInscricao(inscricao.Id);

            if (exists)
                return;

            var charge = webhook.Charges.First();

            var financeiro = new Financeiro(inscricao)
            {
                OrganizadorId = inscricao.EventoId, // depois ajustar para o organizador correto

                OrderId = webhook.Id,
                ChargeId = charge.Id,
                ReferenceId = webhook.Reference_Id,
                NSU = charge.Payment_Response.Reference,
                Parcelas = charge.Payment_Method.Installments,
                DataPrevistaRecebimento = inscricao.DataLiberacao,

                StatusFinanceiro =
                    inscricao.TipoPagamento == "pix"
                        ? "RECEBIDO"
                        : "AGUARDANDO_RECEBIMENTO"
            };

            financeiro = await repository.Insert(financeiro);

            // Atualiza a Wallet
            if (financeiro.StatusFinanceiro == "RECEBIDO")
                await walletService.CreditarPix(financeiro);
            else
                await walletService.CreditarCartao(financeiro);
        }
    }
}