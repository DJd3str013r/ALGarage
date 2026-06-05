using System.Text.RegularExpressions;

namespace ALGarage.Domain.Vehicles;

/// <summary>
/// Value object do VIN (Vehicle Identification Number). 17 caracteres, sem I/O/Q.
/// Mantém a invariante de formato fora da entidade. O cálculo do dígito verificador
/// (posição 9) é padronizado na América do Norte e fica como melhoria futura — aqui
/// validamos formato/charset, que já barra a maioria dos erros de digitação.
/// </summary>
public readonly partial record struct Vin
{
    public string Value { get; }

    private Vin(string value) => Value = value;

    public static Vin Parse(string raw)
    {
        if (!TryParse(raw, out var vin))
        {
            throw new ArgumentException($"VIN inválido: '{raw}'. Esperado 17 caracteres (sem I, O ou Q).", nameof(raw));
        }

        return vin;
    }

    public static bool TryParse(string? raw, out Vin vin)
    {
        vin = default;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var normalized = raw.Trim().ToUpperInvariant();
        if (!VinPattern().IsMatch(normalized))
        {
            return false;
        }

        vin = new Vin(normalized);
        return true;
    }

    /// <summary>World Manufacturer Identifier — os 3 primeiros caracteres (ex.: identifica a Volvo).</summary>
    public string Wmi => Value[..3];

    public override string ToString() => Value;

    [GeneratedRegex("^[A-HJ-NPR-Z0-9]{17}$")]
    private static partial Regex VinPattern();
}
