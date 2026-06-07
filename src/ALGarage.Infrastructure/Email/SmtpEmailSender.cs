using System.Net;
using System.Net.Mail;
using ALGarage.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ALGarage.Infrastructure.Email;

/// <summary>
/// Envia e-mail via SMTP. Se <see cref="EmailOptions.Enabled"/> for falso (padrão), apenas registra
/// no log — assim o app roda em dev/CI sem servidor SMTP. Configure a seção "Email" para habilitar.
/// </summary>
public sealed class SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation("E-mail desabilitado; lembrete NÃO enviado para {To} (assunto: {Subject}).", to, subject);
            return;
        }

        using var message = new MailMessage(_options.From, to, subject, body);
        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.UseSsl,
            Credentials = string.IsNullOrEmpty(_options.Username)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(_options.Username, _options.Password)
        };

        await client.SendMailAsync(message, ct);
        logger.LogInformation("Lembrete enviado para {To}.", to);
    }
}
