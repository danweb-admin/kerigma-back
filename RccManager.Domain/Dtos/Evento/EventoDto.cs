using System;
using RccManager.Domain.Entities;

namespace RccManager.Domain.Dtos.Evento
{
    public class EventoDto
    {
        public Guid? Id { get; set; }
        public string Nome { get; set; }
        public string Slug { get; set; }
        public string BannerImagem { get; set; }
        public string CapaImagem { get; set; }

        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public string OrganizadorNome { get; set; }
        public string OrganizadorEmail { get; set; }
        public string OrganizadorContato { get; set; }
        public string Status { get; set; }
        public bool ExibirPregadores { get; set; }
        public bool ExibirProgramacao { get; set; }
        public bool ExibirInformacoesAdicionais { get; set; }
        public bool HabilitarPix { get; set; }
        public bool HabilitarCartao { get; set; }
        public bool HabilitarDinheiro { get; set; }
        public int QtdParcelas { get; set; }
        public int? LimiteParticipantes { get; set; }
        public decimal? TaxaServico { get; set; }
        public bool EventoGratuito { get; set; }


        // 🔗 Relações
        public LocalDto Local { get; set; }
        public SobreDto Sobre { get; set; }
        public InformacaoAdicionalDto InformacoesAdicionais { get; set; }

        public List<LoteDto> LotesInscricoes { get; set; }
        public List<ProgramacaoDto> Programacao { get; set; }
        public List<ParticipacaoDto> Participacoes { get; set; }
        public List<InscricaoDto> Inscricoes { get; set; } 
        public List<CamposFormularioDto> EventoCampos { get; set; } 

    }

    // -----------------------------------------
    // SUB-DTOs relacionados ao Evento
    // -----------------------------------------

    public class LocalDto
    {
        public Guid? Id { get; set; }
        public string ImagemMapa { get; set; }
        public string NomeLocal { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Endereco { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public Guid? EventoId { get; set; }
    }

    public class SobreDto
    {
        public Guid? Id { get; set; }
        public string Conteudo { get; set; }
        public Guid? EventoId { get; set; }
    }

    public class ParticipacaoDto
    {
        public Guid? Id { get; set; }
        public string Descricao { get; set; }
        public string Nome { get; set; }
        public string Foto { get; set; }
        public Guid? EventoId { get; set; }
    }

    public class LoteDto
    {
        public Guid? Id { get; set; }
        public string Nome { get; set; }
        public decimal Valor { get; set; }
        public int Quantidade { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public Guid? EventoId { get; set; }
    }

    public class ProgramacaoDto
    {
        public Guid? Id { get; set; }
        public string Dia { get; set; }
        public string Descricao { get; set; }
        public Guid? EventoId { get; set; }
    }

    public class InformacaoAdicionalDto
    {
        public Guid? Id { get; set; }
        public string Texto { get; set; }
        public Guid? EventoId { get; set; }
    }

    public class CamposFormularioDto
    {
        public Guid? Id { get; set; }
        public string Texto { get; set; }
        public Guid? EventoId { get; set; }
        public string Label { get; set; }
        public string NomeCampo { get; set; }
        public string Tipo { get; set; }
        public bool Obrigatorio { get; set; }
        public bool Active { get; set; }
        public List<string> Opcoes { get; set; }
        public int Ordem { get; set; }
    }

    public class InscricaoCampoValoresDto
    {
        public Guid? Id { get; set; }
        public Guid? InscricaoId { get; set; }
        public Guid EventoCampoId { get; set; }
        public string Valor { get; set; }
    }

    public class InscricaoDto
    {
        public Guid? Id { get; set; }
        public string CodigoInscricao { get; set; }
        public string Nome { get; set; }
        public string Cpf { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string GrupoOracao { get; set; }
        public string Decanato { get; set; }
        public string TipoPagamento { get; set; }
        public string Status { get; set; }
        public decimal ValorInscricao { get; set; }
        public Guid EventoId { get; set; }
        public Guid? DecanatoId { get; set; }
        public Guid? GrupoOracaoId { get; set; }
        public Guid? ServoId { get; set; }
        public string LinkQrCodePNG { get; set; }
        public string LinkQrCodeBase64 { get; set; }
        public string LinkPgtoCartao { get; set; }
        public string QRCodeCopiaCola { get; set; }
        public bool Active { get; set; }
        public bool? CheckIn { get; set; }
        public DateTime? DataCheckIn { get; set; }
        public decimal NumeroFatura { get; set; }
        public decimal ValorLiquido { get; set; }
        public decimal TaxaServico { get; set; }
        public decimal TaxaFinanceira { get; set; }
        public DateTime? DataLiberacao { get; set; }
        public string NSU { get; set; }
        public List<InscricaoCampoValoresDto> CamposDinamicos { get; set; }

        public InscricaoDto()
        {
            CamposDinamicos = new List<InscricaoCampoValoresDto>();
        }


    }
}

