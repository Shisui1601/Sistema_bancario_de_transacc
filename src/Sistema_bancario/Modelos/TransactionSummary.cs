namespace BankApp.Models;

public class TransactionSummary
{
    public DateTime Fecha { get; set; } = DateTime.Today;
    public int TotalMovimientos { get; set; }
    public int TotalTransferencias { get; set; }
    public int TotalDepositos { get; set; }
    public decimal MontoTotalTransferido { get; set; }
    public decimal MontoTotalDepositado { get; set; }
    public List<SimulacionMetricas> Simulaciones { get; set; } = new();
}

public class SimulacionMetricas
{
    public int Procesadores { get; set; }
    public double TiempoDuracionMs { get; set; }
    public double Speedup { get; set; }
    public double Eficiencia { get; set; }
}