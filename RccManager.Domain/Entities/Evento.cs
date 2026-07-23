using System;
namespace RccManager.Domain.Entities
{
	
    public class Evento : BaseEntity
    {
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
        public int LimiteParticipantes { get; set; }
        public decimal TaxaServico { get; set; }
        public bool EventoGratuito { get; set; }

        public virtual Local Local { get; set; }
        public virtual Sobre Sobre { get; set; }
        public virtual Wallet Wallet { get; set; }

        public virtual InformacoesAdicionais InformacoesAdicionais { get; set; }
        public virtual ICollection<Participacao> Participacoes { get; set; }
        public virtual ICollection<LoteInscricao> LotesInscricoes { get; set; }
        public virtual ICollection<Programacao> Programacao { get; set; }
        public virtual ICollection<Inscricao> Inscricoes { get; set; }
        public virtual ICollection<UsuariosCheckin> UsuariosCheckin { get; set; }
        public virtual ICollection<EventoCampos> EventoCampos { get; set; }
        public virtual ICollection<EventoUsuarios> EventoUsuarios { get; set; }


    }
}

