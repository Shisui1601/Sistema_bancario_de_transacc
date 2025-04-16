void SubmenuCuentas(BankService banco)
{
    while (true)
    {
        Console.WriteLine(@"
----- SUBMENÚ DE CUENTAS -----
1. Ver todas las cuentas
2. Registrar nueva cuenta
3. Consultar cuenta por ID
4. Volver al menú principal
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
                Console.WriteLine("Opción inválida.");
                break;
        }
    }
}
