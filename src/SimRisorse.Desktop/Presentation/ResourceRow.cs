using System.Globalization;
using SimRisorse.Application.ReadModels;

namespace SimRisorse.Desktop.Presentation;

public sealed class ResourceRow(ResourceView view) : ObservableObject
{
    private ResourceView _view = view;
    private static readonly CultureInfo Italian = CultureInfo.GetCultureInfo("it-IT");
    public string Id => _view.Id;
    public string Name => _view.Name;
    public string Quantity => _view.Quantity.ToString("#,0.###", Italian);
    public string Capacity => _view.Capacity.ToString("#,0.###", Italian);
    public string Stock => $"{Quantity} / {Capacity}";
    public string NetRate => _view.NetPerSecond.ToString("+0.###;-0.###;±0", Italian) + " /s";
    public string Production => _view.ProductionPerSecond.ToString("0.###", Italian) + " /s";
    public string Consumption => _view.ConsumptionPerSecond.ToString("0.###", Italian) + " /s";
    public double FillPercent => _view.Capacity == 0 ? 0 : (double)(_view.Quantity / _view.Capacity * 100);
    public void Update(ResourceView updated) { _view = updated; Notify(string.Empty); }
}
