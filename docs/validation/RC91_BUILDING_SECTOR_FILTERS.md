# Piano di validazione RC91 / RC91.3 — Filtri settoriali v15

## Gate obbligatorio

Da Windows con SDK .NET 10, nella radice del repository:

```powershell
.\validate-f1.cmd
```

**Evidenza Windows:** RC91 si è fermata all'analizzatore `xUnit2029` in `Rc91BuildingSectorFilterTests.cs:63`, corretto in RC91.1. Nell'esecuzione RC91.1, `Desktop.Tests` ha registrato **246 test passati e 1 fallito (247 totali)**: `ExactlyOneButtonHasActivePresentationAfterEveryChange` chiamava `Single(other => other.Sector != option.Sector)` su otto pulsanti diversi. **RC91.2:** sostituito quel controllo con `Assert.All` che verifica, per ogni cambio, inattività e differenza di Background, Foreground e Border di tutti gli altri pulsanti.

**Evidenza Windows RC91.2:** dopo la pubblicazione Desktop, `validate.ps1` è fallito su `Missing render from this test run: v15-buildings-1040.png`. In RC91 i test visivi generano invece sei file `v15-buildings-{all|food|industry}-{1040|1240}.png`. **RC91.3** aggiorna il gate per richiedere ciascuno dei sei, oltre a tutti i render storici, e migliora il messaggio d'errore mostrando i PNG presenti in caso di mancanza. Non sono stati saltati test o render, e i vecchi nomi non vengono simulati copiando file.

**Attesi 1188/1188 test superati**: RC90.3 1167 confermati, 21 nuovi RC91, nessun caso aggiunto negli hotfix. Verificare zero fallimenti e zero ignorati, nessun errore dagli analizzatori. La RC91.3 non è stata compilata né eseguita in questo ambiente.

## Controlli interattivi

1. Aprire **Edifici**: accanto alla scritta fissa **Filtra** devono esserci i nove pulsanti Tutti, Alimentazione, Estrazione, Industria, Lavorazioni, Logistica, Abitazioni, Civici e Militari. **Tutti** è predefinito.
2. Alimentazione: include Pescatore, Fattoria, Pozzo, Mulino, Forno, Stalla, Cacciatore, Macellaio, Orto, Frutteto e Depositi Base/Grande/Avanzato; non mostra il vecchio `food-store`.
3. Estrazione: Taglialegna, Tagliapietra e miniere di Ferro/Carbone/Oro (tutti i tier). Industria: Fonderie, Forgia e Zecca (tutti i tier). Lavorazioni: Segherie. Logistica: Magazzino. Abitazioni: case. Civici: Municipio e Scuola. Militari: Caserma.
4. Selezionare Zecca Grande nel filtro Industria, passare a Tutti: il dettaglio deve restare sulla stessa Zecca. Selezionare un settore diverso: viene mostrato il primo edificio visibile. Tornare al settore e verificare pulsante evidenziato.
5. Gli edifici bloccati devono rimanere visibili, attenuati ma consultabili; i pulsanti Costruisci devono rimanere disabilitati se mancano requisiti. Il filtro non deve alterare personale, costi, icone o quantità.
6. Controllare le finestre a **1040×740** e **1240×850**: i pulsanti possono andare a capo ma non devono coprire intestazioni, scrollbar o dettaglio. Verificare anche passaggio del mouse senza spostamenti geometrici.
7. Avviare nuova partita e caricare un salvataggio senza effetti collaterali: nessuna informazione del filtro deve essere salvata nel database SQLite.

## Controlli automatizzati aggiunti

`Rc91BuildingSectorFilterTests`: classificazione di tutti gli edifici, 13 ID rappresentativi, 3 tier e stato bloccato, selezione conservata o riposizionata, refresh, unicità dell'evidenziazione. `V15VisualRegressionTests` esteso con tre filtri all/food/industry × due risoluzioni e sei render obbligatori e le intestazioni alle dimensioni indicate; il test di integrazione storico del vecchio pulsante è stato aggiornato senza aggiungere duplicati.

## Roadmap successiva

Mantenere separata l'attivazione dei costi quasi esponenziali: prima definire versamenti progressivi senza softlock, quindi sperimentare curva A opt-in e documentare il regime nel salvataggio. Non cambiare i prezzi predefiniti nell'ambito di questo filtro.
