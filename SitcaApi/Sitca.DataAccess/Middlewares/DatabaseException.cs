using System;

namespace Sitca.DataAccess.Middlewares;

/// <summary>
/// Excepción personalizada para errores relacionados con operaciones de base de datos
/// </summary>
public class DatabaseException : Exception
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="DatabaseException"/>
    /// </summary>
    public DatabaseException()
        : base() { }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="DatabaseException"/> con un mensaje de error específico
    /// </summary>
    /// <param name="message">El mensaje que describe el error</param>
    public DatabaseException(string message)
        : base(message) { }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="DatabaseException"/> con un mensaje
    /// de error específico y una referencia a la excepción interna que es la causa de esta excepción
    /// </summary>
    /// <param name="message">El mensaje que describe el error</param>
    /// <param name="innerException">La excepción que causó la excepción actual</param>
    public DatabaseException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Proporciona datos adicionales sobre la operación de base de datos que falló
    /// </summary>
    public string SqlStatement { get; set; }

    /// <summary>
    /// Identifica la entidad relacionada con la excepción
    /// </summary>
    public string EntityName { get; set; }

    /// <summary>
    /// Establece información sobre la entidad que causó la excepción
    /// </summary>
    /// <param name="entityName">Nombre de la entidad</param>
    /// <returns>La misma instancia para permitir llamadas encadenadas</returns>
    public DatabaseException WithEntity(string entityName)
    {
        EntityName = entityName;
        return this;
    }

    /// <summary>
    /// Establece información sobre la sentencia SQL que causó la excepción
    /// </summary>
    /// <param name="sqlStatement">Sentencia SQL</param>
    /// <returns>La misma instancia para permitir llamadas encadenadas</returns>
    public DatabaseException WithSqlStatement(string sqlStatement)
    {
        SqlStatement = sqlStatement;
        return this;
    }

    /// <summary>
    /// Obtiene un mensaje que describe la excepción actual, incluyendo información adicional si está disponible
    /// </summary>
    public override string Message
    {
        get
        {
            var baseMessage = base.Message;

            if (!string.IsNullOrEmpty(EntityName) && !string.IsNullOrEmpty(SqlStatement))
            {
                return $"{baseMessage} | Entidad: {EntityName} | SQL: {SqlStatement}";
            }
            else if (!string.IsNullOrEmpty(EntityName))
            {
                return $"{baseMessage} | Entidad: {EntityName}";
            }
            else if (!string.IsNullOrEmpty(SqlStatement))
            {
                return $"{baseMessage} | SQL: {SqlStatement}";
            }

            return baseMessage;
        }
    }
}
