using System;
namespace RccManager.Domain.Responses
{
	public class InscricaoMQResponse
	{
        public string CodigoInscricao { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public decimal ValorInscricao { get; set; }
        public string NomeEvento { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public string Local { get; set; }
        public string OrganizadorNome { get; set; }
        public string Status { get; set; }

    }
}

