namespace Shared.Constants;

public static class WaterQualityParameters
{
    public const string Ph = "pH";
    public const string Turbidity = "Turbidity";
    public const string DissolvedOxygen = "DissolvedOxygen";
    public const string Temperature = "Temperature";
    public const string Conductivity = "Conductivity";

    public static readonly string[] All = { Ph, Turbidity, DissolvedOxygen, Temperature, Conductivity };

    public static readonly Dictionary<string, (double SafeMin, double SafeMax, string Unit)> ParameterRanges = new()
    {
        [Ph] = (6.5, 8.5, "pH"),
        [Turbidity] = (0, 5, "NTU"),
        [DissolvedOxygen] = (5, 12, "mg/L"),
        [Temperature] = (15, 30, "°C"),
        [Conductivity] = (100, 800, "µS/cm")
    };
}
