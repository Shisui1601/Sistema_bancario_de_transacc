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


var cuentasSimuladas = FileManager.LoadAccounts(); // Cargar cuentas desde el archivo de simulación
if (cuentasSimuladas.Count < 100) // Verificar si hay menos de 100 cuentas
{
    Console.WriteLine("Generando cuentas simuladas...");
    int nextId = cuentasSimuladas.Any() ? cuentasSimuladas.Max(c => c.Id) + 1 : 1; // Obtener el próximo ID disponible
    for (int i = cuentasSimuladas.Count; i < 100; i++)
    {
        cuentasSimuladas.Add(new Account(nextId++, $"Cuenta Simulada {i + 1}", new Random().Next(1000, 10000))); // Generar cuentas con saldo aleatorio
    }
    FileManager.SaveAccountsSimulacion(cuentasSimuladas); // Guardar las cuentas simuladas
    Console.WriteLine("Cuentas simuladas generadas.");
}
var cuentasDictSimuladas = new ConcurrentDictionary<int, Account>(cuentasSimuladas.ToDictionary(a => a.Id)); // Crear un diccionario concurrente a partir de la lista de cuentas simuladas
var bankSimulacion = new BankService(cuentasDictSimuladas); // Crear instancia de BankService con el diccionario de cuentas simuladas



var cuentasList = JsonStorage.LoadAccounts(); // Cargar cuentas desde el archivo
var cuentasDict = new ConcurrentDictionary<int, Account>(cuentasList.ToDictionary(a => a.Id)); // Crear un diccionario concurrente a partir de la lista de cuentas
var bank = new BankService(cuentasDict);   // Crear instancia de BankService con el diccionario de cuentas

var transacciones = JsonStorage.LoadTransactions(); // Cargar transacciones desde el archivo
var nextTransactionId = transacciones.Any() ? transacciones.Max(t => t.Id) + 1 : 1; // ID para la siguiente transacción


async Task EjecutarConParallel(BankService banco, int numTransacciones, int[] procesadores, TransactionSummary resumen)
{
    double tiempoSecuencial = 0;

    foreach (var numProcesadores in procesadores)
    {
        Console.WriteLine($"\n[Parallel.ForEachAsync] Ejecutando {numTransacciones} transacciones con {numProcesadores} procesador(es)...");

        var cuentas = banco.GetAllAccounts().ToList();
        var rnd = new Random();
        var errores = 0;

        var stopwatch = Stopwatch.StartNew();

        await Parallel.ForEachAsync(
            Enumerable.Range(0, numTransacciones),
            new ParallelOptions { MaxDegreeOfParallelism = numProcesadores },
            async (i, _) =>
            {
                int fromIndex, toIndex;
                lock (rnd)
                {
                    do
                    {
                        fromIndex = rnd.Next(cuentas.Count);
                        toIndex = rnd.Next(cuentas.Count);
                    } while (fromIndex == toIndex);
                }

                int fromId = cuentas[fromIndex].Id;
                int toId = cuentas[toIndex].Id;
                decimal amount = rnd.Next(1, 100);

                await Task.Delay(50); // simulación no bloqueante

                var ok = banco.Transfer(fromId, toId, amount);
                if (!ok) Interlocked.Increment(ref errores);
            });

        stopwatch.Stop();

        var tiempoDuracion = stopwatch.Elapsed.TotalMilliseconds;

        if (numProcesadores == 1)
            tiempoSecuencial = tiempoDuracion;

        double speedup = tiempoSecuencial > 0 ? tiempoSecuencial / tiempoDuracion : 1;
        double eficiencia = speedup / numProcesadores;

        resumen.Simulaciones.Add(new SimulacionMetricas
        {
            Procesadores = numProcesadores,
            TiempoDuracionMs = tiempoDuracion,
            Speedup = speedup,
            Eficiencia = eficiencia
        });

        Console.WriteLine($"Completado en {tiempoDuracion:F2}ms.");
        Console.WriteLine($"Speedup: {speedup:F2}");
        Console.WriteLine($"Eficiencia: {eficiencia:P2}");
    }
}

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
            FileManager.SaveAccountsSimulacion(bankSimulacion.GetAllAccounts());
            JsonStorage.SaveAccounts(bank.GetAllAccounts());
            JsonStorage.SaveTransactionSummary(transacciones);
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
            FileManager.SaveAccountsSimulacion(bankSimulacion.GetAllAccounts());
            JsonStorage.SaveAccounts(bank.GetAllAccounts());
            JsonStorage.SaveTransactionSummary(transacciones);
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
        4. Eliminar cuenta
        5. Volver al menú principal
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
                
                Console.Write("Ingresa el ID de la cuenta: ");
                int buscarId = int.Parse(Console.ReadLine()!);
                var cuenta = banco.GetAllAccounts().FirstOrDefault(c => c.Id == buscarId);
                if (cuenta != null)
                    Console.WriteLine($"[{cuenta.Id}] {cuenta.Owner} - ${cuenta.Balance}");
                else
                    Console.WriteLine("Cuenta no encontrada.");

                break;
            
            case "4":
                Console.Write("Ingresa el ID de la cuenta a eliminar: ");
                int eliminarId = int.Parse(Console.ReadLine()!);
                var cuentaEliminar = banco.GetAllAccounts().FirstOrDefault(c => c.Id == eliminarId);
                if (cuentaEliminar != null)
                {
                    if (banco.RemoveAccount(eliminarId))
                    {
                        JsonStorage.SaveAccounts(banco.GetAllAccounts());
                        FileManager.SaveAccountsSimulacion(banco.GetAllAccounts());
                        Console.WriteLine($"Cuenta con ID {eliminarId} eliminada.");
                    }
                    else
                    {
                        Console.WriteLine("No se pudo eliminar la cuenta.");
                    }
                }
                else
                {
                    Console.WriteLine("Cuenta no encontrada.");
                }
                break;

            case "5":
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
