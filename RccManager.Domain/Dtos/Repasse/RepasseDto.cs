using System;
namespace RccManager.Domain.Dtos.Repasse
{
    public class RepasseDto
    {
        public Guid EventoId { get; set; }
        public Guid WalletId { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; }
        public string NomeBeneficiario { get; set; }
        public string EmailBeneficiario { get; set; }
        public string ChavePix { get; set; }
        public string TipoChavePix { get; set; }
        public string Observacao { get; set; }
        public string Comprovante { get; set; }
    }
}

