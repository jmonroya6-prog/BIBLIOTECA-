using System.Reflection;
using Microsoft.Data.Sqlite;

namespace PrestamosBiblioteca.Persistencia;

/// <summary>
/// Prepara la base de datos SQLite la primera vez que arranca el programa:
/// crea el archivo biblioteca.db junto al ejecutable y, si las tablas no
/// existen, las crea a partir del script embebido BaseDatos/crear_tablas.sql.
/// </summary>
internal sealed class InicializadorBaseDatos
{
    /// <summary>Cadena de conexion que debe usar el resto de la aplicacion.</summary>
    public string CadenaConexion { get; }

    private readonly string _rutaArchivo;

    public InicializadorBaseDatos()
    {
        _rutaArchivo = Path.Combine(AppContext.BaseDirectory, "biblioteca.db");
        CadenaConexion = $"Data Source={_rutaArchivo}";
    }

    /// <summary>
    /// Crea el archivo de base de datos (si no existe) y ejecuta el script
    /// de creacion de tablas. El script usa "IF NOT EXISTS", asi que es
    /// seguro llamar este metodo cada vez que arranca el programa.
    /// </summary>
    public void Inicializar()
    {
        using var conexion = new SqliteConnection(CadenaConexion);
        conexion.Open();

        using (var pragma = conexion.CreateCommand())
        {
            pragma.CommandText = "PRAGMA foreign_keys = ON;";
            pragma.ExecuteNonQuery();
        }

        foreach (var sentencia in LeerScriptEmbebido().Split(';'))
        {
            var sql = sentencia.Trim();
            if (sql.Length == 0) continue;

            using var comando = conexion.CreateCommand();
            comando.CommandText = sql;
            comando.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Lee el contenido del script crear_tablas.sql incrustado en el
    /// ensamblado (Build Action: Embedded Resource en el .csproj).
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Si el recurso embebido no se encuentra.
    /// </exception>
    private static string LeerScriptEmbebido()
    {
        var ensamblado = Assembly.GetExecutingAssembly();
        var recurso = ensamblado.GetManifestResourceNames()
            .FirstOrDefault(nombre => nombre.EndsWith("crear_tablas.sql", StringComparison.OrdinalIgnoreCase));

        if (recurso is null)
        {
            throw new InvalidOperationException(
                "No se encontro el script crear_tablas.sql incrustado en el proyecto. " +
                "Verifica que BaseDatos/crear_tablas.sql tenga Build Action = Embedded Resource.");
        }

        using var flujo = ensamblado.GetManifestResourceStream(recurso);
        using var lector = new StreamReader(flujo!);
        return lector.ReadToEnd();
    }
}
