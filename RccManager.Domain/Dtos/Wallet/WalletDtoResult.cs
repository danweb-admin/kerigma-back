using System;
namespace RccManager.Domain.Dtos.Wallet
{
    public class WalletDtoResult
    {
        public Guid? Id { get; set; }

        public Guid OrganizadorId { get; set; }

        public decimal SaldoDisponivel { get; set; }

        public decimal SaldoPendente { get; set; }

        public decimal SaldoRepassado { get; set; }

        public decimal ReceitaTotal
        {
            get
            {
                return SaldoDisponivel + SaldoPendente + SaldoRepassado;
            }
        }
    }
}

