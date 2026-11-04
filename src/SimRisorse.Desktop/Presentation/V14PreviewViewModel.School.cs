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
    // Read-only progress of the individual trainees. Keep the existing first-finisher
    // bar for continuity, while exposing the other occupied seats in the Scuola panel.
    // The view uses snapshot state only; neither queue order nor game rules change.
    public string SchoolTrainingDetails
    {
        get
        {
            var progress = _session.Snapshot().SchoolResearch.TrainingProgressSeconds;
            if (progress.IsEmpty) return string.Empty;
            const int visibleSeats = 6;
            var displayed = progress.Take(visibleSeats).Select((elapsed, index) =>
                $"Allievo {index + 1}: {elapsed}/{_schoolRules.TrainingSeconds} s · " +
                $"{TimeSpan.FromSeconds(_schoolRules.TrainingSeconds - elapsed).ToString(@"mm\:ss", Italian)} rimanenti");
            var lines = displayed.ToList();
            if (progress.Length > visibleSeats)
                lines.Add($"Altri {progress.Length - visibleSeats} allievi in formazione.");
            return string.Join(Environment.NewLine, lines);
        }
    }
    public string SchoolQueueExplanation => "Ogni Scuola forma un allievo alla volta. Gli altri aspettano; " +
        "gli studenti conclusi diventano ricercatori qualificati, non automaticamente attivi.";
    public string SchoolSuppliesStatus => $"{SchoolPencils:#,0} / {SchoolPencilCapacity:#,0} Matite · " +
        $"per iscrivere {SchoolQuantity} servono {_schoolRules.PencilsPerStudent * SchoolQuantity} Matite e " +
        $"{(_schoolRules.FoodPerStudent.Units * SchoolQuantity):#,0.###} Cibo.";
    public string SchoolCraftStatus => "Laboratorio della Scuola: 1 Legno lavorato + 1 Carbone = 4 Matite. " +
        "Il Municipio, i Magazzini e le Scuole conservano le Matite.";
    public string SchoolCraftHint
    {
        get
        {
            if (!CanMutateColony)
                return "Produzione Matite sospesa: conferma o annulla la nuova partita.";
            var failure = _session.CheckPencilCraft(SchoolQuantity);
            if (failure == PencilCraftFailure.MissingSchool)
                return "Costruisci prima una Scuola.";
            if (failure == PencilCraftFailure.InsufficientMaterials)
            {
                var stock = _session.Snapshot().Resources;
                var missing = new List<string>();
                var planks = stock["planks"].Units;
                var coal = stock["coal"].Units;
                if (planks < SchoolQuantity)
                    missing.Add($"Legno lavorato insufficiente: {FormatSchoolSupply(planks)}/{SchoolQuantity} " +
                        $"(mancano {FormatSchoolSupply(SchoolQuantity - planks)})");
                if (coal < SchoolQuantity)
                    missing.Add($"Carbone insufficiente: {FormatSchoolSupply(coal)}/{SchoolQuantity} " +
                        $"(mancano {FormatSchoolSupply(SchoolQuantity - coal)})");
                return string.Join("; ", missing) + ".";
            }
            if (failure == PencilCraftFailure.InsufficientStorage)
            {
                var needed = (decimal)V14SchoolSupplies.PencilsPerBatch * SchoolQuantity;
                var available = Math.Max(0m, SchoolPencilCapacity - SchoolPencils);
                return $"Spazio Matite insufficiente: {FormatSchoolSupply(available)} libero, " +
                    $"{FormatSchoolSupply(needed)} necessario per +{SchoolQuantity} lotti. " +
                    "Costruisci un Magazzino o usa le Matite disponibili.";
            }
            if (failure != PencilCraftFailure.None)
                return "Impossibile produrre Matite: quantità non valida o limite numerico.";
            return $"Puoi produrre {V14SchoolSupplies.PencilsPerBatch * SchoolQuantity} Matite " +
                $"con {SchoolQuantity} Legno lavorato e {SchoolQuantity} Carbone; " +
                $"spazio disponibile {FormatSchoolSupply(SchoolPencilCapacity - SchoolPencils)}.";
        }
    }

    private static string FormatSchoolSupply(decimal quantity) => quantity.ToString("#,0.###", Italian);

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
    public bool SchoolCanCraft => _session.CheckPencilCraft(SchoolQuantity) == PencilCraftFailure.None;
    public string SchoolEnrollmentHint => !CanMutateColony
        ? "Iscrizioni sospese: conferma o annulla la nuova partita."
        : SchoolCount == 0
        ? "Costruisci una Scuola dalla schermata Edifici."
        : AvailablePopulation < SchoolQuantity ? "Abitanti liberi insufficienti."
        : SchoolFood < _schoolRules.FoodPerStudent.Units * SchoolQuantity ? "Cibo insufficiente per le iscrizioni."
        : SchoolPencils < (decimal)_schoolRules.PencilsPerStudent * SchoolQuantity
            ? "Matite insufficienti: produci Matite nel laboratorio della Scuola."
        : "L'iscrizione paga subito Cibo e Matite e riserva gli abitanti.";
    public string SchoolCancelHint => !CanMutateColony
        ? "Annullamento studenti sospeso: conferma o annulla la nuova partita."
        : SchoolCancellable < SchoolQuantity
        ? "Studenti da annullare insufficienti."
        : !SchoolCanRefund ? "Magazzino pieno: non c'è spazio per restituire Cibo e Matite."
        : "Annulla prima chi è in attesa, poi chi ha iniziato da meno. Restituisce Cibo e le Matite pagate (non quelle dei vecchi salvataggi).";
    // The message must use the same batch quantity as AssignResearcherCommand.
    // A nonzero number of free seats is not sufficient for a +5/+10 order.
    public string SchoolResearchHint => !CanMutateColony
        ? "Completa o annulla la conferma di nuova partita prima di assegnare ricercatori."
        : SchoolCount == 0
        ? "Costruisci una Scuola dalla schermata Edifici per avere posti di ricerca."
        : SchoolFreeResearchPlaces < SchoolQuantity
            ? $"Posti ricerca insufficienti per +{SchoolQuantity}: " +
              $"disponibili {SchoolFreeResearchPlaces}. Costruisci una Scuola o sospendi ricercatori."
        : SchoolAvailableResearchers < SchoolQuantity
            ? $"Ricercatori qualificati liberi insufficienti per +{SchoolQuantity}: " +
              $"disponibili {SchoolAvailableResearchers}."
        : "Assegna i ricercatori qualificati per accumulare punti ricerca.";

    private void RefreshSchoolPanel()
    {
        string[] properties =
        [
            nameof(SchoolQuantity), nameof(SchoolRoster), nameof(SchoolSeatsStatus),
            nameof(SchoolTrainingHeading), nameof(SchoolWaitingLabel), nameof(SchoolTrainingLabel),
            nameof(SchoolReadyLabel), nameof(SchoolTrainingPercent), nameof(SchoolTrainingStatus),
            nameof(SchoolTrainingDetails),
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
