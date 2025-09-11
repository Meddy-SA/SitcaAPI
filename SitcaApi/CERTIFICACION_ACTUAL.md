# Lógica de Certificación Actual - SITCA

## Contexto del Negocio

En el sistema SITCA, una empresa puede tener múltiples procesos de certificación a lo largo del tiempo. Es crucial entender cuál es la "certificación actual" para mostrar el estado correcto del negocio en los reportes, dashboards y documentos oficiales.

## Escenarios de Negocio

Una empresa puede estar en cualquiera de los siguientes escenarios:

1. **Empresa nueva con proceso inicial**
   - Solo tiene un proceso sin finalizar (estados 0-7)
   - Este proceso es su certificación actual

2. **Empresa con certificación vigente**
   - Tiene un proceso finalizado (estado 8) con fecha de vencimiento futura
   - El certificado vigente es su certificación actual

3. **Empresa en reproceso inicial**
   - No hay registro de procesos anteriores (importada o migrada)
   - Está en un nuevo proceso de certificación
   - El proceso activo es su certificación actual

4. **Empresa en recertificación**
   - Tiene un certificado previo (vigente o vencido)
   - Inició un nuevo proceso de recertificación
   - El proceso activo de recertificación es su certificación actual

5. **Múltiples reprocesos**
   - Ha pasado por varios ciclos de certificación
   - Puede tener certificados vencidos y procesos activos
   - Se aplica la lógica de priorización

6. **Certificación vencida sin renovación**
   - Tiene certificado(s) vencido(s)
   - No ha iniciado proceso de renovación
   - El certificado vencido más reciente es su certificación actual

## Estados del Proceso

Los estados posibles de un proceso de certificación son:

- **0 - Inicial**: Proceso recién creado
- **1 - Para Asesorar**: Listo para comenzar asesoría
- **2 - Asesoría en Proceso**: Asesoría activa
- **3 - Asesoría Finalizada**: Asesoría completada
- **4 - Para Auditar**: Listo para auditoría
- **5 - Auditoría en Proceso**: Auditoría activa
- **6 - Auditoría Finalizada**: Auditoría completada
- **7 - En revisión de CTC**: Comité Técnico de Certificación
- **8 - Finalizado**: Proceso completado (con o sin certificación)

## Algoritmo de Selección de Certificación Actual

El método `ProcessCurrentCertification` en `EmpresaRepository.cs` implementa la siguiente lógica de priorización:

### Orden de Prioridad

1. **Proceso Activo** (Estados 0-7)
   - Si existe un proceso en curso, este es siempre la certificación actual
   - Se toma el más reciente por ID si hay múltiples

2. **Certificación Vigente** (Estado 8 + Fecha de vencimiento futura)
   - Si no hay proceso activo, busca certificados finalizados y vigentes
   - Se prioriza por fecha de vencimiento más reciente

3. **Certificación Vencida** (Estado 8 + Fecha de vencimiento pasada)
   - Si no hay proceso activo ni certificado vigente
   - Se toma la certificación vencida con fecha de vencimiento más reciente
   - Importante: Se ordena por fecha de vencimiento, NO por ID

4. **Fallback** (Cualquier proceso)
   - Como último recurso, toma el proceso más reciente por ID
   - Cubre casos edge no contemplados

## Alerta de Vencimiento

La certificación actual incluye una alerta de vencimiento (`alertaVencimiento`) que se activa cuando:
- La certificación tiene fecha de vencimiento definida
- La fecha de vencimiento está dentro de los próximos 6 meses

## Implementación Técnica

```csharp
// Ubicación: Sitca.DataAccess/Data/Repository/EmpresaRepository.cs
private static CertificacionDetailsVm ProcessCurrentCertification(
    List<CertificacionDetailsVm> certificaciones)
{
    // 1. Buscar proceso activo (estados 0-7)
    // 2. Buscar certificación vigente (estado 8 + vigente)
    // 3. Buscar certificación vencida más reciente por fecha
    // 4. Fallback: proceso más reciente por ID
}
```

## Uso en el Sistema

Esta lógica se utiliza en:
- **Dashboards**: Para mostrar el estado actual de cada empresa
- **Reportes PDF**: Dictamen técnico, recomendaciones CTC
- **APIs**: Endpoint `/api/empresas/{id}` retorna certificación actual
- **Notificaciones**: Para alertas de vencimiento

## Consideraciones Importantes

1. **No asumir orden por ID**: El ID más alto no siempre es la certificación más relevante
2. **Priorizar procesos activos**: Un proceso en curso siempre tiene precedencia
3. **Fechas de vencimiento**: Son el criterio principal para certificaciones finalizadas
4. **Tipología**: La certificación actual determina la tipología mostrada en reportes

## Casos Edge

- **Sin certificaciones**: No debería ocurrir, pero el sistema maneja gracefully
- **Múltiples procesos activos**: Se toma el más reciente por ID
- **Fechas null**: Se manejan apropiadamente en las comparaciones
- **Estados en diferentes idiomas**: Se verifica por contenido ("8 -", "Finalizado", "Completed")

## Prompt para IA

Para futuros cambios o explicaciones sobre la certificación actual, usar este contexto:

```
En SITCA, la "certificación actual" de una empresa se determina por prioridad:
1. Proceso activo más reciente (estados 0-7)
2. Certificación vigente (estado 8 con fecha futura)
3. Certificación vencida más reciente por fecha de vencimiento
4. Fallback: proceso más reciente por ID

Una empresa puede tener múltiples procesos: inicial, recertificación, o certificados vencidos sin renovación. 
La lógica está en ProcessCurrentCertification() en EmpresaRepository.cs.
Los estados van de 0 (Inicial) a 8 (Finalizado).
```

## Historial de Cambios

- **2024-01**: Implementación inicial - Solo tomaba el proceso más reciente por ID
- **2024-12**: Refactoring completo - Implementación de lógica de priorización por estado y fechas de vencimiento