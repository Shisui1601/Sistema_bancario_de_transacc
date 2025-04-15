using BankSim.Models;
using System.Threading.Tasks;
using BankSim.Services;
using BankSim.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using BankApp.Models;
using BankApp.Services;
using System.Linq;
using System;


var cuentasSimuladas = FileManager.LoadAccounts(); // Cargar cuentas desde el archivo para la simulación
if (cuentasSimuladas.Count == 0) // Si no hay cuentas, crear una cuenta por defecto
{
    cuentasSimuladas.Add(new Account(1, "Cuenta de Prueba", 1000));
    FileManager.SaveAccounts(cuentasSimuladas); // Guardar la cuenta por defecto en el archivo
}
var accountsDict = new ConcurrentDictionary<int, Account>(cuentasSimuladas.ToDictionary(a => a.Id)); // Crear un diccionario concurrente a partir de la lista de cuentas



var cuentasList = FileManager.LoadAccounts(); // Cargar cuentas desde el archivo
var cuentasDict = new ConcurrentDictionary<int, Account>(cuentasList.ToDictionary(a => a.Id)); // Crear un diccionario concurrente a partir de la lista de cuentas
var bank = new BankService(cuentasDict);   // Crear instancia de BankService con el diccionario de cuentas

var transacciones = JsonStorage.LoadTransactions(); // Cargar transacciones desde el archivo
var nextTransactionId = transacciones.Any() ? transacciones.Max(t => t.Id) + 1 : 1; // ID para la siguiente transacción






while (true)
{
    Console.WriteLine(@"
    ----- MENÚ PRINCIPAL -----
    1. Cuentas
    2. Movimientos (Transferir, Depositar)
    3. Guardar 
    4. Simular transacciones con Parallel.For
    5. Simular transacciones con Task.Run
    6. Salir
    ");
    Console.Write("Selecciona una opción: ");
    var option = Console.ReadLine();

    switch (option)
    {
        case "1":
            SubmenuCuentas(bank);
            break;

        case "2":
            SubmenuOperaciones(bank);
            break;

        case "3":
            FileManager.SaveAccounts(bank.GetAllAccounts());
            JsonStorage.SaveTransactions(transacciones);
            Console.WriteLine("Cuentas guardadas.");
            break;


        case "4":
            Console.Write("Número de transacciones: ");
            //proximamente...
            break;

        case "5":
            Console.Write("Número de transacciones: ");
            //proximamente...
            break;

        case "6":
            Console.WriteLine("Saliendo...");
            FileManager.SaveAccounts(bank.GetAllAccounts());
            JsonStorage.SaveTransactions(transacciones);
            Console.WriteLine("Cuentas guardadas.");
            return;

        default:
            Console.WriteLine("Opción inválida");
            break;
    }
}


void SubmenuCuentas(BankService banco)
{
    while (true)
    {
        Console.WriteLine(@"
        ----- SUBMENÚ CUENTAS -----
        1. Ver todas las cuentas
        2. Registrar nueva cuenta
        3. Consultar cuenta por ID
        4. Volver al menú principal
        ");
        Console.Write("Selecciona una opción: ");
        var option = Console.ReadLine();

        switch (option)
        {
            case "1":
                //proximamente...
                break;

            case "2":
                //proximamente...
                break;

            case "3":
               //proximamente...
                break;

            case "4":
                return;

            default:
                Console.WriteLine("Opción inválida");
                break;
        }
    }
}

void SubmenuOperaciones(BankService banco)
{
    while (true)
    {
        Console.WriteLine(@"
----- SUBMENÚ DE Movimientos -----
1. Transferencia
2. Depósito
3. Ver todas las transacciones
4. Buscar transacción por ID
5. Volver al menú principal
");

        Console.Write("Selecciona una opción: ");
        var opcion = Console.ReadLine();

        switch (opcion)
        {
            case "1":
                Console.Write("Cuenta origen: ");
                //proximamente...
                break;

            case "2":
                Console.Write("Cuenta a depositar: ");
                //proximamente...
                break;

            case "3":
                //proximamente...
                break;

            case "4":
                Console.Write("ID de transacción: ");
               //proximamente...
                break;

            case "5":
                return;

            default:
                Console.WriteLine("Opción inválida.");
                break;
        }
    }
}
