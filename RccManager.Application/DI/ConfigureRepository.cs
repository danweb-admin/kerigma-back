using Microsoft.Extensions.DependencyInjection;
using RccManager.Domain.Interfaces.Repositories;
using RccManager.Infra.Repositories;

namespace RccManager.Application.DI
{
    public class ConfigureRepository
    {
        public static void ConfigureDependenciesRepository(IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
            services.AddScoped<IDecanatoSetorRepository, DecanatoSetorRepository>();
            services.AddScoped<IParoquiaCapelaRepository, ParoquiaCapelaRepsoitory>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IGrupoOracaoRepository, GrupoOracaoRepository>();
            services.AddScoped<IServoRepository, ServoRepository>();
            services.AddScoped<IFormacaoRepository, FormacaoRepository>();
            services.AddScoped<IFormacoesServoRepository, FormacoesServoRepository>();
            services.AddScoped<IServoTempRepository, ServoTempRepository>();
            services.AddScoped<IHistoryRepository, HistoryRepository>();
            services.AddScoped<IEventoRepository, EventoRepository>();
            services.AddScoped<IInscricoesEventoRepository, InscricoesEventoRepository>();
            services.AddScoped<IInscricaoRepository, InscricaoRepository>();
            services.AddScoped<IPagamentoAsaasRepository, PagamentoAsaasRepository>();
            services.AddScoped<IUsuarioCheckinRepository, UsuarioCheckinRepository>();
            services.AddScoped<ITransferenciaServoRepository, TranferenciaServoRepository>();
            services.AddScoped<IEventoUsuariosRepository, EventoUsuariosRepository>();
            services.AddScoped<IFinanceiroRepository, FinanceiroRepository>();



        }
    }
}
