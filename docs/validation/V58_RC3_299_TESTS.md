# v58-RC3 — diagnosi e correzioni dopo il primo gate Windows

**Data:** 3 novembre 2026. **Stato:** candidato RC3 pronto per nuovo `validate.cmd` su Windows. Nessuna esecuzione .NET in ambiente di preparazione.

## Evidenza riportata dall'utente (v58-RC2)

- Compilazione della soluzione arrivata alla fase test, senza gli errori CS0136 precedenti.
- **299 test eseguiti, 295 riusciti, 4 falliti, 0 ignorati**. Suite Infrastructure senza fallimenti.
- La procedura `validate.cmd` si è interrotta sui test: **simulazioni/benchmark/smoke successivi non risultano eseguiti** in quel gate.

## Correzioni RC3

| Test fallito | Diagnosi | Correzione e motivazione |
| --- | --- | --- |
| Core `IndividualRecruitmentTests.LegacyConversionPreservesPeopleAndNormalizesPercentProgress` | L'assert con due `ImmutableArray<int>` sceglie un confronto non per sequenza: output `[15]`/`[15]` identico | Convertito il valore effettivo in array e verificati gli elementi con `Assert.Equal(new[] { 15 }, ...ToArray())`. Nessun cambiamento al convertitore. |
| Application `WarehouseTests.UnknownOrInconsistentRulesAreNotConverted` | `civil-growth-13` è ora un ruleset valido; il test rimasto alla v12 lo tratta come futuro sconosciuto | Il test verifica `civil-growth-14`, ancora sconosciuto. Il test di caricamento del ruleset corrente rimane coperto da altri test. |
| Desktop `DesktopTests.ProducerWithOnSiteStorageStillShowsItsRecipeAndMissingInputs` | La funzione `BuildingRow.Format` converte i nomi della ricetta in minuscolo, coerentemente con altri test di dettaglio forno; il test attendeva maiuscole | Verifica ora la ricetta completa `1 farina · 1 acqua → 3 pane`, mantenendo distinta l'asserzione `Farina` nella diagnosi. Nessuna modifica alla formattazione dell'interfaccia. |
| Desktop `DesktopTests.HistoricalFoundationKeepsMilitaryNavigationHidden` | `ListBoxItem` può perdere l'assegnazione imperativa `IsVisible` quando Avalonia materializza/ricicla il contenitore della navigazione | `MilitaryNavigation` e `MilitaryTab` usano un binding esplicito alla proprietà `HasMilitaryRules` del VM. Non dipendono più dall'eredità del `DataContext` dei contenitori. Rinforzata l'asserzione di visibilità. |

## Invarianti che devono continuare a valere

- Lo scenario `economy-foundation` non visualizza né navigazione né scheda Esercito.
- Gli scenari `civil-growth` dotati di Caserma visualizzano entrambe le voci; il click della navigazione resta funzionante.
- Le ricette della filiera Pane mantengono la rappresentazione minuscola già utilizzata dagli altri test.
- `civil-growth-13` è valido; le versioni successive non dichiarate sono incompatibili.
- Il convertitore dei progressi e tutte le operazioni SQLite restano **identici alla RC2**.

## Cosa verificare ora

1. Estrarre RC3 in cartella nuova ed eseguire `\.\validate.cmd` da Windows con SDK .NET 10.
2. Acquisire numero test, suite e log; il gate R3 passa solo con tutte le suite e le successive simulazioni/sonde del validatore completate.
3. Verificare l'interfaccia nelle risoluzioni **1040×740** e **1240×850**: fondazione priva di Esercito, campagna con Esercito, visualizzazione ricetta Forno e stoccaggi.
4. Testare l'upgrade SQLite solo su copie di database personali; verificare backup e secondo caricamento.

**Non eseguito qui:** `dotnet build`, xUnit, simulatore, smoke Windows. La RC3 è una correzione statica basata sui quattro errori reali forniti dall'utente, **non una release validata**.
