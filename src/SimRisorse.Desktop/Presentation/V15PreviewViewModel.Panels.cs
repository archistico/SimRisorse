using System.Globalization;
using SimRisorse.Core.Food;
using SimRisorse.Core.Research;

namespace SimRisorse.Desktop.Presentation;

/// <summary>
/// The validated v14 military/school panels are reused visually, while the
/// commands and observations below read only the v15 session/checkpoint.
/// </summary>
public sealed partial class V15PreviewViewModel
{
    public IReadOnlyList<int> BatchSizes { get; } = Array.AsReadOnly(new[] { 1, 5, 10 });
    public int MilitaryQuantity
    {
        get => _militaryQuantity;
        set { if (_militaryQuantity == value || !BatchSizes.Contains(value)) return;
            _militaryQuantity = value; RefreshParityPanels(); }
    }
    public int SchoolQuantity
    {
        get => _schoolQuantity;
        set { if (_schoolQuantity == value || !BatchSizes.Contains(value)) return;
            _schoolQuantity = value; RefreshParityPanels(); }
    }
    private int[] TrainingProgress => _session.Snapshot().CityCampaign.SchoolResearch.TrainingProgressSeconds.ToArray();
    public int SchoolCount => _session.Snapshot().CityCampaign.SchoolCount;
    public int SchoolWaiting => _session.Snapshot().CityCampaign.SchoolResearch.WaitingStudents;
    public int SchoolTraining => _session.Snapshot().CityCampaign.SchoolResearch.TrainingStudents;
    public int QualifiedResearchers => _session.Snapshot().CityCampaign.SchoolResearch.QualifiedResearchers;
    public int ActiveResearchers => _session.Snapshot().CityCampaign.SchoolResearch.ActiveResearchers;
    public string SchoolRoster => $"{Population} abitanti · {AvailableWorkers} liberi · " +
        $"{SchoolWaiting} studenti in coda · {SchoolTraining} in formazione · " +
        $"{QualifiedResearchers} ricercatori qualificati ({ActiveResearchers} attivi).";
    public string SchoolSeatsStatus => $"{SchoolCount} {(SchoolCount == 1 ? "Scuola" : "Scuole")} · " +
        $"{SchoolTraining}/{(long)SchoolCount * _school.TrainingSeatsPerSchool} posti formazione · " +
        $"{ActiveResearchers}/{(long)SchoolCount * _school.ResearcherPlacesPerSchool} posti ricerca.";
    public string SchoolSuppliesStatus =>
        $"{_session.Snapshot().CityCampaign.Resources[V14SchoolSupplies.ResourceId].Units.ToString("#,0.###", Italian)} " +
        $"Matite · per {SchoolQuantity} studenti servono {SchoolQuantity * _school.PencilsPerStudent} Matite e " +
        $"{(_school.FoodPerStudent.Units * SchoolQuantity).ToString("0.###", Italian)} Cibo dal Deposito.";
    public string SchoolTrainingHeading => $"FORMAZIONE · {_school.TrainingSeconds} SECONDI PER STUDENTE";
    public string SchoolWaitingLabel => SchoolWaiting.ToString(Italian);
    public string SchoolTrainingLabel => SchoolTraining.ToString(Italian);
    public string SchoolReadyLabel => QualifiedResearchers.ToString(Italian);
    public double SchoolTrainingPercent => TrainingProgress.Length == 0 ? 0d :
        Math.Clamp(100d * TrainingProgress[0] / _school.TrainingSeconds, 0d, 100d);
    public string SchoolTrainingStatus => TrainingProgress.Length == 0
        ? SchoolWaiting == 0 ? "Nessuno studente in formazione." : "Studenti in attesa di un posto libero."
        : $"{SchoolTraining} in formazione · primo ricercatore pronto tra " +
          TimeSpan.FromSeconds(_school.TrainingSeconds - TrainingProgress[0]).ToString(@"mm\:ss", Italian) + ".";
    public string SchoolTrainingDetails
    {
        get
        {
            var progress = TrainingProgress;
            var rows = progress.Take(6).Select((elapsed, index) =>
                $"Allievo {index + 1}: {elapsed}/{_school.TrainingSeconds} s · " +
                $"{TimeSpan.FromSeconds(_school.TrainingSeconds - elapsed).ToString(@"mm\:ss", Italian)} rimanenti").ToList();
            if (progress.Length > 6) rows.Add($"Altri {progress.Length - 6} allievi in formazione.");
            return string.Join(Environment.NewLine, rows);
        }
    }
    public string SchoolQueueExplanation => "Una Scuola forma un allievo alla volta; i qualificati non vengono assegnati automaticamente.";
    public string SchoolCraftStatus => "Laboratorio Matite: 1 Legno lavorato + 1 Carbone = 4 Matite.";
    public string SchoolCraftLabel => $"Produci +{V14SchoolSupplies.PencilsPerBatch * SchoolQuantity} Matite";
    public string SchoolCraftHint => CanChange && Trial().CraftPencils(SchoolQuantity) == PencilCraftFailure.None
        ? "Materiali e spazio sufficienti per la produzione." : "Verifica Scuola, Legno lavorato, Carbone e capacità Matite.";
    public string SchoolEnrollLabel => $"Iscrivi +{SchoolQuantity}";
    public string SchoolCancelLabel => $"Annulla −{SchoolQuantity}";
    public string SchoolAssignLabel => $"Assegna +{SchoolQuantity}";
    public string SchoolUnassignLabel => $"Sospendi −{SchoolQuantity}";
    public string SchoolReturnLabel => $"Torna civile −{SchoolQuantity}";
    public string SchoolEnrollmentHint => CanChange && Trial().EnrollStudents(SchoolQuantity) == SchoolActionFailure.None
        ? "Iscrizione disponibile; Cibo dal Deposito e Matite dalla città." :
        "Verifica posti, abitanti liberi, Cibo nel Deposito e Matite.";
    public string SchoolCancelHint => CanChange && Trial().CancelStudents(SchoolQuantity) == SchoolActionFailure.None
        ? "Annullamento disponibile con rimborso secondo le capienze." :
        "Verifica studenti annullabili e capienza per il rimborso.";
    public string SchoolResearchHint =>
        $"{ActiveResearchers} ricercatori attivi; {_school.MilliPointsPerResearcherPerSecond / 1000m:0.###} punti/s per ricercatore. " +
        "Gli acquisti delle tecnologie avvengono solo nell'Albero tecnologico.";

    public int WaitingRecruits => _session.Snapshot().CityCampaign.Military.WaitingRecruits;
    public int TrainingRecruits => _session.Snapshot().CityCampaign.Military.TrainingRecruits;
    public int AvailableSoldiers => _session.Snapshot().CityCampaign.Military.AvailableSoldiers;
    public int MilitaryBarracks => _session.Snapshot().CityCampaign.OtherBuildings.Single(b => b.BuildingId == "barracks").Count;
    public long MilitarySeats => (long)MilitaryBarracks * (_baseline.MilitaryRules?.SeatsPerBarracks ?? 0);
    public string MilitaryRoster => $"{Population} abitanti · {AvailableWorkers} liberi · " +
        $"{WaitingRecruits} reclute in coda · {TrainingRecruits} in addestramento · {AvailableSoldiers} soldati. " +
        "Gli altri abitanti sono occupati nelle filiere, nelle industrie o nella Scuola.";
    public string MilitaryCapacityStatus => $"{MilitaryBarracks} {(MilitaryBarracks == 1 ? "Caserma" : "Caserme")} · " +
        $"{TrainingRecruits}/{MilitarySeats} posti occupati · {Math.Max(0L, MilitarySeats - TrainingRecruits)} liberi";
    public string MilitarySeatsExplanation => "Una Caserma richiede la Forgia e addestra le reclute in posti limitati.";
    public string MilitaryWeaponsStatus => $"Spade disponibili: " +
        $"{_session.Snapshot().CityCampaign.Resources["swords"].Units.ToString("#,0.###", Italian)} · " +
        $"per +{MilitaryQuantity} reclute servono {MilitaryQuantity * (_baseline.MilitaryRules?.SwordsPerRecruit ?? 0)} Spade";
    public string MilitaryArmamentExplanation => "Le Spade vengono addebitate all'arruolamento e rimborsate se si annulla la recluta.";
    public string MilitaryTrainingHeading => $"FORMAZIONE · {_baseline.MilitaryRules?.TrainingSecondsPerGroup ?? 30} SECONDI PER RECLUTA";
    public string MilitaryQueueExplanation => "Le reclute eccedenti aspettano in coda e l'annullamento perde il tempo già svolto.";
    public string MilitaryTrainingStatus => TrainingRecruits == 0 ? "Nessuna recluta in addestramento." :
        $"{TrainingRecruits} reclute in addestramento · prima pronta tra " +
        TimeSpan.FromSeconds((_baseline.MilitaryRules?.TrainingSecondsPerGroup ?? 30) -
            _session.Snapshot().CityCampaign.Military.TrainingProgressSeconds).ToString(@"mm\:ss", Italian) + ".";
    public double MilitaryTrainingPercent => TrainingRecruits == 0 ? 0d : Math.Clamp(
        100d * _session.Snapshot().CityCampaign.Military.TrainingProgressSeconds /
        (_baseline.MilitaryRules?.TrainingSecondsPerGroup ?? 30), 0d, 100d);
    public string MilitaryLimitsNote => $"{AvailableWorkers} abitanti liberi · " +
        $"{WaitingRecruits + TrainingRecruits} reclute annullabili.";
    public string MilitaryRecruitmentHint => MilitaryBarracks == 0 ? "Costruisci prima una Caserma dalla scheda Edifici." :
        CanChange && Trial().Enlist(MilitaryQuantity).Succeeded ? "Arruolamento disponibile." :
        "Verifica abitanti liberi, Spade e posti in Caserma.";
    public string MilitaryCancelHint => CanChange && Trial().CancelRecruits(MilitaryQuantity).Succeeded
        ? "Puoi annullare le reclute e recuperare le Spade." :
        "Verifica reclute annullabili e spazio per le Spade rimborsate.";
    public string MilitaryDischargeHint => "Congedo rinviato fino all'introduzione delle missioni.";
    public string EnlistLabel => $"Arruola +{MilitaryQuantity}";
    public string CancelRecruitsLabel => $"Annulla reclute −{MilitaryQuantity}";
    public string DischargeLabel => $"Congeda −{MilitaryQuantity}";

    private void RefreshParityPanels()
    {
        string[] names =
        [
            nameof(ScenarioName), nameof(HasPopulationGrowth), nameof(HasAdvancedHousingRules),
            nameof(ShowBreadSupplyStatus), nameof(BatchSizes), nameof(MilitaryQuantity), nameof(SchoolQuantity),
            nameof(SchoolCount), nameof(SchoolRoster), nameof(SchoolSeatsStatus), nameof(SchoolSuppliesStatus),
            nameof(SchoolTrainingHeading), nameof(SchoolWaitingLabel), nameof(SchoolTrainingLabel),
            nameof(SchoolReadyLabel), nameof(SchoolTrainingPercent), nameof(SchoolTrainingStatus),
            nameof(SchoolTrainingDetails), nameof(SchoolQueueExplanation), nameof(SchoolCraftStatus),
            nameof(SchoolCraftLabel), nameof(SchoolCraftHint), nameof(SchoolEnrollLabel), nameof(SchoolCancelLabel),
            nameof(SchoolAssignLabel), nameof(SchoolUnassignLabel), nameof(SchoolReturnLabel),
            nameof(SchoolEnrollmentHint), nameof(SchoolCancelHint), nameof(SchoolResearchHint), nameof(ActiveResearchers),
            nameof(QualifiedResearchers), nameof(WaitingRecruits), nameof(TrainingRecruits),
            nameof(AvailableSoldiers), nameof(MilitaryBarracks), nameof(MilitaryRoster),
            nameof(MilitaryCapacityStatus), nameof(MilitarySeatsExplanation), nameof(MilitaryWeaponsStatus),
            nameof(MilitaryArmamentExplanation), nameof(MilitaryTrainingHeading),
            nameof(MilitaryQueueExplanation), nameof(MilitaryTrainingStatus),
            nameof(MilitaryTrainingPercent), nameof(MilitaryLimitsNote), nameof(MilitaryRecruitmentHint),
            nameof(MilitaryCancelHint), nameof(MilitaryDischargeHint), nameof(EnlistLabel),
            nameof(CancelRecruitsLabel), nameof(DischargeLabel)
        ];
        foreach (var name in names) Notify(name);
        EnlistCommand.Refresh(); CancelRecruitCommand.Refresh(); DischargeCommand.Refresh();
        EnrollCommand.Refresh(); CancelStudentCommand.Refresh(); CraftPencilsCommand.Refresh();
        AssignResearcherCommand.Refresh(); UnassignResearcherCommand.Refresh(); ReturnResearcherCommand.Refresh();
    }
}
