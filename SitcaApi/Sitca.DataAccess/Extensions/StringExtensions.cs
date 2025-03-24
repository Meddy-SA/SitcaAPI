using System;

namespace Sitca.DataAccess.Extensions;

public static class StringExtensions
{
    public static DateTime? ToUniversal(this string fecha)
    {
        if (string.IsNullOrEmpty(fecha))
            return null;

        // Intentar parsear la fecha directamente usando DateTime.TryParse
        if (DateTime.TryParse(fecha, out DateTime parsedDate))
            return parsedDate;

        // Si el formato es específico (yyyy-MM-dd), usar un enfoque más robusto
        try
        {
            // Dividir la cadena y verificar que tenga exactamente 3 partes
            var terminos = fecha.Split('-');
            if (terminos.Length != 3)
                return null;

            if (
                int.TryParse(terminos[0], out int year)
                && int.TryParse(terminos[1], out int month)
                && int.TryParse(terminos[2], out int day)
            )
            {
                return new DateTime(year, month, day);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
