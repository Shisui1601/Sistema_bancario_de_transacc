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
                foreach (var acc in banco.GetAllAccounts())

                    Console.WriteLine($"[{acc.Id}] {acc.Owner} - ${acc.Balance}");

                break;

            case "2":
                int nuevoId;
                while (true)
                {
                    Console.Write("ID de cuenta (dejar vacío para generar automaticamente): ");
                    string inputId = Console.ReadLine()!;
                    if (string.IsNullOrWhiteSpace(inputId))
                    {
                        var cuentasExistentes = banco.GetAllAccounts().ToList();
                        nuevoId = cuentasExistentes.Any() ? cuentasExistentes.Max(c => c.Id) + 1 : 1;
                        break;
                    }
                    else if (int.TryParse(inputId, out int manualId))
                    {
                        bool existe = banco.GetAllAccounts().Any(c => c.Id == manualId);
                        if (existe)
                        {
                            Console.WriteLine("Ya existe una cuenta con ese ID. Introduzca otro.");
                        }
                        else
                        {
                            nuevoId = manualId;
                            break;
                        }
                    }
                    else
                    {
                        Console.WriteLine("ID inválido. Debe ser un número entero.");
                    }
                }
                Console.Write("Nombre del dueño: ");
                string owner = Console.ReadLine()!;
                Console.Write("Saldo inicial: ");
                decimal saldo = decimal.Parse(Console.ReadLine()!);
                banco.AddAccount(new Account(nuevoId, owner, saldo));
                JsonStorage.SaveAccounts(banco.GetAllAccounts()); // Guardar cuentas después de agregar una nueva
                JsonStorage.SaveTransactions(transacciones); // Guardar transacciones después de agregar una nueva cuenta
                var nuevaCuenta = new Account(nuevoId, owner, saldo);
                var nuevasCuentas = new List<Account>();
                // Verificar si la cuenta ya existe en la lista
                nuevasCuentas.Add(nuevaCuenta);
                Console.WriteLine($"Cuenta registrada con ID: {nuevoId}");
                Console.WriteLine("\n--- Resumen de cuentas nuevas ---");
                foreach (var cuentaNueva in nuevasCuentas)
                    Console.WriteLine($"[{cuentaNueva.Id}] {cuentaNueva.Owner} - ${cuentaNueva.Balance}");
                break;

            case "3":
                Console.Write("Ingresa el ID de la cuenta: ");
                int buscarId = int.Parse(Console.ReadLine()!);
                var cuenta = banco.GetAllAccounts().FirstOrDefault(c => c.Id == buscarId);
                if (cuenta != null)
                    Console.WriteLine($"[{cuenta.Id}] {cuenta.Owner} - ${cuenta.Balance}");
                else
                    Console.WriteLine("Cuenta no encontrada.");
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
                int cuentaId = int.Parse(Console.ReadLine()!);
                Console.Write("Monto a depositar: ");
                decimal deposito = decimal.Parse(Console.ReadLine()!);

                var cuenta = banco.GetAllAccounts().FirstOrDefault(c => c.Id == cuentaId);
                if (cuenta != null)
                {
                    cuenta.Balance += deposito;
                    transacciones.Add(new Transaction
                    {
                        Id = nextTransactionId++,
                        Tipo = "Depósito",
                        FromAccountId = null,
                        ToAccountId = cuentaId,
                        Monto = deposito
                    });

                    JsonStorage.SaveAccounts(banco.GetAllAccounts()); // Guardar cuentas después del depósito
                    JsonStorage.SaveTransactions(transacciones); // Guardar transacciones después del depósito
                    JsonStorage.SaveTransactionSummary(transacciones); // Guardar resumen de transacciones

                    Console.WriteLine($"Depósito de {deposito:C} realizado en la cuenta {cuentaId}.");
                    Console.WriteLine("Depósito realizado.");
                }
                else
                {
                    Console.WriteLine("Cuenta no encontrada.");
                }
                break;

            case "3":
                  if (transacciones.Count == 0)
                {
                    Console.WriteLine("📭 No hay transacciones registradas.");
                }
                else
                {
                    foreach (var t in transacciones)
                        Console.WriteLine(t);
                }
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
