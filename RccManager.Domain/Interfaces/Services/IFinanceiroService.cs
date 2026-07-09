using RccManager.Domain.Dtos.Financeiro;
using RccManager.Domain.Entities;
using RccManager.Domain.Responses;

namespace RccManager.Domain.Interfaces.Services
{
    public interface IFinanceiroService
    {
        Task<HttpResponse> Create(FinanceiroDto financeiroDto);

        Task<HttpResponse> Update(FinanceiroDto financeiroDto, Guid id);

        Task<HttpResponse> Delete(Guid id);

        Task<HttpResponse> ActivateDeactivate(Guid id);

        Task<IEnumerable<FinanceiroDtoResult>> GetAll();

        Task RegistrarPagamento(Financeiro financeiro);

        Task RegistrarFinanceiro(Inscricao inscricao, PagSeguroWebhook webhook);
    }
}