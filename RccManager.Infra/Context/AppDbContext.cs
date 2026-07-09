using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RccManager.Domain.Entities;
using RccManager.Infra.Mappings;

namespace RccManager.Infra.Context;

public class AppDbContext : DbContext
{
    public DbSet<DecanatoSetor> Decanatos { get; set; }
    public DbSet<ParoquiaCapela> ParoquiasCapelas { get; set; }
    public DbSet<GrupoOracao> GrupoOracoes { get; set; }
    public DbSet<Servo> Servos { get; set; }
    public DbSet<ServoTemp> ServosTemp { get; set; }
    public DbSet<Formacao> Formacoes { get; set; }
    public DbSet<FormacoesServo> FormacoesServos { get; set; }
    public DbSet<History> Histories { get; set; }
    public DbSet<Evento> Eventos { get; set; }
    public DbSet<Sobre> Sobre { get; set; }
    public DbSet<Local> Locais { get; set; }
    public DbSet<InformacoesAdicionais> InformacoesAdicionais { get; set; }
    public DbSet<Participacao> Participacoes { get; set; }
    public DbSet<Programacao> Programacoes { get; set; }
    public DbSet<Inscricao> Inscricoes { get; set; }
    public DbSet<LoteInscricao> LotesInscricao { get; set; }
    public DbSet<PagamentosAsaas> PagamentosAsaas { get; set; }
    public DbSet<UsuariosCheckin> UsuariosCheckin { get; set; }
    public DbSet<TransferenciaServo> TransferenciaServos { get; set; }
    public DbSet<EventoCampos> EventoCampos { get; set; }
    public DbSet<InscricaoCampoValores> InscricaoCampoValores { get; set; }
    public DbSet<EventoUsuarios> EventoUsuarios { get; set; }
    public DbSet<Financeiro> Financeiros { get; set; }



    public DbSet<User> Users { get; set; }


    public AppDbContext()
    {

    }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new DecanatoSetorMap());
        modelBuilder.ApplyConfiguration(new ParoquiaCapelaMap());
        modelBuilder.ApplyConfiguration(new GrupoOracaoMap());
        modelBuilder.ApplyConfiguration(new ServoMap());
        modelBuilder.ApplyConfiguration(new ServoTempMap());
        modelBuilder.ApplyConfiguration(new FormacaoMap());
        modelBuilder.ApplyConfiguration(new FormacoesServoMap());
        modelBuilder.ApplyConfiguration(new UserMap());
        modelBuilder.ApplyConfiguration(new HistoryMap());
        modelBuilder.ApplyConfiguration(new EventoMap());
        modelBuilder.ApplyConfiguration(new InscricoesEventoMap());
        modelBuilder.ApplyConfiguration(new InscricaoMap());
        modelBuilder.ApplyConfiguration(new UsuariosCheckinMap());
        modelBuilder.ApplyConfiguration(new TransferenciaServoMap());
        modelBuilder.ApplyConfiguration(new EventoCamposMap());
        modelBuilder.ApplyConfiguration(new EventoUsuariosMap());
        modelBuilder.ApplyConfiguration(new FinanceiroMap());


    }
}
