using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using RccManager.Domain.Dtos.Evento;
using RccManager.Domain.Dtos.Formacao;
using RccManager.Domain.Responses;

namespace RccManager.Domain.Interfaces.Services
{
	public interface IEventoService
	{
        Task<IEnumerable<EventoDtoResult>> GetAll(Guid userId);
        Task<IEnumerable<EventoDtoResult>> GetEventsByEmail(string email);
        Task<IEnumerable<EventoDtoResult>> GetAllHome();
        Task<EventoDto> GetSlug(string slug);
        Task<EventoDto> GetById(Guid id);
        Task<IEnumerable<CamposFormularioDto>> GetCamposByEvento(Guid eventoId);
        Task<HttpResponse> Create(EventoDto dto, Guid userId);
        Task<HttpResponse> Update(EventoDto dto, Guid id);
        Task<decimal> LoteInscricao(Guid id);
        Task<InscricaoDto> Inscricao(InscricaoDto inscricao);
        Task<InscricaoDto> VerificarCPF(string cpf, Guid eventoId);
        Task<bool> VerificaLimiteParticipante(Guid eventoId);
        Task GerarQrCodePNG(Guid eventoId);
        Task<ValidationResult> ReenvioComprovante(string codigoInscricao, string email);
        Task<ValidationResult> IsentarInscricao(string codigoInscricao);
        Task<ValidationResult> RemoverInscricao(string codigoInscricao);
        Task<string> VerificaStatus(string codigoInscricao);
        Task<IEnumerable<InscricaoDto>> GetAllInscricoesByEvento(Guid eventoId);
        Task<HttpResponse> FazerCheckin(string codigoInscricao);
        Task<ValidationResult> EventosWebhook(string response);
        Task<DataTable> ExportarInscricoes(Guid eventoId);
        Task VerificaInscricoesPendentes();
        byte[] GerarExcel(DataTable DataTable);
    }
}

