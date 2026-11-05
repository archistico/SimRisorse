using System.Globalization;
using System.Text;
using SimRisorse.Core.Construction;
using SimRisorse.Core.Economy;

namespace SimRisorse.Application.Benchmarks;

public static class E4bCurveStudyExport
{
    private static string N(decimal value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    public static string Materials(ResourceBundle bundle) => string.Join(" + ",
        bundle.Amounts.Select(kv => $"{N(kv.Value.Units)} {kv.Key}"));

    public static string QuotesCsv(E4bCurveStudyReport report)
    {
        var text = new StringBuilder("curve,family,tier,epoch,depth,purchase,resource,units,batch_storage_route\n");
        foreach (var row in report.Quotes)
            foreach (var (label, cost) in new[] { ("first", row.First), ("second", row.Second),
                ("batch-five", row.BatchFive), ("batch-ten", row.BatchTen) })
                foreach (var (resource, value) in cost.Amounts)
                    text.AppendLine($"{row.CurveId},{row.FamilyId},{row.Tier},{row.Epoch},{row.DependencyDepth},{label},{resource},{N(value.Units)},{(label == "batch-five" ? row.BatchFiveLiquidity.Status : label == "batch-ten" ? row.BatchTenLiquidity.Status : "not-a-batch")}");
        return text.ToString();
    }

    public static string InvestmentsCsv(E4bCurveStudyReport report)
    {
        var text = new StringBuilder("curve,mints,tier,scope,resource,units,storage_route,warehouses,residents,mint_workers,reference_material_worker_seconds,stonecutter_work_equivalents,research_points\n");
        foreach (var row in report.Investments)
            foreach (var (scope, cost) in new[] { ("construction-without-warehouses", row.ConstructionWithoutWarehouses),
                ("construction-with-warehouses", row.ConstructionWithWarehouses) })
            {
                if (cost is null) continue; // a blocked route is never represented as zero cost
                foreach (var (resource, value) in cost.Amounts)
                    text.AppendLine($"{row.CurveId},{row.MintBuildings},{row.Tier},{scope},{resource},{N(value.Units)},{row.Liquidity.Status},{row.Liquidity.Warehouses},{row.Workforce.RequiredResidents},{row.Workforce.MintWorkers},{N(row.ConstructionWorkerSeconds)},{N(row.StonecutterWorkEquivalents)},{N(row.RequiredResearchPoints)}");
            }
        return text.ToString();
    }

    public static string ToMarkdown(E4bCurveStudyReport report)
    {
        var text = new StringBuilder("# RC78 / E4b.1 — confronto numerico delle proposte\n\n");
        text.AppendLine($"Versione `{report.Version}`. Modalità `{report.Mode}`. Nessun coefficiente installato nel gioco.");
        text.AppendLine("\n## Parametri e metodo\n");
        text.AppendLine("| Curva | Criterio | Base progressione | Rincaro copie | Grande | Avanzato |\n|---|---|---:|---:|---:|---:|");
        foreach (var c in report.Curves)
            text.AppendLine($"| {c.Id} | {c.Kind} | {N(c.ProgressionBase)} | {N(c.CopyGrowth)} | {N(c.LargeMultiplier)} | {N(c.AdvancedMultiplier)} |");
        text.AppendLine("\nIngresso = ceil(costo corrente × base^indice), poi ceil(ingresso × tier × copie^n) per risorsa e edificio. Indice A/C: epoca (0 avvio, 1 pane/case grandi/Scuola/depositi alimentari, 2 miniere, 3 fonderie, 4 Forgia/Zecca); B: profondità massima dei prerequisiti espliciti (Zecca 6). B aggiunge 40 Ferro/20 Oro alla Zecca e 40 Ferro alla Forgia; C aggiunge 120 Ferro/80 Oro e 80 Ferro rispettivamente. Anche questi materiali scalano con tier e copie. Magazzini: epoca/profondità 0.");
        text.AppendLine("\n## Riferimento osservato RC77-M01\n");
        if (report.MeasuredCurrentBaseline is null)
            text.AppendLine("Nessun file di misure importato. Tempi di completamento delle proposte non misurati.");
        else
            text.AppendLine(V15IndustrialProgressionAnalysis.ToMarkdown(report.MeasuredCurrentBaseline));
        text.AppendLine("\nI tempi importati appartengono alle regole correnti, con Zecca da un addetto. Non stimano le proposte. Ricerca proposta mantenuta a 350 punti per Base/Grande e 800 per Avanzato: 3500/8000 secondi-lavoratore teorici a 0,1 punti/s, esclusa formazione. L'osservazione corrente comprende 3540s di attesa ricerca e 120s formazione. Materiali per iscrizione/crafting e consumo continuo di cibo/ricette sono separati dai preventivi edilizi qui sotto.");
        text.AppendLine("\n## Preventivi diretti\n\nOgni lotto è il pagamento atomico completo, somma dei prezzi arrotondati delle copie. I preventivi non attestano che un lotto sia finanziabile.\n");
        text.AppendLine("| Curva | Famiglia | Tier | Prima | Seconda | Lotto 5 | Lotto 10 |\n|---|---|---|---|---|---|---|");
        foreach (var row in report.Quotes)
            text.AppendLine($"| {row.CurveId} | {row.FamilyId} | {row.Tier} | {Materials(row.First)} | {Materials(row.Second)} | {Materials(row.BatchFive)} | {Materials(row.BatchTen)} |");
        text.AppendLine("\n## Liquidità lotti Zecca Base\n\n| Curva | Lotto 5 | Lotto 10 |\n|---|---|---|");
        foreach (var row in report.Quotes.Where(r => r.FamilyId == "mint" && r.Tier == ProductionTier.Base))
            text.AppendLine($"| {row.CurveId} | {row.BatchFiveLiquidity.Status} ({row.BatchFiveLiquidity.Warehouses} Magazzini) | {row.BatchTenLiquidity.Status} ({row.BatchTenLiquidity.Warehouses} Magazzini) |");
        text.AppendLine("\n## Filiera completa, acquisti singoli\n");
        text.AppendLine("| Curva | Zecche | Tier | Edilizia senza Magazzini | Edilizia con Magazzini | Magazzini | Esito percorso | Abitanti / addetti Zecca | Lavoro materiali / equivalenti Tagliapietre |\n|---|---:|---|---|---|---:|---|---|---|");
        foreach (var row in report.Investments)
            text.AppendLine($"| {row.CurveId} | {row.MintBuildings} | {row.Tier} | {Materials(row.ConstructionWithoutWarehouses)} | {(row.ConstructionWithWarehouses is null ? "non finanziabile nel percorso" : Materials(row.ConstructionWithWarehouses))} | {row.Liquidity.Warehouses} | {row.Liquidity.Status} | {row.Workforce.RequiredResidents} / {row.Workforce.MintWorkers} | {N(row.ConstructionWorkerSeconds)} s-lav / {N(row.StonecutterWorkEquivalents)} |");
        text.AppendLine("\nLa chiusura dei prerequisiti conta una sola volta le strutture condivise e sottrae quelle iniziali. Include due Case piccole, almeno una grande, Scuola, cibo, pane, miniere e fonderie necessarie, con quantità dimensionate sul personale. I produttori avanzati richiesti per pagare i materiali precedono la Zecca; rilevati cicli materiali/prerequisiti mediante visita del grafo. Tutti gli edifici di supporto sono Base. Non include un'Esercito o Forgia non necessari alla Zecca.");
        text.AppendLine("\nLavoro materiali: somma ricorsiva dei secondi-lavoratore nominali delle ricette correnti per produrre i materiali edilizi lordi, senza Magazzini. Include input dei metalli e dei lavorati, esclude scorte iniziali, ricerca e consumo continuo. Equivalente Tagliapietre = lavoro materiali / lavoro per 30 tavole + 10 pietre del riferimento immutato. È una misura di lavoro, non una valuta né tempo di partita. JSON espone anche i due rapporti separati tavole/30 e pietra/10: non sono sommati.");
        text.AppendLine("\nIl percorso di liquidità parte dalla capacità iniziale, costruisce Magazzini Base uno alla volta pagando i rincari, e controlla ogni pagamento prima di aggiungere 500 posti per risorsa. Assume disponibilità rinnovabile dei materiali, ignora spazio accessorio dei produttori, tempi, upgrade dei Magazzini e altre strategie. Il massimo componente per risorsa dei pagamenti singoli è un inviluppo di capacità, non un unico addebito. Un blocco di questo percorso non prova uno stallo globale. I lotti richiedono l'intera somma disponibile: vanno controllati separatamente con AuditWarehouseRoute.");
        text.AppendLine("\n## Potenziamenti della prima Zecca\n");
        text.AppendLine("| Curva | Avanzata diretta | Base→Grande→Avanzata pagato | Massimo pagamento del percorso | Diretto / upgrade | No sconto |\n|---|---|---|---|---|---|");
        foreach (var row in report.Upgrades)
            text.AppendLine($"| {row.CurveId} | {Materials(row.DirectAdvanced)} | {Materials(row.PaidViaBaseLargeAdvanced)} | {Materials(row.LargestUpgradePayment)} | {row.DirectLiquidity.Status} / {row.UpgradeLiquidity.Status} | {row.SameCounterTelescopes && row.NextCounterPathNoCheaper} |");
        text.AppendLine("\nIl primo acquisto porta lo storico da 0 a 1; gli upgrade conservano 1. Le differenze allo stesso storico telescopizzano; il percorso reale qui costa almeno quanto l'Avanzata diretta a storico 0. Non è una prova di assenza di arbitraggio per ogni futura migrazione/demolizione.");
        text.AppendLine("\n## Personale: ipotesi analitica, non planner attivo\n");
        text.AppendLine("Squadra Zecca di 10; moduli candidati 1/3/10 per Base/Grande/Avanzato. 0–9 addetti: zero produzione e consumo; due Base con 10–19: un modulo, con 20: due. Un modulo completo consuma 1 Oro + 1 Carbone e produce 3 Monete/s solo con input e spazio. Gli addetti parziali/inattivi restano riservati. Input e spazio sono limiti congiunti; nessun consumo per moduli non eseguibili.");
        text.AppendLine("\nSupporto per modulo: un addetto Miniera Oro, uno Fonderia Oro e uno Miniera Carbone (2 Carbone/s per fonderia e Zecca). Quattro persone per tre produttori di avvio e ricerca. Cibo a 0,01 per abitante/s; pane a 0,015 per abitante in Casa grande/s, dopo i 16 posti delle due Case piccole. Dimensionamento iterativo del personale agricolo e alimentare; alloggi da 48 posti iniziali del percorso più Case grandi da 32. È un budget a regime e popolazione fissa, senza buffer di personale: non dimostra un avvio atomico sostenibile, crescita, assegnazioni reali o salvataggio/ripresa. Questi controlli richiedono la futura implementazione versionata.");
        return text.ToString();
    }
}
