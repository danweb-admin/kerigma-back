using System;
namespace RccManager.Domain.Dtos.Evento
{
	public class EventoDtoResult
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
        public bool EventoGratuito { get; set; }


        // 🔗 Relações
        public LocalDto Local { get; set; }
        public SobreDto Sobre { get; set; }
        public InformacaoAdicionalDto InformacoesAdicionais { get; set; }

        public List<LoteDto> LotesInscricoes { get; set; }
        public List<ProgramacaoDto> Programacao { get; set; }
        public List<ParticipacaoDto> Participacoes { get; set; }
        public List<InscricaoDto> Inscricoes { get; set; }
        public List<CamposFormularioDto> CamposFormulario { get; set; } 

    }
}

