using Utilities.Common;

public static class StatusConverter
{
    // Diccionarios de mapeo para búsquedas eficientes
    private static readonly Dictionary<string, int> _textToStatusMap;
    private static readonly Dictionary<int, string> _statusToSpanishMap;
    private static readonly Dictionary<int, string> _statusToEnglishMap;

    // Inicialización estática
    static StatusConverter()
    {
        // Inicializar diccionario de texto a estado
        _textToStatusMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            // Español con prefijo numérico
            { Constants.ProcessStatusText.Spanish.Initial, Constants.ProcessStatus.Initial },
            {
                Constants.ProcessStatusText.Spanish.ForConsulting,
                Constants.ProcessStatus.ForConsulting
            },
            {
                Constants.ProcessStatusText.Spanish.ConsultancyUnderway,
                Constants.ProcessStatus.ConsultancyUnderway
            },
            {
                Constants.ProcessStatusText.Spanish.ConsultancyCompleted,
                Constants.ProcessStatus.ConsultancyCompleted
            },
            {
                Constants.ProcessStatusText.Spanish.ForAuditing,
                Constants.ProcessStatus.ForAuditing
            },
            {
                Constants.ProcessStatusText.Spanish.AuditingUnderway,
                Constants.ProcessStatus.AuditingUnderway
            },
            {
                Constants.ProcessStatusText.Spanish.AuditCompleted,
                Constants.ProcessStatus.AuditCompleted
            },
            {
                Constants.ProcessStatusText.Spanish.UnderCCTReview,
                Constants.ProcessStatus.UnderCCTReview
            },
            { Constants.ProcessStatusText.Spanish.Completed, Constants.ProcessStatus.Completed },
            // Español sin prefijo numérico
            {
                Constants.ProcessStatusText.Spanish.InitialNoNumber,
                Constants.ProcessStatus.Initial
            },
            {
                Constants.ProcessStatusText.Spanish.ForConsultingNoNumber,
                Constants.ProcessStatus.ForConsulting
            },
            {
                Constants.ProcessStatusText.Spanish.ConsultancyUnderwayNoNumber,
                Constants.ProcessStatus.ConsultancyUnderway
            },
            {
                Constants.ProcessStatusText.Spanish.ConsultancyCompletedNoNumber,
                Constants.ProcessStatus.ConsultancyCompleted
            },
            {
                Constants.ProcessStatusText.Spanish.ForAuditingNoNumber,
                Constants.ProcessStatus.ForAuditing
            },
            {
                Constants.ProcessStatusText.Spanish.AuditingUnderwayNoNumber,
                Constants.ProcessStatus.AuditingUnderway
            },
            {
                Constants.ProcessStatusText.Spanish.AuditCompletedNoNumber,
                Constants.ProcessStatus.AuditCompleted
            },
            {
                Constants.ProcessStatusText.Spanish.UnderCCTReviewNoNumber,
                Constants.ProcessStatus.UnderCCTReview
            },
            {
                Constants.ProcessStatusText.Spanish.CompletedNoNumber,
                Constants.ProcessStatus.Completed
            },
            // Inglés con prefijo numérico
            { Constants.ProcessStatusText.English.Initial, Constants.ProcessStatus.Initial },
            {
                Constants.ProcessStatusText.English.ForConsulting,
                Constants.ProcessStatus.ForConsulting
            },
            {
                Constants.ProcessStatusText.English.ConsultancyUnderway,
                Constants.ProcessStatus.ConsultancyUnderway
            },
            {
                Constants.ProcessStatusText.English.ConsultancyCompleted,
                Constants.ProcessStatus.ConsultancyCompleted
            },
            {
                Constants.ProcessStatusText.English.ForAuditing,
                Constants.ProcessStatus.ForAuditing
            },
            {
                Constants.ProcessStatusText.English.AuditingUnderway,
                Constants.ProcessStatus.AuditingUnderway
            },
            {
                Constants.ProcessStatusText.English.AuditCompleted,
                Constants.ProcessStatus.AuditCompleted
            },
            {
                Constants.ProcessStatusText.English.UnderCCTReview,
                Constants.ProcessStatus.UnderCCTReview
            },
            { Constants.ProcessStatusText.English.Completed, Constants.ProcessStatus.Completed },
            // Inglés sin prefijo numérico
            {
                Constants.ProcessStatusText.English.InitialNoNumber,
                Constants.ProcessStatus.Initial
            },
            {
                Constants.ProcessStatusText.English.ForConsultingNoNumber,
                Constants.ProcessStatus.ForConsulting
            },
            {
                Constants.ProcessStatusText.English.ConsultancyUnderwayNoNumber,
                Constants.ProcessStatus.ConsultancyUnderway
            },
            {
                Constants.ProcessStatusText.English.ConsultancyCompletedNoNumber,
                Constants.ProcessStatus.ConsultancyCompleted
            },
            {
                Constants.ProcessStatusText.English.ForAuditingNoNumber,
                Constants.ProcessStatus.ForAuditing
            },
            {
                Constants.ProcessStatusText.English.AuditingUnderwayNoNumber,
                Constants.ProcessStatus.AuditingUnderway
            },
            {
                Constants.ProcessStatusText.English.AuditCompletedNoNumber,
                Constants.ProcessStatus.AuditCompleted
            },
            {
                Constants.ProcessStatusText.English.UnderCCTReviewNoNumber,
                Constants.ProcessStatus.UnderCCTReview
            },
            {
                Constants.ProcessStatusText.English.CompletedNoNumber,
                Constants.ProcessStatus.Completed
            },
        };

        // Inicializar diccionarios de estado a texto
        _statusToSpanishMap = new Dictionary<int, string>
        {
            { Constants.ProcessStatus.Initial, Constants.ProcessStatusText.Spanish.Initial },
            {
                Constants.ProcessStatus.ForConsulting,
                Constants.ProcessStatusText.Spanish.ForConsulting
            },
            {
                Constants.ProcessStatus.ConsultancyUnderway,
                Constants.ProcessStatusText.Spanish.ConsultancyUnderway
            },
            {
                Constants.ProcessStatus.ConsultancyCompleted,
                Constants.ProcessStatusText.Spanish.ConsultancyCompleted
            },
            {
                Constants.ProcessStatus.ForAuditing,
                Constants.ProcessStatusText.Spanish.ForAuditing
            },
            {
                Constants.ProcessStatus.AuditingUnderway,
                Constants.ProcessStatusText.Spanish.AuditingUnderway
            },
            {
                Constants.ProcessStatus.AuditCompleted,
                Constants.ProcessStatusText.Spanish.AuditCompleted
            },
            {
                Constants.ProcessStatus.UnderCCTReview,
                Constants.ProcessStatusText.Spanish.UnderCCTReview
            },
            { Constants.ProcessStatus.Completed, Constants.ProcessStatusText.Spanish.Completed },
        };

        _statusToEnglishMap = new Dictionary<int, string>
        {
            { Constants.ProcessStatus.Initial, Constants.ProcessStatusText.English.Initial },
            {
                Constants.ProcessStatus.ForConsulting,
                Constants.ProcessStatusText.English.ForConsulting
            },
            {
                Constants.ProcessStatus.ConsultancyUnderway,
                Constants.ProcessStatusText.English.ConsultancyUnderway
            },
            {
                Constants.ProcessStatus.ConsultancyCompleted,
                Constants.ProcessStatusText.English.ConsultancyCompleted
            },
            {
                Constants.ProcessStatus.ForAuditing,
                Constants.ProcessStatusText.English.ForAuditing
            },
            {
                Constants.ProcessStatus.AuditingUnderway,
                Constants.ProcessStatusText.English.AuditingUnderway
            },
            {
                Constants.ProcessStatus.AuditCompleted,
                Constants.ProcessStatusText.English.AuditCompleted
            },
            {
                Constants.ProcessStatus.UnderCCTReview,
                Constants.ProcessStatusText.English.UnderCCTReview
            },
            { Constants.ProcessStatus.Completed, Constants.ProcessStatusText.English.Completed },
        };
    }

    /// <summary>
    /// Convierte un texto que describe un estado en su valor numérico correspondiente
    /// según las constantes definidas en Constants.ProcessStatus.
    /// </summary>
    /// <param name="statusText">Texto que describe el estado (ej: "1 - Para Asesorar" o "1 - For consulting")</param>
    /// <returns>El valor numérico del estado o -1 si no se puede convertir</returns>
    public static int ConvertStatusTextToInt(string statusText)
    {
        if (string.IsNullOrWhiteSpace(statusText))
            return -1;

        // Verificamos si el texto comienza con un número seguido de un espacio y un guion
        if (
            statusText.Length >= 3
            && char.IsDigit(statusText[0])
            && statusText[1] == ' '
            && statusText[2] == '-'
        )
        {
            // Extraemos el primer carácter y lo convertimos a int
            if (int.TryParse(statusText[0].ToString(), out int result))
                return result;
        }

        // Verificar coincidencia exacta en el diccionario
        if (_textToStatusMap.TryGetValue(statusText, out int statusValue))
        {
            return statusValue;
        }

        // Si no hay coincidencia exacta, buscar por contención
        string textLower = statusText.ToLowerInvariant();

        // Casos especiales que requieren lógica adicional
        // Para "Finalizado"/"Completed" hay que tener cuidado con los casos como "Asesoria Finalizada"
        if (
            textLower.Contains(
                Constants.ProcessStatusText.Spanish.CompletedNoNumber.ToLowerInvariant()
            )
            && !textLower.Contains("asesoria")
            && !textLower.Contains("auditoria")
        )
        {
            return Constants.ProcessStatus.Completed;
        }

        if (
            textLower.Contains(
                Constants.ProcessStatusText.English.CompletedNoNumber.ToLowerInvariant()
            )
            && !textLower.Contains("consultancy")
            && !textLower.Contains("audit")
        )
        {
            return Constants.ProcessStatus.Completed;
        }

        // Búsqueda por contención para todos los demás estados
        foreach (var pair in _textToStatusMap.OrderByDescending(kvp => kvp.Key.Length)) // Ordenar por longitud para priorizar las coincidencias más específicas
        {
            if (textLower.Contains(pair.Key.ToLowerInvariant()))
            {
                // Evitar falsas coincidencias para "Completed"/"Finalizado"
                if (
                    pair.Value == Constants.ProcessStatus.Completed
                    && (
                        textLower.Contains("asesoria")
                        || textLower.Contains("auditoria")
                        || textLower.Contains("consultancy")
                        || textLower.Contains("audit")
                    )
                )
                {
                    continue;
                }

                return pair.Value;
            }
        }

        return -1; // No se encontró coincidencia
    }

    /// <summary>
    /// Convierte un valor entero de estado a su representación en texto
    /// </summary>
    /// <param name="statusValue">Valor numérico del estado (0-8)</param>
    /// <param name="language">Idioma deseado ("es" para español, cualquier otro valor para inglés)</param>
    /// <returns>El texto correspondiente al estado o string vacío si no existe</returns>
    public static string ConvertStatusIntToText(int statusValue, string language = "en")
    {
        var dictionary =
            language.ToLowerInvariant() == "es" ? _statusToSpanishMap : _statusToEnglishMap;

        // Verificar si el diccionario contiene el valor
        if (dictionary != null && dictionary.TryGetValue(statusValue, out string? text))
        {
            // Garantizar que text no sea nulo
            return text ?? string.Empty;
        }

        return string.Empty;
    }

    /// <summary>
    /// Convierte un valor entero de estado a su representación en texto en español
    /// </summary>
    /// <param name="statusValue">Valor numérico del estado (0-8)</param>
    /// <returns>El texto en español correspondiente al estado o string vacío si no existe</returns>
    public static string ConvertStatusIntToSpanishText(int statusValue)
    {
        return ConvertStatusIntToText(statusValue, "es");
    }

    /// <summary>
    /// Convierte un valor entero de estado a su representación en texto en inglés
    /// </summary>
    /// <param name="statusValue">Valor numérico del estado (0-8)</param>
    /// <returns>El texto en inglés correspondiente al estado o string vacío si no existe</returns>
    public static string ConvertStatusIntToEnglishText(int statusValue)
    {
        return ConvertStatusIntToText(statusValue, "en");
    }
}
