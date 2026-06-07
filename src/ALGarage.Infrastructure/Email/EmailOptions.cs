namespace ALGarage.Infrastructure.Email;

/// <summary>Configuração de e-mail (seção "Email"). Desabilitado por padrão (apenas loga).</summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; }
    public string Host { get; set; } = "";
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string From { get; set; } = "no-reply@algarage.local";
}
