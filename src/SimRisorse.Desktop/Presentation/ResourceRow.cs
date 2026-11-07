using System.Globalization;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using SimRisorse.Application.ReadModels;
using SimRisorse.Core.Economy;

namespace SimRisorse.Desktop.Presentation;

public sealed class ResourceRow(ResourceView view) : ObservableObject
{
    private ResourceView _view = view;
    private string _capacityBreakdown = string.Empty;
    private IReadOnlyList<ResourceFlowContributor>? _flowContributors;
    private static readonly CultureInfo Italian = CultureInfo.GetCultureInfo("it-IT");
    // I colori sono condivisi tra le righe, ma i test headless ricreano il
    // Dispatcher per ogni caso: SolidColorBrush e' legato al thread di creazione.
    // Un brush immutabile puo' invece essere riutilizzato tra dispatcher distinti.
    private static readonly IImmutableBrush SurplusBrush = new ImmutableSolidColorBrush(Color.Parse("#176858"));
    private static readonly IImmutableBrush DeficitBrush = new ImmutableSolidColorBrush(Color.Parse("#A44732"));
    public string Id => _view.Id;
    public string Name => _view.Name;
    public string Quantity => _view.Quantity.ToString("#,0.###", Italian);
    public string QuantityRounded => decimal.Round(_view.Quantity, 0, MidpointRounding.AwayFromZero).ToString("#,0", Italian);
    public string Capacity => _view.Capacity.ToString("#,0.###", Italian);
    public string CapacityBreakdown => _capacityBreakdown;
    public string Stock => $"{Quantity} / {Capacity}";
    public string NetRate => _view.NetPerSecond.ToString("+0.###;-0.###;±0", Italian) + " /s";
    public IBrush NetForeground => _view.NetPerSecond < 0 ? DeficitBrush : SurplusBrush;
    public string Production => _view.ProductionPerSecond.ToString("0.###", Italian) + " /s";
    public string Consumption => _view.ConsumptionPerSecond.ToString("0.###", Italian) + " /s";
    public string ProductionBreakdown => ResourceFlowBreakdown.Format(Name, Id,
        _view.ProductionPerSecond, true, _flowContributors);
    public string ConsumptionBreakdown => ResourceFlowBreakdown.Format(Name, Id,
        _view.ConsumptionPerSecond, false, _flowContributors);
    public string TimeToFull
    {
        get
        {
            if (_view.Capacity <= 0m) return "—";
            if (_view.Quantity >= _view.Capacity) return "00:00:00";
            if (_view.NetPerSecond <= 0m) return "—";

            // Stima al saldo previsto del prossimo secondo, non una simulazione della filiera futura.
            var seconds = decimal.Ceiling((_view.Capacity - _view.Quantity) / _view.NetPerSecond);
            var hours = decimal.Floor(seconds / 3600m);
            var minutes = decimal.Floor((seconds % 3600m) / 60m);
            var remainingSeconds = seconds % 60m;
            return $"{hours:00}:{minutes:00}:{remainingSeconds:00}";
        }
    }
    public double FillPercent => _view.Capacity == 0 ? 0 : (double)(_view.Quantity / _view.Capacity * 100);
    public void Update(ResourceView updated, string? capacityBreakdown = null,
        IReadOnlyList<ResourceFlowContributor>? flowContributors = null)
    {
        _view = updated;
        _flowContributors = flowContributors;
        if (capacityBreakdown is not null) _capacityBreakdown = capacityBreakdown;
        Notify(string.Empty);
    }
}
