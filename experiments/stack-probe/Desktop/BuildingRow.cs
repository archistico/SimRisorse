using SimRisorse.StackProbe.Model;

namespace SimRisorse.StackProbe.Desktop;

public sealed class BuildingRow(BuildingDefinition definition) : ObservableObject
{
    private BuildingState _state = new(definition.Kind, 0, 0);
    private int _runnable;
    private ResourceAmounts _resources;

    public BuildingKind Kind => definition.Kind;
    public string Name => definition.Name;
    public string Initials => Kind switch { BuildingKind.TreeFeller => "TL", BuildingKind.Sawmill => "SG", _ => "TP" };
    public int Count => _state.Count;
    public int Workers => _state.Workers;
    public string Staff => $"{Workers} / {Count}";
    public string Cost => $"{definition.Cost.Planks} legno · {definition.Cost.Stone} pietra";
    public string Description => Kind switch
    {
        BuildingKind.TreeFeller => "Il primo anello della filiera. Fornisce i tronchi necessari alle segherie.",
        BuildingKind.Sawmill => "Trasforma i tronchi in legno lavorato per costruire e ampliare la colonia.",
        _ => "Estrae la pietra necessaria a nuove costruzioni e investimenti."
    };
    public string Recipe => Kind switch
    {
        BuildingKind.TreeFeller => "+2 tronchi / secondo",
        BuildingKind.Sawmill => "2 tronchi → 1 legno / secondo",
        _ => "+1 pietra / secondo"
    };
    public string Status => Workers == 0 ? "Nessun lavoratore" : _runnable == Workers ? "Produzione disponibile" :
        Kind == BuildingKind.Sawmill && _resources.Logs < 2L * Workers ? "Tronchi insufficienti" : "Deposito pieno";
    public string Activity => $"{_runnable} di {Workers} lavoratori possono produrre al prossimo passo.";

    public void Update(ProbeSession session)
    {
        _state = session.GetBuilding(Kind);
        _resources = session.Resources;
        _runnable = session.RunnableWorkers(Kind);
        Notify(string.Empty);
    }
}
