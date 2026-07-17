using System;
using RccManager.Domain.Interfaces.Services;

namespace RccManager.Service.Job
{
    public class LiberarRecebimentoCartaoJob
    {
        private readonly IFinanceiroService financeiroService;

        public LiberarRecebimentoCartaoJob(IFinanceiroService financeiroService)
        {
            this.financeiroService = financeiroService;
        }

        public async Task Execute()
        {
            Console.WriteLine("Iniciando liberação de recebimentos de cartão...");

            await financeiroService.LiberarRecebimentosCartao();

            Console.WriteLine("Liberação concluída.");
        }
    }
}

