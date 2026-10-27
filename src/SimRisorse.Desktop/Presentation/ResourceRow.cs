using System.Globalization;
using Avalonia.Media;
using SimRisorse.Application.ReadModels;

namespace SimRisorse.Desktop.Presentation;

public sealed class ResourceRow(ResourceView view) : ObservableObject
{
    private ResourceView _view = view;
    private static readonly CultureInfo Italian = CultureInfo.GetCultureInfo("it-IT");
    private static readonly IBrush SurplusBrush = new SolidColorBrush(Color.Parse("#176858"));
    private static readonly IBrush DeficitBrush = new SolidColorBrush(Color.Parse("#A44732"));
    public string Id => _view.Id;
    public string Name => _view.Name;
    public string Quantity => _view.Quantity.ToString("#,0.###", Italian);
    public string QuantityRounded => decimal.Round(_view.Quantity, 0, MidpointRounding.AwayFromZero).ToString("#,0", Italian);
    public string Capacity => _view.Capacity.ToString("#,0.###", Italian);
    public string Stock => $"{Quantity} / {Capacity}";
    public string NetRate => _view.NetPerSecond.ToString("+0.###;-0.###;±0", Italian) + " /s";
    public IBrush NetForeground => _view.NetPerSecond < 0 ? DeficitBrush : SurplusBrush;
    public string Production => _view.ProductionPerSecond.ToString("0.###", Italian) + " /s";
    public string Consumption => _view.ConsumptionPerSecond.ToString("0.###", Italian) + " /s";
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
    public void Update(ResourceView updated) { _view = updated; Notify(string.Empty); }
}
