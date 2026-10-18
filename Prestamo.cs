namespace PrestamosBiblioteca.Dominio;

/// <summary>
/// Prestamo de un ejemplar de un libro a un lector. Mientras
/// <see cref="FechaDevolucion"/> sea null, el prestamo sigue activo y el
/// ejemplar cuenta como ocupado.
/// </summary>
public sealed class Prestamo
{
    /// <summary>Id del lector que recibe el prestamo.</summary>
    public int LectorId { get; }

    /// <summary>Id del libro prestado.</summary>
    public int LibroId { get; }

    /// <summary>Fecha en que se realizo el prestamo.</summary>
    public DateTime FechaPrestamo { get; }

    /// <summary>Fecha limite para devolver el libro.</summary>
    public DateTime FechaLimite { get; }

    /// <summary>Fecha de devolucion, o null si el prestamo sigue activo.</summary>
    public DateTime? FechaDevolucion { get; }

    /// <summary>True si el prestamo todavia no ha sido devuelto.</summary>
    public bool EstaActivo => FechaDevolucion is null;

    public Prestamo(int lectorId, int libroId, DateTime fechaPrestamo, DateTime fechaLimite, DateTime? fechaDevolucion = null)
    {
        if (lectorId <= 0)
            throw new ArgumentException("Debe indicar un lector valido.");
        if (libroId <= 0)
            throw new ArgumentException("Debe indicar un libro valido.");
        if (fechaLimite.Date < fechaPrestamo.Date)
            throw new ArgumentException("La fecha limite no puede ser anterior a la fecha de prestamo.");
        if (fechaDevolucion.HasValue && fechaDevolucion.Value.Date < fechaPrestamo.Date)
            throw new ArgumentException("La fecha de devolucion no puede ser anterior a la fecha de prestamo.");

        LectorId = lectorId;
        LibroId = libroId;
        FechaPrestamo = fechaPrestamo.Date;
        FechaLimite = fechaLimite.Date;
        FechaDevolucion = fechaDevolucion?.Date;
    }
}
