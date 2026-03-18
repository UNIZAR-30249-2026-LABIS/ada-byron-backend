using AdaByron.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AdaByron.API.Middleware;

// Captura las ExcepcionDominio lanzadas por el dominio y las convierte
// en respuestas HTTP estándar con formato ProblemDetails (RFC 7807).
public class ExceptionHandlingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ExcepcionPermisos ex)
        {
            await EscribirProblema(context, ex.Message, (int)HttpStatusCode.Forbidden);
        }
        catch (ExcepcionConflictoReserva ex)
        {
            await EscribirProblema(context, ex.Message, (int)HttpStatusCode.Conflict);
        }
        catch (ExcepcionAforoSuperado ex)
        {
            await EscribirProblema(context, ex.Message, (int)HttpStatusCode.UnprocessableEntity);
        }
        catch (ExcepcionCambioCategoria ex)
        {
            await EscribirProblema(context, ex.Message, (int)HttpStatusCode.UnprocessableEntity);
        }
        catch (ExcepcionDominio ex)
        {
            await EscribirProblema(context, ex.Message, (int)HttpStatusCode.BadRequest);
        }
    }

    private static async Task EscribirProblema(HttpContext context, string detalle, int status)
    {
        context.Response.StatusCode  = status;
        context.Response.ContentType = "application/problem+json";

        var problema = new ProblemDetails
        {
            Status = status,
            Title  = DescripcionEstado(status),
            Detail = detalle,
        };

        await context.Response.WriteAsJsonAsync(problema);
    }

    private static string DescripcionEstado(int status) => status switch
    {
        400 => "Solicitud incorrecta",
        403 => "Sin permiso",
        409 => "Conflicto de reserva",
        422 => "Entidad no procesable",
        _   => "Error"
    };
}
