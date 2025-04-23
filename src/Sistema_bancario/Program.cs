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
using System.Diagnostics;
using BankSystem.Test;

BankServiceTest.Main();


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



async Task EjecutarConTasks(BankService banco, int numTransacciones, int[] procesadores, TransactionSummary resumen)
{
    double tiempoSecuencial = 0;

    foreach (var numProcesadores in procesadores)
    {
        Console.WriteLine($"\n[Task.Run Async] Ejecutando {numTransacciones} transacciones con {numProcesadores} procesador(es)...");

        var cuentas = banco.GetAllAccounts().ToList();
        var errores = 0;
        var rnd = new Random();

        using var semaphore = new SemaphoreSlim(numProcesadores);
        var tasks = new List<Task>();
        var stopwatch = Stopwatch.StartNew();

        for (int i = 0; i < numTransacciones; i++)
        {
            await semaphore.WaitAsync();

            var task = Task.Run(async () =>
            {
                try
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
                }
                finally
                {
                    semaphore.Release();
                }
            });

            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

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
            int n1 = int.Parse(Console.ReadLine()!);
            var procesadores1 = new[] { 1, 2, 4, 6, 8, 10 };
            var resumen = new TransactionSummary();
            await EjecutarConParallel(bankSimulacion, n1, procesadores1, resumen);
            // JsonStorage.SaveTransactionSummary(resumen);
            Console.WriteLine("Resumen guardado.");
            break;

        case "5":
            Console.Write("Número de transacciones: ");
            int n2 = int.Parse(Console.ReadLine()!);
            var procesadores2 = new[] { 1, 2, 4, 6, 8, 10 };
            var resu = new TransactionSummary();
            await EjecutarConTasks(bankSimulacion, n2, procesadores2, resu);
            //JsonStorage.SaveTransactionSummary(resu);
            Console.WriteLine("Resumen guardado.");
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
----- SUBMENÚ DE CUENTAS -----
1. Ver todas las cuentas
2. Registrar nueva cuenta
3. Consultar cuenta por ID
4. Eliminar cuenta
5. Volver al menú principal
");

        Console.Write("Selecciona una opción: ");
        var opcion = Console.ReadLine();

        switch (opcion)
        {
            case "1":
                foreach (var acc in banco.GetAllAccounts())
                    Console.WriteLine($"[{acc.Id}] {acc.Owner} - ${acc.Balance}");
                break;

            case "2":
                int nuevoId;
                while (true)
                {
                    Console.Write("ID de cuenta (dejar vacío para generar automáticamente): ");
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

                string owner;
                while (true)
                {
                    Console.Write("Nombre del dueño: ");
                    owner = Console.ReadLine()!;
                    if (!string.IsNullOrWhiteSpace(owner) && owner.All(char.IsLetter))
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("El nombre del dueño solo debe contener letras.");
                    }
                }

                decimal saldo;
                while (true)
                {
                    Console.Write("Saldo inicial: ");
                    string inputSaldo = Console.ReadLine()!;
                    if (decimal.TryParse(inputSaldo, out saldo) && saldo >= 0)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Saldo inválido. Debe ser un número positivo.");
                    }
                }

                banco.AddAccount(new Account(nuevoId, owner, saldo));
                JsonStorage.SaveAccounts(banco.GetAllAccounts()); // Guardar cuentas después de agregar una nueva
                FileManager.SaveAccountsSimulacion(banco.GetAllAccounts()); // Guardar cuentas simuladas
                JsonStorage.SaveTransactions(transacciones); // Guardar transacciones después de agregar una nueva cuenta
                var nuevaCuenta = new Account(nuevoId, owner, saldo);
                var nuevasCuentas = new List<Account>();
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
                Console.WriteLine("Opción inválida.");
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
                int from, to;
                decimal monto;

                while (true)
                {
                    Console.Write("Cuenta origen: ");
                    string inputFrom = Console.ReadLine()!;
                    if (int.TryParse(inputFrom, out from))
                        break;
                    else
                        Console.WriteLine("ID inválido. Debe ser un número entero.");
                }

                while (true)
                {
                    Console.Write("Cuenta destino: ");
                    string inputTo = Console.ReadLine()!;
                    if (int.TryParse(inputTo, out to))
                        break;
                    else
                        Console.WriteLine("ID inválido. Debe ser un número entero.");
                }

                while (true)
                {
                    Console.Write("Monto: ");
                    string inputMonto = Console.ReadLine()!;
                    if (decimal.TryParse(inputMonto, out monto) && monto > 0)
                        break;
                    else
                        Console.WriteLine("Monto inválido. Debe ser un número positivo.");
                }

                if (banco.Transfer(from, to, monto))
                {
                    transacciones.Add(new Transaction
                    {
                        Id = nextTransactionId++,
                        Tipo = "Transferencia",
                        FromAccountId = from,
                        ToAccountId = to,
                        Monto = monto
                    });

                    JsonStorage.SaveAccounts(banco.GetAllAccounts());
                    FileManager.SaveAccountsSimulacion(banco.GetAllAccounts());
                    JsonStorage.SaveTransactions(transacciones);
                    JsonStorage.SaveTransactionSummary(transacciones);

                    Console.WriteLine($"Transferencia de {monto:C} de la cuenta {from} a la cuenta {to}.");
                    Console.WriteLine("Transferencia realizada.");
                }
                else
                {
                    Console.WriteLine("Error en la transferencia.");
                }
                break;

            case "2": 
                int cuentaId;
                decimal deposito;

                while (true)
                {
                    Console.Write("Cuenta a depositar: ");
                    string inputCuentaId = Console.ReadLine()!;
                    if (int.TryParse(inputCuentaId, out cuentaId))
                        break;
                    else
                        Console.WriteLine("ID inválido. Debe ser un número entero.");
                }

                while (true)
                {
                    Console.Write("Monto a depositar: ");
                    string inputDeposito = Console.ReadLine()!;
                    if (decimal.TryParse(inputDeposito, out deposito) && deposito > 0)
                        break;
                    else
                        Console.WriteLine("Monto inválido. Debe ser un número positivo.");
                }

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

                    JsonStorage.SaveAccounts(banco.GetAllAccounts());
                    FileManager.SaveAccountsSimulacion(banco.GetAllAccounts());
                    JsonStorage.SaveTransactions(transacciones);
                    JsonStorage.SaveTransactionSummary(transacciones);

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
                    Console.WriteLine("No hay transacciones registradas.");
                }
                else
                {
                    foreach (var t in transacciones)
                        Console.WriteLine(t);
                }
                break;

            case "4": 
                Console.Write("ID de transacción: ");
                int tId = int.Parse(Console.ReadLine()!);
                var trans = transacciones.FirstOrDefault(t => t.Id == tId);
                if (trans != null)
                    Console.WriteLine(trans);
                else
                    Console.WriteLine("Transacción no encontrada.");
                break;

            case "5": 
                return;

            default:
                Console.WriteLine("Opción inválida.");
                break;
        }
    }
}
