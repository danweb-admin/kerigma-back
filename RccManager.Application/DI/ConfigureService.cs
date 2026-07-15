
using Microsoft.Extensions.DependencyInjection;
using RccManager.Domain.Interfaces.Services;
using RccManager.Domain.Services;
using RccManager.Infra.Context;
using RccManager.Service.MQ;
using RccManager.Service.Services;

namespace RccManager.Application.DI
{
    public class ConfigureService
    {
        public static void ConfigureDependenciesService(IServiceCollection services)
        {
            services.AddScoped<IDecanatoSetorService, DecanatoSetorService>();
            services.AddScoped<IParoquiaCapelaService, ParoquiaCapelaService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IGrupoOracaoService, GrupoOracaoService>();
            services.AddScoped<ICachingService, CachingService>();
            services.AddScoped<IServoService, ServoService>();
            services.AddScoped<IFormacaoService, FormacaoService>();
            services.AddScoped<IFormacoesServoService, FormacoesServoService>();
            services.AddSingleton<IMD5Service, MD5Service>();
            services.AddScoped<IServoTempService, ServoTempService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEventoService, EventoService>();
            services.AddScoped<IInscricoesEventoService, InscricoesEventoService>();
            services.AddScoped<IPagSeguroService, PagSeguroService>();
            services.AddScoped<IUsuarioCheckinService, UsuarioCheckinService>();
            services.AddScoped<ITransferenciaServoService, TransferenciaServoService>();
            services.AddScoped<IEventoUsuariosService, EventoUsuariosService>();
            services.AddScoped<IFinanceiroService, FinanceiroService>();
            services.AddScoped<IWalletService, WalletService>();
            services.AddScoped<IWalletMovimentoService, WalletMovimentoService>();


            services.AddScoped<AppDbContext>();
            services.AddSingleton<RabbitMQConnection>();


        }
    }
}
