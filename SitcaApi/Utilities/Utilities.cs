using System.Globalization;
using System.Text.RegularExpressions;

namespace Utilities;

public static class Utilities
{
  public static string? GetCountry(int countryId)
  {
    switch (countryId)
    {
      case 1:
        return "Belize";
      case 2:
        return "Guatemala";
      case 3:
        return "El Salvador";
      case 4:
        return "Honduras";
      case 5:
        return "Nicaragua";
      case 6:
        return "Costa Rica";
      case 7:
        return "Panama";
      case 8:
        return "Republica Dominicana";
    }
    return null;
  }

  public static string RoleToText(IList<string> roles, string lang)
  {
    //var result = Roles.Select(s =>s).Concat(",");
    var result = "";

    foreach (var item in roles)
    {
      switch (item)
      {
        case "Asesor":
          result = lang == "es" ? item : "Consultant";
          break;
        case "TecnicoPais":
          result = lang == "es" ? "Técnico País" : "Country Technician";
          break;
        case "Empresa":
          result = lang == "es" ? item : "Company";
          break;
        case "CTC":
          result = lang == "es" ? item : "CCT";
          break;
        default:
          result = item;
          break;
      }
    }

    return result;
  }

  public static List<string> TranslateRoles(IList<string> roles, string lang)
  {
    var result = new List<string>();

    foreach (var item in roles)
    {
      if (lang == "en")
      {
        switch (item)
        {
          case "Asesor":
            result.Add("Consultant");
            break;
          case "TecnicoPais":
            result.Add("Country Technician");
            break;
          case "Empresa":
            result.Add("Company");
            break;
          case "CTC":
            result.Add("CCT");

            break;
          default:
            result.Add(item);
            break;
        }
      }
      else
      {
        switch (item)
        {
          case "Consultant":
            result.Add("Asesor");
            break;
          case "Country Technician":
            result.Add("TecnicoPais");

            break;
          case "Company":
            result.Add("Company");
            break;
          case "CCT":
            result.Add("CTC");

            break;
          default:
            result.Add(item);
            break;
        }
      }

    }

    return result;
  }

  public static bool IsNumeric(this string input)
  {
    int number;
    try
    {
      return int.TryParse(input, out number);

    }
    catch (Exception)
    {

      return false;
    }

  }

  public static string GetEstado(this decimal? input, string leng)
  {

    //0 - Inicial
    //1 - Para Asesorar
    //2 - Asesoria en Proceso
    //3 - Asesoria Finalizada
    //4 - Para Auditar
    //5 - Auditoria en Proceso
    //6 - Auditoria Finalizada
    //7 - En revisión de CTC
    //8 - Finalizado

    if (leng == "es")
    {
      switch (input)
      {
        case 0: return "0 - Inicial";
        case 1: return "1 - Para Asesorar";
        case 2: return "2 - Asesoria en Proceso";
        case 3: return "3 - Asesoria Finalizada";
        case 4: return "4 - Para Auditar";
        case 5: return "5 - Auditoria en Proceso";
        case 6: return "6 - Auditoria Finalizada";
        case 7: return "7 - En revisión de CTC";
        case 8: return "8 - Finalizado";
        default:
          return "";
      }
    }

    switch (input)
    {
      case 0: return "0 - Start";
      case 1: return "1 - For consulting";
      case 2: return "2 - Consultancy underway";
      case 3: return "3 - Consultancy completed";
      case 4: return "4 - For auditing";
      case 5: return "5 - Auditing underway";
      case 6: return "6 - Audit completed";
      case 7: return "7 - Under CCT Review";
      case 8: return "8 - Completed";
    }
    return "";

  }

  public static string CambiarIdiomaEstado(string estado, string leng)
  {
    var input = Int32.Parse(estado[0].ToString());

    if (leng == "es")
    {
      switch (input)
      {
        case 0: return "0 - Inicial";
        case 1: return "1 - Para Asesorar";
        case 2: return "2 - Asesoria en Proceso";
        case 3: return "3 - Asesoria Finalizada";
        case 4: return "4 - Para Auditar";
        case 5: return "5 - Auditoria en Proceso";
        case 6: return "6 - Auditoria Finalizada";
        case 7: return "7 - En revisión de CTC";
        case 8: return "8 - Finalizado";
        default:
          return "";
      }
    }

    switch (input)
    {
      case 0: return "0 - Start";
      case 1: return "1 - For consulting";
      case 2: return "2 - Consultancy underway";
      case 3: return "3 - Consultancy completed";
      case 4: return "4 - For auditing";
      case 5: return "5 - Auditing underway";
      case 6: return "6 - Audit completed";
      case 7: return "7 - Under CCT Review";
      case 8: return "8 - Completed";
    }
    return "";

  }

  public static DateTime ChangeTime(this DateTime dateTime, int hours, int minutes)
  {
    return new DateTime(
        dateTime.Year,
        dateTime.Month,
        dateTime.Day,
        hours,
        minutes,
        0,
        0,
        dateTime.Kind);
  }

  public static string GetSKU(this int idProduct, int? idPresentacion, string color, string size)
  {
    string result = idProduct.ToString();
    if (idPresentacion != null && idPresentacion != 0)
    {
      result += "-" + idPresentacion;
    }

    if (color != null && !string.IsNullOrEmpty(color.Trim()))
    {
      result += "-" + color;
    }

    if (size != null && !string.IsNullOrEmpty(size.Trim()))
    {
      result += "-" + size;
    }

    return result;
  }

  public static string ToCode(this int number, string prefix, int spaces)
  {
    prefix = prefix.RemoveAccent();
    prefix = prefix.ToUpper();
    return prefix + number.ToString(CultureInfo.InvariantCulture).PadLeft(spaces, '0');
  }

  public static string CamelCase(this string frase)
  {
    var terminos = frase.Split(' ');
    var frasefinal = terminos.Where(item => item.Length > 2).Aggregate("", (current, item) => current + (" " + item.Substring(0, 1).ToUpper() + item.Substring(1, item.Length - 1).ToLower()));
    return frasefinal.Trim();
  }

  public static string CamelCase2(this string frase)
  {
    var terminos = frase.Split(' ');
    var frasefinal = terminos.Where(item => item.Length > 0 || item == "-").Aggregate("", (current, item) => current + (" " + item.Substring(0, 1).ToUpper() + item.Substring(1, item.Length - 1).ToLower()));

    if (frasefinal.Length > 35)
    {
      frasefinal = frasefinal.Replace("Unds", "U");
      frasefinal = frasefinal.Replace("Unids", "U");
      frasefinal = frasefinal.Replace("Docena", "Doc");
      frasefinal = frasefinal.Replace("Caja", "Cja");
      frasefinal = frasefinal.Replace("Dispensador", "Disp");
      frasefinal = frasefinal.Replace("Paquete", "Paq");
      frasefinal = frasefinal.Replace("Botella", "Bot");

      if (frasefinal.Length > 55)
      {
        frasefinal = frasefinal.Substring(0, Math.Min(frasefinal.Length, 55));
      }
    }

    return frasefinal.Trim();
  }

  public static DateTime? ToDateArg(this string fecha)
  {
    if (!string.IsNullOrEmpty(fecha))
    {
      try
      {
        var terminos = fecha.Split('/');
        return new DateTime(Int32.Parse(terminos[2]), Int32.Parse(terminos[1]), Int32.Parse(terminos[0]));
      }
      catch (Exception)
      {
        return null;
      }
    }
    else
    {
      return null;
    }
  }
  public static DateTime ToDateArg(this string fecha, DateTime defaultTime)
  {
    if (!string.IsNullOrEmpty(fecha))
    {
      try
      {
        var terminos = fecha.Split('/');
        return new DateTime(Int32.Parse(terminos[2]), Int32.Parse(terminos[1]), Int32.Parse(terminos[0]));
      }
      catch (Exception)
      {
        return defaultTime;
      }
    }
    else
    {
      return defaultTime;
    }
  }
  public static DateTime? ToDateUsa(this string fecha)
  {
    if (!string.IsNullOrEmpty(fecha))
    {
      try
      {
        var terminos = fecha.Split('/');
        return new DateTime(Int32.Parse(terminos[2]), Int32.Parse(terminos[0]), Int32.Parse(terminos[1]));
      }
      catch (Exception)
      {
        return null;
      }
    }
    else
    {
      return null;
    }
  }
  public static DateTime? ToDateUniversal(this string fecha)
  {
    if (!string.IsNullOrEmpty(fecha))
    {
      try
      {
        var terminos = fecha.Split('-');
        return new DateTime(Int32.Parse(terminos[0]), Int32.Parse(terminos[1]), Int32.Parse(terminos[2]));
      }
      catch (Exception)
      {
        return null;
      }
    }
    else
    {
      return null;
    }
  }


  public static string? ToStringUsa(this DateTime? fecha)
  {
    if (fecha != null)
      return fecha.Value.ToString("MM/dd/yyyy", System.Globalization.CultureInfo.InvariantCulture);
    else return null;
  }
  public static string? ToUtc(this DateTime? fecha)
  {
    if (fecha != null)
      return fecha.Value.ToString("s") + "Z";
    else return null;
  }
  public static string? ToUtc(this DateTime fecha)
  {
    return fecha.ToString("s") + "Z";
  }
  public static string? ToStringArg(this DateTime? fecha)
  {
    if (fecha != null)
      return fecha.Value.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
    else return null;
  }
  public static string? ToStringArg(this DateTime fecha)
  {
    return fecha.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
  }

  public static string? ToStringArgWhitTime(this DateTime fecha)
  {
    return fecha.ToString("dd/MM/yyyy HH:mm");
  }
  public static string? CleanDecimalString(this string number)
  {
    if (!string.IsNullOrWhiteSpace(number))
    {
      var stringLimpio = Regex.Replace(number, "[^0-9.]", "");
      try
      {
        var dec = Decimal.Parse(stringLimpio, System.Globalization.CultureInfo.InvariantCulture);
        return dec.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
      }
      catch (Exception)
      {
        try
        {
          var stringLimpio2 = Regex.Replace(number, "[^0-9]", "");
          var dec2 = Decimal.Parse(stringLimpio2);
          return dec2.ToString("G");
        }
        catch (Exception)
        {
          return null;
        }

      }
    }
    return null;
  }
  public static Decimal? ToDecimal(this string value)
  {
    if (!string.IsNullOrWhiteSpace(value))
    {
      if (value.Contains(','))
      {
        value = value.Replace(',', '.');
      }

      if (value.Contains("E-"))
      {
        return 0;
      }


      var dec = Decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
      return dec;
    }
    else
    {
      return null;
    }

  }
  public static Decimal ToDecimalStrict(this string value)
  {
    if (!string.IsNullOrWhiteSpace(value))
    {
      if (value.Contains(','))
      {
        value = value.Replace(',', '.');
      }
      var dec = Decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
      return dec;
    }
    else
    {
      return 0;
    }

  }
  public static long? ToTicks(this string value)
  {

    if (!string.IsNullOrWhiteSpace(value))
    {
      if (value.Contains('.'))
      {
        var doub = value.ToDouble();
        if (doub != null)
        {
          return TimeSpan.FromHours(doub.Value).Ticks;
        }
        return null;
      }
      else
      {
        if (value.Contains(':'))
        {
          try
          {
            var ts = new TimeSpan(int.Parse(value.Split(':')[0]),    // hours
           int.Parse(value.Split(':')[1]),    // minutes
           0);

            return ts.Ticks;

          }
          catch (Exception)
          {
            return null;
          }
        }
        else
        {
          var doub = value.ToDouble();
          if (doub != null)
          {
            return TimeSpan.FromHours(doub.Value).Ticks;
          }
          return null;

        }
      }
    }
    else
    {
      return null;
    }
  }
  public static Double? ToDouble(this string value)
  {
    if (!string.IsNullOrWhiteSpace(value))
    {
      var dec = Double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
      return dec;
    }
    else
    {
      return null;
    }

  }

  public static string? ToHuman(this Decimal? value)
  {
    return value != null ? string.Format(CultureInfo.InvariantCulture, "{0:n}", value.Value) : null;
  }

  public static string ToHuman(this Decimal value)
  {
    return string.Format(CultureInfo.InvariantCulture, "{0:n}", value);
  }

  public static string? ToStringUsa(this Decimal? value)
  {
    if (value != null)
    {
      return value.Value.ToString("G29", CultureInfo.InvariantCulture);
    }
    return null;
  }
  public static string ToStringUsa(this Decimal value)
  {
    var result = value.ToString("G29", CultureInfo.InvariantCulture);
    return result;
  }

  public static string ToStringUsaWithAllDecimal(this Decimal value)
  {
    var cult = new CultureInfo("en-US");
    return string.Format(cult, "{0:n6}", value, System.Globalization.CultureInfo.InvariantCulture).Replace(",", "");
  }

  public static string? ToStringUsaCut(this Decimal? value)
  {
    if (value != null)
    {
      var cult = new CultureInfo("en-US");
      return string.Format(cult, "{0:n2}", value, System.Globalization.CultureInfo.InvariantCulture).Replace(",", "");
    }
    return null;
  }
  public static string ToStringUsaCut(this Decimal value)
  {
    var cult = new CultureInfo("en-US");
    return string.Format(cult, "{0:n2}", value, System.Globalization.CultureInfo.InvariantCulture).Replace(",", ""); ;
  }

  public static decimal Truncate(this Decimal? value)
  {
    if (value != null)
    {
      value = Math.Truncate((value ?? 0) * 100) / 100;
      return value ?? 0;
    }
    return 0;
  }

  public static string? ToStringTruncate(this Decimal? value)
  {
    if (value != null)
    {
      value = Math.Truncate((value ?? 0) * 100) / 100;
      var cult = new CultureInfo("en-US");
      return string.Format(cult, "{0:n2}", value, System.Globalization.CultureInfo.InvariantCulture).Replace(",", "");
    }
    return null;
  }
  public static string ToStringTruncate(this Decimal value)
  {
    value = Math.Truncate(value * 100) / 100;
    var cult = new CultureInfo("en-US");
    return string.Format(cult, "{0:n2}", value, System.Globalization.CultureInfo.InvariantCulture).Replace(",", ""); ;
  }


  public static string? TicksToString(this long? value)
  {
    if (value != null)
    {
      var time = TimeSpan.FromTicks(value.Value);
      return string.Format("{0:00}:{1:D2}", (int)time.TotalHours, time.Minutes);
    }
    return null;
  }



  public static string ToCode(this string value)
  {
    value = value.RemoveAccent();
    value = value.ToUpper();

    if (value.StartsWith("#"))
    {
      return value;
    }
    else
    {
      return "#" + value;
    }
  }

  public static string? ToSlug(this string phrase)
  {
    if (string.IsNullOrWhiteSpace(phrase))
      return null;

    string str = phrase.RemoveAccent().ToLower();
    // invalid chars           
    str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
    // convert multiple spaces into one space   
    str = Regex.Replace(str, @"\s+", " ").Trim();
    // cut and trim 
    str = str.Substring(0, str.Length <= 45 ? str.Length : 45).Trim();
    str = Regex.Replace(str, @"\s", "-"); // hyphens   
    return str;
  }

  public static string RemoveAccent(this string txt)
  {
    byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(txt);
    return System.Text.Encoding.ASCII.GetString(bytes);
  }

  public static string IsNullZero(this string str)
  {
    return string.IsNullOrEmpty(str) ? "0" : str;
  }

  public static int GetMesNumber(this string mes)
  {
    switch (mes)
    {
      case "Enero":
        return 1;
      case "Febrero":
        return 2;
      case "Marzo":
        return 3;
      case "Abril":
        return 4;
      case "Mayo":
        return 5;
      case "Junio":
        return 6;
      case "Julio":
        return 7;
      case "Agosto":
        return 8;
      case "Septiembre":
        return 9;
      case "Octubre":
        return 10;
      case "Noviembre":
        return 11;
      case "Diciembre":
        return 12;
      default:
        return 0;
    }
  }
}

