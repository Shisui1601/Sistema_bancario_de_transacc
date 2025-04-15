using System.Text.Json;
using BankApp.Models;
using BankSim.Models;

namespace BankApp.Services;

public static class JsonStorage
{
    private const string AccountsFile = "data/accounts.json";
    private const string TransactionsFile = "data/transactions.json";

    public static void SaveAccounts(IEnumerable<Account> cuentas)
    {
        Directory.CreateDirectory("data");
        var json = JsonSerializer.Serialize(cuentas, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(AccountsFile, json);
    }

    public static List<Account> LoadAccounts()
    {
        if (!File.Exists(AccountsFile)) return new();
        var json = File.ReadAllText(AccountsFile);
        return JsonSerializer.Deserialize<List<Account>>(json) ?? new();
    }

    public static void SaveTransactions(IEnumerable<Transaction> transacciones)
    {
        Directory.CreateDirectory("data");
        var json = JsonSerializer.Serialize(transacciones, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(TransactionsFile, json);
    }

    public static List<Transaction> LoadTransactions()
    {
        if (!File.Exists(TransactionsFile)) return new();
        var json = File.ReadAllText(TransactionsFile);
        return JsonSerializer.Deserialize<List<Transaction>>(json) ?? new();
    }

    public static void SaveTransactionSummary(List<Transaction> transacciones)
    {
        Directory.CreateDirectory("metrics");

        var resumen = new TransactionSummary
        {
            Fecha = DateTime.Today,
            TotalMovimientos = transacciones.Count,
            TotalTransferencias = transacciones.Count(t => t.Tipo == "Transferencia"),
            TotalDepositos = transacciones.Count(t => t.Tipo == "Depósito"),
            MontoTotalTransferido = transacciones.Where(t => t.Tipo == "Transferencia").Sum(t => t.Monto),
            MontoTotalDepositado = transacciones.Where(t => t.Tipo == "Depósito").Sum(t => t.Monto)
        };

        var json = JsonSerializer.Serialize(resumen, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("metrics/resumen-transacciones.json", json);

        Console.WriteLine("Resumen de transacciones guardado en metrics/resumen-transacciones.json");
    }
}


