```csharp
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace SistemaBiblioteca
{
    // 1. CLASE MODELO (Entidad de Datos)
    public class Prestamo
    {
        public int Id { get; set; }
        public string NombrePersona { get; set; }
        public string NombreLibro { get; set; }
        public DateTime FechaSalida { get; set; }
        public DateTime FechaDevolucion { get; set; }
        public decimal MontoPagado { get; set; }
        public bool EntregoATiempo { get; set; }
    }

    // 2. CLASE REPOSITORIO (Manejo de Base de Datos SQLite)
    public class RepositorioBiblioteca
    {
        private readonly string connectionString = "Data Source=biblioteca.db";

        public RepositorioBiblioteca()
        {
            InicializarBaseDeDatos();
        }

        private void InicializarBaseDeDatos()
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Prestamos (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        NombrePersona TEXT NOT NULL,
                        NombreLibro TEXT NOT NULL,
                        FechaSalida TEXT NOT NULL,
                        FechaDevolucion TEXT NOT NULL,
                        MontoPagado REAL NOT NULL,
                        EntregoATiempo INTEGER NOT NULL
                    );";
                command.ExecuteNonQuery();
            }
        }

        public void RegistrarPrestamo(Prestamo prestamo)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Prestamos (NombrePersona, NombreLibro, FechaSalida, FechaDevolucion, MontoPagado, EntregoATiempo)
                    VALUES ($persona, $libro, $salida, $devolucion, $monto, $aTiempo);";

                command.Parameters.AddWithValue("$persona", prestamo.NombrePersona);
                command.Parameters.AddWithValue("$libro", prestamo.NombreLibro);
                command.Parameters.AddWithValue("$salida", prestamo.FechaSalida.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("$devolucion", prestamo.FechaDevolucion.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("$monto", prestamo.MontoPagado);
                command.Parameters.AddWithValue("$aTiempo", prestamo.EntregoATiempo ? 1 : 0);

                command.ExecuteNonQuery();
            }
        }

        public List<Prestamo> ObtenerTodosLosPrestamos()
        {
            var lista = new List<Prestamo>();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, NombrePersona, NombreLibro, FechaSalida, FechaDevolucion, MontoPagado, EntregoATiempo FROM Prestamos;";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Prestamo
                        {
                            Id = reader.GetInt32(0),
                            NombrePersona = reader.GetString(1),
                            NombreLibro = reader.GetString(2),
                            FechaSalida = DateTime.Parse(reader.GetString(3)),
                            FechaDevolucion = DateTime.Parse(reader.GetString(4)),
                            MontoPagado = Convert.ToDecimal(reader.GetDouble(5)),
                            EntregoATiempo = reader.GetInt32(6) == 1
                        });
                    }
                }
            }
            return lista;
        }

        public List<Prestamo> ObtenerHistorialPorPersona(string nombrePersona)
        {
            var lista = new List<Prestamo>();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, NombrePersona, NombreLibro, FechaSalida, FechaDevolucion, MontoPagado, EntregoATiempo FROM Prestamos WHERE NombrePersona LIKE $persona;";
                command.Parameters.AddWithValue("$persona", $"%{nombrePersona}%");

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Prestamo
                        {
                            Id = reader.GetInt32(0),
                            NombrePersona = reader.GetString(1),
                            NombreLibro = reader.GetString(2),
                            FechaSalida = DateTime.Parse(reader.GetString(3)),
                            FechaDevolucion = DateTime.Parse(reader.GetString(4)),
                            MontoPagado = Convert.ToDecimal(reader.GetDouble(5)),
                            EntregoATiempo = reader.GetInt32(6) == 1
                        });
                    }
                }
            }
            return lista;
        }
    }

    // 3. CLASE MENÚ DE INTERACCIÓN
    public class MenuBiblioteca
    {
        private readonly RepositorioBiblioteca repo = new RepositorioBiblioteca();

        public void MostrarMenu()
        {
            bool salir = false;
            while (!salir)
            {
                Console.Clear();
                Console.WriteLine("========================================");
                Console.WriteLine("    SISTEMA DE PRESTAMOS DE BIBLIOTECA   ");
                Console.WriteLine("========================================");
                Console.WriteLine("1. Registrar nuevo préstamo");
                Console.WriteLine("2. Ver todos los préstamos");
                Console.WriteLine("3. Consultar récord personal por cliente");
                Console.WriteLine("4. Salir");
                Console.WriteLine("========================================");
                Console.Write("Seleccione una opción: ");

                string opcion = Console.ReadLine();

                switch (opcion)
                {
                    case "1":
                        RegistrarPrestamo();
                        break;
                    case "2":
                        MostrarTodos();
                        break;
                    case "3":
                        ConsultarRecordPersonal();
                        break;
                    case "4":
                        salir = true;
                        Console.WriteLine("\n¡Gracias por usar el sistema!");
                        break;
                    default:
                        Console.WriteLine("\nOpción no válida. Presione cualquier tecla...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        private void RegistrarPrestamo()
        {
            Console.Clear();
