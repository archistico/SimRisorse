using SimRisorse.Core.Economy;
using SimRisorse.Core.Research;

namespace SimRisorse.Desktop.Presentation;

/// <summary>School panel reuses the individual-training progress already stored by v14.</summary>
public sealed partial class V14PreviewViewModel
{
    private int _schoolQuantity = 1;

    public int SchoolQuantity
    {
        get => _schoolQuantity;
        set
        {
            if (!BatchSizes.Contains(value) || _schoolQuantity == value) return;
            _schoolQuantity = value;
            RefreshSchoolPanel();
        }
    }

    private int SchoolWaiting => _session.Snapshot().SchoolResearch.WaitingStudents;
    private int SchoolTraining => _session.Snapshot().SchoolResearch.TrainingStudents;
    public int SchoolCancellable => checked(SchoolWaiting + SchoolTraining);
    public int SchoolAvailableResearchers => QualifiedResearchers - ActiveResearchers;
    public long SchoolTrainingSeats => (long)SchoolCount * _schoolRules.TrainingSeatsPerSchool;
    public long SchoolResearchSeats => (long)SchoolCount * _schoolRules.ResearcherPlacesPerSchool;
    public long SchoolFreeResearchPlaces => Math.Max(0, SchoolResearchSeats - ActiveResearchers);
    public decimal SchoolPencils => _session.Snapshot().Resources[V14SchoolSupplies.ResourceId].Units;
    public decimal SchoolPencilCapacity => V14SchoolSupplies.Capacity(_baseline, _session.Snapshot()).Units;
    public decimal SchoolFood => _session.Snapshot().Resources[_schoolRules.FoodResourceId].Units;
    public string SchoolRoster => $"{Population} abitanti · {AvailablePopulation} liberi · " +
        $"{SchoolWaiting} studenti in coda · {SchoolTraining} in formazione · " +
        $"{QualifiedResearchers} ricercatori qualificati ({ActiveResearchers} attivi).";
    public string SchoolSeatsStatus => $"{SchoolCount} {(SchoolCount == 1 ? "Scuola" : "Scuole")} · " +
        $"{SchoolTraining}/{SchoolTrainingSeats} posti formazione · " +
        $"{ActiveResearchers}/{SchoolResearchSeats} posti ricerca.";
    public string SchoolTrainingHeading => $"FORMAZIONE · {_schoolRules.TrainingSeconds} SECONDI PER STUDENTE";
    public string SchoolWaitingLabel => SchoolWaiting.ToString(Italian);
    public string SchoolTrainingLabel => SchoolTraining.ToString(Italian);
    public string SchoolReadyLabel => QualifiedResearchers.ToString(Italian);
    public double SchoolTrainingPercent => SchoolTraining == 0 ? 0d :
        100d * _session.Snapshot().SchoolResearch.TrainingProgressSeconds[0] / _schoolRules.TrainingSeconds;
    public string SchoolTrainingStatus => SchoolTraining == 0
        ? SchoolWaiting == 0 ? "Nessuno studente in formazione." : "Studenti in attesa di un posto libero."
        : $"{SchoolTraining} in formazione · primo ricercatore pronto tra " +
          TimeSpan.FromSeconds(_schoolRules.TrainingSeconds -
              _session.Snapshot().SchoolResearch.TrainingProgressSeconds[0]).ToString(@"mm\:ss", Italian) + ".";
    public string SchoolQueueExplanation => "Ogni Scuola forma un allievo alla volta. Gli altri aspettano; " +
        "gli studenti conclusi diventano ricercatori qualificati, non automaticamente attivi.";
    public string SchoolSuppliesStatus => $"{SchoolPencils:#,0} / {SchoolPencilCapacity:#,0} Matite · " +
        $"per iscrivere {SchoolQuantity} servono {_schoolRules.PencilsPerStudent * SchoolQuantity} Matite e " +
        $"{(_schoolRules.FoodPerStudent.Units * SchoolQuantity):#,0.###} Cibo.";
    public string SchoolCraftStatus => "Laboratorio della Scuola: 1 Legno lavorato + 1 Carbone = 4 Matite. " +
        "Il Municipio, i Magazzini e le Scuole conservano le Matite.";
    public string SchoolCraftHint => SchoolCount == 0 ? "Costruisci prima una Scuola."
        : _session.Snapshot().Resources["planks"].Units < SchoolQuantity
            ? "Legno lavorato insufficiente: assegna lavoratori alla Segheria."
        : _session.Snapshot().Resources["coal"].Units < SchoolQuantity
            ? "Carbone insufficiente: costruisci una Miniera di carbone e assegna lavoratori."
        : !SchoolCanCraft ? "Spazio Matite insufficiente: costruisci un Magazzino."
        : "Materiali e spazio disponibili: puoi produrre le Matite.";
    public string SchoolEnrollLabel => $"Iscrivi +{SchoolQuantity}";
    public string SchoolCancelLabel => $"Annulla −{SchoolQuantity}";
    public string SchoolAssignLabel => $"Assegna +{SchoolQuantity}";
    public string SchoolUnassignLabel => $"Sospendi −{SchoolQuantity}";
    public string SchoolReturnLabel => $"Torna civile −{SchoolQuantity}";
    public string SchoolCraftLabel => $"Produci +{V14SchoolSupplies.PencilsPerBatch * SchoolQuantity} Matite";

    private ResourceBundle SchoolEnrollmentCost => _schoolRules.TrainingCost(SchoolQuantity);
    public bool SchoolCanAffordEnrollment => _session.Snapshot().Resources.Covers(SchoolEnrollmentCost);
    public bool SchoolCanRefund
    {
        get
        {
            try
            {
                var refund = ResourceBundle.Create((_schoolRules.FoodResourceId,
                    _schoolRules.FoodPerStudent.Units * SchoolQuantity));
                var eligible = Math.Min(SchoolQuantity, _session.Snapshot().PencilPaidStudents);
                if (_schoolRules.PencilsPerStudent > 0 && eligible > 0)
                    refund += ResourceBundle.Create((V14SchoolSupplies.ResourceId,
                        (decimal)_schoolRules.PencilsPerStudent * eligible));
                return CalculateStorageCapacity(_session.Snapshot()).Covers(_session.Snapshot().Resources + refund);
            }
            catch (OverflowException) { return false; }
        }
    }
    public bool SchoolCanCraft
    {
        get
        {
            if (SchoolCount == 0 || !_session.Snapshot().Resources.Covers(V14SchoolSupplies.Inputs(SchoolQuantity)))
                return false;
            return SchoolPencils + (decimal)V14SchoolSupplies.PencilsPerBatch * SchoolQuantity <= SchoolPencilCapacity;
        }
    }
    public string SchoolEnrollmentHint => SchoolCount == 0
        ? "Costruisci una Scuola dalla schermata Edifici."
        : AvailablePopulation < SchoolQuantity ? "Abitanti liberi insufficienti."
        : SchoolFood < _schoolRules.FoodPerStudent.Units * SchoolQuantity ? "Cibo insufficiente per le iscrizioni."
        : SchoolPencils < (decimal)_schoolRules.PencilsPerStudent * SchoolQuantity
            ? "Matite insufficienti: produci Matite nel laboratorio della Scuola."
        : "L'iscrizione paga subito Cibo e Matite e riserva gli abitanti.";
    public string SchoolCancelHint => SchoolCancellable < SchoolQuantity
        ? "Studenti da annullare insufficienti."
        : !SchoolCanRefund ? "Magazzino pieno: non c'è spazio per restituire Cibo e Matite."
        : "Annulla prima chi è in attesa, poi chi ha iniziato da meno. Restituisce Cibo e le Matite pagate (non quelle dei vecchi salvataggi).";
    public string SchoolResearchHint => SchoolFreeResearchPlaces == 0
        ? "Posti ricerca occupati o nessuna Scuola costruita."
        : SchoolAvailableResearchers < SchoolQuantity ? "Non ci sono abbastanza ricercatori qualificati liberi."
        : "Assegna i ricercatori qualificati per accumulare punti ricerca.";

    private void RefreshSchoolPanel()
    {
        string[] properties =
        [
            nameof(SchoolQuantity), nameof(SchoolRoster), nameof(SchoolSeatsStatus),
            nameof(SchoolTrainingHeading), nameof(SchoolWaitingLabel), nameof(SchoolTrainingLabel),
            nameof(SchoolReadyLabel), nameof(SchoolTrainingPercent), nameof(SchoolTrainingStatus),
            nameof(SchoolQueueExplanation), nameof(SchoolSuppliesStatus), nameof(SchoolCraftStatus), nameof(SchoolCraftHint),
            nameof(SchoolEnrollLabel), nameof(SchoolCancelLabel), nameof(SchoolAssignLabel),
            nameof(SchoolUnassignLabel), nameof(SchoolReturnLabel), nameof(SchoolCraftLabel),
            nameof(SchoolCancellable), nameof(SchoolAvailableResearchers), nameof(SchoolTrainingSeats),
            nameof(SchoolResearchSeats), nameof(SchoolFreeResearchPlaces), nameof(SchoolPencils),
            nameof(SchoolPencilCapacity), nameof(SchoolFood), nameof(SchoolCanAffordEnrollment),
            nameof(SchoolCanRefund), nameof(SchoolCanCraft), nameof(SchoolEnrollmentHint),
            nameof(SchoolCancelHint), nameof(SchoolResearchHint)
        ];
        foreach (var property in properties) Notify(property);
        EnrollCommand.Refresh(); CancelStudentCommand.Refresh(); AssignResearcherCommand.Refresh();
        UnassignResearcherCommand.Refresh(); ReturnResearcherCommand.Refresh(); CraftPencilsCommand.Refresh();
    }
}
