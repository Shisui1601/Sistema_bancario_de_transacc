using System.Collections.Generic;
using System.IO;
using BankSim.Models;
using System.Text.Json;

namespace BankSim.Utils
{
    public static class FileManager
    {
        private const string FilePath = "accountsSimulacion.json";

        public static void SaveAccountsSimulacion(IEnumerable<Account> accounts)
        {
            var json = JsonSerializer.Serialize(accounts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }

        public static List<Account> LoadAccounts()
        {
            if (!File.Exists(FilePath))
                return new List<Account>();

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<Account>>(json) ?? new List<Account>();
        }
    }
}
