using System;
using RccManager.Domain.Responses;

namespace RccManager.Domain.Interfaces.Services
{
    public interface IWhatsAppService
    {
        Task EnviarTexto(InscricaoMQResponse model);
        Task EnviarQrCode(InscricaoMQResponse model);
    }
}

