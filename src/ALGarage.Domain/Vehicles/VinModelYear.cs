namespace ALGarage.Domain.Vehicles;

/// <summary>
/// Decodificação do ano-modelo a partir do 10º caractere do VIN (posição padrão ISO 3779).
/// Há ambiguidade de ciclo de 30 anos (ex.: 'C' = 1982, 2012 ou 2042); para o escopo do produto
/// (carros recentes) resolvemos para a janela 2010–2039. Suficiente para o V40 (2012–2019).
/// </summary>
public static class VinModelYear
{
    // 10º caractere → ano, para a janela 2010–2039.
    private static readonly Dictionary<char, int> Map = new()
    {
        ['A'] = 2010, ['B'] = 2011, ['C'] = 2012, ['D'] = 2013, ['E'] = 2014, ['F'] = 2015,
        ['G'] = 2016, ['H'] = 2017, ['J'] = 2018, ['K'] = 2019, ['L'] = 2020, ['M'] = 2021,
        ['N'] = 2022, ['P'] = 2023, ['R'] = 2024, ['S'] = 2025, ['T'] = 2026, ['V'] = 2027,
        ['W'] = 2028, ['X'] = 2029, ['Y'] = 2030,
        ['1'] = 2031, ['2'] = 2032, ['3'] = 2033, ['4'] = 2034, ['5'] = 2035,
        ['6'] = 2036, ['7'] = 2037, ['8'] = 2038, ['9'] = 2039
    };

    public static int? TryDecode(Vin vin)
    {
        var c = vin.Value[9]; // 10º caractere (índice 9)
        return Map.TryGetValue(c, out var year) ? year : null;
    }
}
