using RccManager.Domain.Entities;

public class Wallet : BaseEntity
{
    public Guid OrganizadorId { get; set; }

    public decimal SaldoDisponivel { get; set; }

    public decimal SaldoPendente { get; set; }

    public decimal SaldoRepassado { get; set; }

    public virtual Evento Organizador { get; set; }

    public virtual ICollection<WalletMovimento> Movimentos { get; set; }
}