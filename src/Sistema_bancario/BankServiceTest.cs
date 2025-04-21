using System;
using System.Collections.Concurrent;
using BankSim.Services;
using BankSim.Models;

namespace BankSystem.Test
{
    public class BankServiceTest
    {
        public static void Main()
        {
            Console.WriteLine("Iniciando pruebas de BankService...");

            // Crear un diccionario concurrente con cuentas iniciales
            var accounts = new ConcurrentDictionary<int, Account>();
            accounts.TryAdd(1, new Account(1, "Cuenta 1", 1000));
            accounts.TryAdd(2, new Account(2, "Cuenta 2", 500));

            // Pasar el diccionario al constructor de BankService
            var bank = new BankService(accounts);

            // Realizar pruebas
            bool result1 = bank.Transfer(1, 2, 200);
            Console.WriteLine("Prueba 1 - Transferencia válida: " + (result1 ? "OK" : "FALLÓ"));

            bool result2 = bank.Transfer(1, 2, 10000);
            Console.WriteLine("Prueba 2 - Transferencia insuficiente: " + (!result2 ? "OK" : "FALLÓ"));

            decimal saldo1 = bank.GetBalance(1);
            decimal saldo2 = bank.GetBalance(2);
            Console.WriteLine($"Saldo final cuenta 1: {saldo1}");
            Console.WriteLine($"Saldo final cuenta 2: {saldo2}");

            Console.WriteLine("Pruebas finalizadas.");
        }
    }
}