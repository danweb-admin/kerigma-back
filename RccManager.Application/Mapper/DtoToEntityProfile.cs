using System;
using AutoMapper;
using RccManager.Domain.Dtos.DecanatoSetor;
using RccManager.Domain.Dtos.Evento;
using RccManager.Domain.Dtos.EventoUsuarios;
using RccManager.Domain.Dtos.Financeiro;
using RccManager.Domain.Dtos.Formacao;
using RccManager.Domain.Dtos.FormacoesServo;
using RccManager.Domain.Dtos.GrupoOracao;
using RccManager.Domain.Dtos.InscricoesEvento;
using RccManager.Domain.Dtos.ParoquiaCapela;
using RccManager.Domain.Dtos.Servo;
using RccManager.Domain.Dtos.ServoTemp;
using RccManager.Domain.Dtos.TransferenciaServico;
using RccManager.Domain.Dtos.Users;
using RccManager.Domain.Dtos.UsuarioCheckin;
using RccManager.Domain.Entities;
using RccManager.Service.Helper;
using static RccManager.Application.Mapper.EntityToDtoProfile;

namespace RccManager.Application.Mapper;

public class DtoToEntityProfile : Profile
{
    public DtoToEntityProfile()
    {
        CreateMap<DecanatoSetor, DecanatoSetorDto>()
            .ReverseMap();

        CreateMap<ParoquiaCapela, ParoquiaCapelaDto>()
            .ReverseMap();

        CreateMap<GrupoOracao, GrupoOracaoDto>()
            .ReverseMap();

        CreateMap<GrupoOracao, GrupoOracaoDtoResult>()
            .ReverseMap();

        CreateMap<Servo, ServoDto>()
            .ForMember(dest => dest.Name, opt => opt.ConvertUsing(new TextDecrypter()))
            .ForMember(dest => dest.Email, opt => opt.ConvertUsing(new TextDecrypter()))
            .ForMember(dest => dest.Cpf, opt => opt.ConvertUsing(new TextDecrypter()))
            .ForMember(dest => dest.CellPhone, opt => opt.ConvertUsing(new TextDecrypter()));

        CreateMap<Servo, ServoDtoResult>()
            .ForMember(dest => dest.Name, opt => opt.ConvertUsing(new TextDecrypter()))
            .ForMember(dest => dest.Email, opt => opt.ConvertUsing(new TextDecrypter()))
            .ForMember(dest => dest.Cpf, opt => opt.ConvertUsing(new TextDecrypter()))
            .ForMember(dest => dest.CellPhone, opt => opt.ConvertUsing(new TextDecrypter()));

        CreateMap<ServoTemp, ServoTempDto>()
            .ForMember(dest => dest.Name, opt => opt.ConvertUsing(new TextDecrypter()))
            .ForMember(dest => dest.Email, opt => opt.ConvertUsing(new TextDecrypter()))
            .ForMember(dest => dest.Cpf, opt => opt.ConvertUsing(new TextDecrypter()))
            .ForMember(dest => dest.CellPhone, opt => opt.ConvertUsing(new TextDecrypter()));

        CreateMap<ServoTemp, ServoTempDtoResult>()
            .ForMember(dest => dest.Name, opt => opt.ConvertUsing(new TextDecrypter()))
            .ForMember(dest => dest.Email, opt => opt.ConvertUsing(new TextDecrypter()))
            .ForMember(dest => dest.Cpf, opt => opt.ConvertUsing(new TextDecrypter()))
            .ForMember(dest => dest.CellPhone, opt => opt.ConvertUsing(new TextDecrypter()));

        CreateMap<Formacao, FormacaoDto>()
            .ReverseMap();

        CreateMap<FormacoesServo, FormacoesServoDtoResult>()
            .ReverseMap();

        //CreateMap<Evento, EventoDto>()
        //    .ReverseMap();

        //CreateMap<Evento, EventoDtoResult>()
        //    .ReverseMap();

        //CreateMap<InscricoesEvento, InscricoesEventoDto>()
        //    .ReverseMap();

        //CreateMap<InscricoesEvento, InscricoesEventoDtoResult>()
        //    .ReverseMap();

        CreateMap<User, UserDto>()
            .ReverseMap();

        CreateMap<User, UserDtoAdd>()
            .ReverseMap();

        CreateMap<User, UserDtoResult>()
            .ReverseMap();

        // ======================================================
        // EVENTO
        // ======================================================

        CreateMap<Evento, EventoDtoResult>()
            .ForMember(dest => dest.Sobre, opt => opt.MapFrom(src => src.Sobre))
            .ForMember(dest => dest.LotesInscricoes, opt => opt.MapFrom(src => src.LotesInscricoes))
            .ForMember(dest => dest.Programacao, opt => opt.MapFrom(src => src.Programacao))
            .ForMember(dest => dest.Participacoes, opt => opt.MapFrom(src => src.Participacoes))
            .ForMember(dest => dest.InformacoesAdicionais, opt => opt.MapFrom(src => src.InformacoesAdicionais))
            .ReverseMap();

        CreateMap<Evento, EventoDto>()
            .ForMember(dest => dest.Sobre, opt => opt.MapFrom(src => src.Sobre))
            .ForMember(dest => dest.LotesInscricoes, opt => opt.MapFrom(src => src.LotesInscricoes))
            .ForMember(dest => dest.Programacao, opt => opt.MapFrom(src => src.Programacao))
            .ForMember(dest => dest.Participacoes, opt => opt.MapFrom(src => src.Participacoes))
            .ForMember(dest => dest.InformacoesAdicionais, opt => opt.MapFrom(src => src.InformacoesAdicionais))
            .ForMember(dest => dest.Inscricoes, opt => opt.MapFrom(src => src.Inscricoes))

             .ReverseMap();


        // ======================================================
        // LOCAL
        // ======================================================
        CreateMap<Local, LocalDto>().ReverseMap();

        // ======================================================
        // SOBRE
        // ======================================================
        CreateMap<Sobre, SobreDto>()
            .ForMember(dest => dest.Conteudo, opt => opt.MapFrom(src => src.Conteudo))
            .ReverseMap();

        // ======================================================
        // LOTE DE INSCRIÇÃO
        // ======================================================
        CreateMap<LoteInscricao, LoteDto>()
            .ForMember(dest => dest.Nome, opt => opt.MapFrom(src => src.Nome))
            .ForMember(dest => dest.Valor, opt => opt.MapFrom(src => src.Valor))
            .ForMember(dest => dest.DataInicio, opt => opt.MapFrom(src => src.DataInicio))
            .ForMember(dest => dest.DataFim, opt => opt.MapFrom(src => src.DataFim))
            .ForMember(dest => dest.Quantidade, opt => opt.Ignore()) // campo não existe na entidade, mas mantido no DTO
            .ReverseMap();

        // ======================================================
        // PROGRAMAÇÃO
        // ======================================================
        CreateMap<Programacao, ProgramacaoDto>()
            .ReverseMap();

        // ======================================================
        // INFORMAÇÕES ADICIONAIS
        // ======================================================
        CreateMap<InformacoesAdicionais, InformacaoAdicionalDto>()
            .ReverseMap();

        // ======================================================
        // PARTICIPACOES
        // ======================================================
        CreateMap<Participacao, ParticipacaoDto>()
            .ReverseMap();

        // ======================================================
        // INSCRICOES
        // ======================================================
        CreateMap<Inscricao, InscricaoDto>()
            .ForMember(dest => dest.CheckIn, opt => opt.MapFrom(src => src.CheckIn ?? false))
            .ReverseMap();

        // ======================================================
        // USUARIOS CHECKIN
        // ======================================================
        CreateMap<UsuariosCheckin, UsuarioCheckinDto>()
            .ReverseMap();

        CreateMap<UsuariosCheckin, UsuarioCheckinDtoResult>()
            .ForMember(dest => dest.Senha, opt => opt.Ignore()) 
            .ReverseMap();

        // ======================================================
        // TRANSFERENCIA SERVO
        // ======================================================
        CreateMap<TransferenciaServo, TransferenciaServoDto>()
            .ReverseMap();

        CreateMap<TransferenciaServo, TransferenciaServoDtoResult>()
            .ReverseMap();

        // ======================================================
        // EventoCampos 
        // ======================================================
        CreateMap<EventoCampos, CamposFormularioDto>()
            .ReverseMap();

        // ======================================================
        // InscricaoCampoValoresDto 
        // ======================================================
        CreateMap<InscricaoCampoValores, InscricaoCampoValoresDto>()
            .ReverseMap();

        // ======================================================
        // EventoUsuarios 
        // ======================================================
        CreateMap<EventoUsuarios, EventoUsuariosDto>()
            .ReverseMap();

        CreateMap<EventoUsuarios, EventoUsuariosDtoResult>()
            .ReverseMap();

        // ======================================================
        // Financeiro 
        // ======================================================
        CreateMap<Financeiro, FinanceiroDto>()
            .ReverseMap();

        CreateMap<Financeiro, FinanceiroDtoResult>()
            .ReverseMap();

        
    }

    public class TextDecrypter : IValueConverter<string, string>
    {
        public string Convert(string sourceMember, ResolutionContext context)
        {
            if (string.IsNullOrEmpty(sourceMember))
                return string.Empty;
            return Utils.Decrypt(sourceMember);
        }
    }
}
