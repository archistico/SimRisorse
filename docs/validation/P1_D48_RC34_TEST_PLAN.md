# Piano di validazione RC34 / P1-D48

**Stato:** Windows/.NET 10 non eseguito qui. Non dichiarare test superati prima della verifica dell'utente.

## Gate automatico cumulativo

Eseguire nella cartella RC34:

```powershell
.\validate.cmd
```

**Atteso: 533 test** (524 della RC33 + 9 nuovi), tutti superati; nessun errore di compilazione.

Nuovi test da controllare nel TRX:

- `TechnologyTierEffectTests.AllPurchasableTechnologiesHaveExactlyThePromisedImplementedBenefits`
- `TechnologyTierEffectTests.PurchasingAResearchWithoutActualCoverageIsRejectedAtCatalogGate`
- `TechnologyTierEffectTests.UnimplementedTechnologyCannotBeMadePurchasableWithNoBenefit`
- `TechnologyTierEffectTests.AdvancedResearchMustDependOnTheCorrespondingLargeTierResearch`
- `V14TechnologyBenefitGateTests.StartingAV14SessionRejectsPurchasableResearchThatCannotUnlockAnyTier`
- `V14PreviewTests.TechnologyDetailsShowTheRealBuildingTierBenefit` (3 esecuzioni)
- `V14PreviewTests.DeferredResearchShowsNoImplementedBenefitAndRemainsUnpurchasable`

## Gate manuale (copia del salvataggio v14)

1. `run-game.cmd` deve aprire la v14; la v13 resta accessibile con `--legacy-v13`, su slot separato.
2. In **Albero tecnologico**, selezionare Lavorazioni I: nel dettaglio devono comparire il costo, i prerequisiti e i tre edifici in versione **Grande**. Lavorazioni II deve indicare **Avanzato** e richiedere la prima.
3. Selezionare Alimentazione I/II e Industria I/II: i benefici devono indicare le sei/quattro famiglie attese e non livelli errati.
4. Con pochi punti il pulsante `Acquista ricerca` resta grigio, anche se il display a due decimali arrotonda il saldo all'importo visibile.
5. Con saldo e prerequisiti reali sufficienti, effettuare un acquisto e costruire o potenziare almeno un edificio sbloccato; l'azione deve applicarsi realmente e consumare i soli costi previsti.
6. Logistica I/II e Navigazione devono indicare **Effetto non implementato** e restare non acquistabili, anche con molti punti.
7. Salvare e riaprire: saldo e ricerche sbloccate devono essere identici; nessuna ricerca acquisita automaticamente; livelli degli edifici invariati.
8. Controllare a **1040×740** e **1240×850** che il nuovo riepilogo sia leggibile nello `ScrollViewer`, senza comprimere o nascondere il pulsante d'acquisto.

## Rischi da monitorare

Il nuovo controllo di coerenza rifiuta intenzionalmente cataloghi tecnologici personalizzati che dichiarino ricerche acquistabili senza un tier reale, o con prerequisiti incoerenti. Questo non modifica i salvataggi ufficiali v14; segnalare eventuali eccezioni di caricamento insieme al catalogo usato. I 533 test attesi non sono una conferma di esito.

[Rapporto D48](../design/P1_D48_RC34_ALBERO_TECNOLOGICO.md) · [Roadmap](../ROADMAP.md)
