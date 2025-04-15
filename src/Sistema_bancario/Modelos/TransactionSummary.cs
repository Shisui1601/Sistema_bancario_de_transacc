namespace BankApp.Models;

public class TransactionSummary
{
    public DateTime Fecha { get; set; } = DateTime.Today;
    public int TotalMovimientos { get; set; }
    public int TotalTransferencias { get; set; }
    public int TotalDepositos { get; set; }
    public decimal MontoTotalTransferido { get; set; }
    public decimal MontoTotalDepositado { get; set; }
}
