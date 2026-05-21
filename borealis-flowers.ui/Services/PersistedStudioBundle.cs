using borealis_flowers.ui.Models;

namespace borealis_flowers.ui.Services;

/// <summary>То, что сохраняем в localStorage.</summary>
public sealed class PersistedStudioBundle
{
    public List<StudioOrderDto> Orders { get; set; } = [];
    public List<WarehouseItemDto> Warehouse { get; set; } = [];
    public List<SupplyRequestDto> SupplyRequests { get; set; } = [];
}
