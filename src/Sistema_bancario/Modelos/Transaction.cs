namespace BankApp.Models;

public class Transaction
{
    public int Id { get; set; }
    public string Tipo { get; set; } = "";
    public int? FromAccountId { get; set; }
    public int? ToAccountId { get; set; }
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;

    public override string ToString()
    {
        return $"[{Id}] {Tipo} - {Monto:C} | De: {FromAccountId} → A: {ToAccountId} ({Fecha})";
    }
}
