namespace ALGarage.Application.Abstractions;

/// <summary>Porta de envio de e-mail. Implementada na Infrastructure (SMTP; no-op/log se desabilitado).</summary>
public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}
