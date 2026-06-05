namespace ALGarage.Domain.Common;

public enum FuelType
{
    Unknown = 0,
    Gasoline,
    Ethanol,
    Flex,
    Diesel,
    Hybrid,
    PluginHybrid,
    Electric
}

public enum Aspiration
{
    Unknown = 0,
    Naturally,
    Turbo,
    Supercharged,
    TwinCharged
}

/// <summary>Estado de um item de manutenção para um veículo. <c>DueSoon</c> é o "vermelho" no 2D/3D.</summary>
public enum MaintenanceState
{
    Ok = 0,
    DueSoon,
    Overdue
}

public enum UpgradeType
{
    Performance = 0,
    Aesthetic
}

/// <summary>De onde veio um dado de spec/decode. Habilita proveniência e resolução de conflitos.</summary>
public enum DataSource
{
    Unknown = 0,
    Vpic,
    Commercial,
    Curated,
    User
}
