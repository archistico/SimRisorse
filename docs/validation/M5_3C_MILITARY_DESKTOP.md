# M5.3c — interfaccia Esercito (v48)

**Aggiornamento v50 — secondo gate Windows:** la build v49 è riuscita; **275/276 test passati**. Unico fallimento: `HistoricalFoundationKeepsMilitaryNavigationHidden` alla riga 177 di `DesktopTests.cs`. `MainViewModel.HasMilitaryRules` risultava `false`, ma la voce `ListBoxItem` Esercito era visibile: il binding sul contenitore di un item non dispone necessariamente del `DataContext` del VM. La v50 rimuove i due binding `IsVisible` da `MilitaryNavigation` e `MilitaryTab` e li imposta entrambi dal costruttore di `MainWindow`, usando lo stesso `viewModel.HasMilitaryRules`. Lo scenario della finestra non cambia durante la partita. **Test invariati (276)**, nuovo gate Windows da eseguire prima di dichiarare M5.3c verificata.

**Aggiornamento v49 — primo tentativo di compilazione Windows:** `validate.cmd` v48 ha restituito `CS0136` in `SqliteGameSaveStore.cs:171` prima di eseguire i test. Corretto il conflitto tra variabili di pattern rinominando la prima `military` in `militaryCandidate` nella condizione iniziale di `SaveCore`. Restano **276 test attesi**; la v49 richiede una nuova esecuzione del gate completo. Nessuna modifica a formato o logica di persistenza.

**28 ottobre 2026 — implementata, ma da compilare e validare con .NET 10 su Windows.**

## Perimetro

Nessuna modifica a `GameSession`, ai costi o alle regole `civil-growth-10`, allo schema SQLite v2 e alle migrazioni S1. L'interfaccia usa il modello di lettura militare creato in v47. I benchmark M5.2c/d rimangono ancorati a `civil-growth-9`.

### Cosa appare nel gioco

La voce **Esercito** è nel menu laterale della partita civile v10 e non compare in `economy-foundation`. La pagina indica:

- **Contabilità unica delle persone**: abitanti totali = lavoratori assegnati + liberi + reclute in coda + reclute in formazione + soldati addestrati. Reclute e soldati continuano a consumare Cibo/Pane e occupare gli alloggi.
- **Caserme**: due posti di addestramento per ogni Caserma; nessun lavoratore assegnato. Edifici mostra l'effetto di formazione anziché un inesistente aumento di posti abitativi, con icona retro 8×8.
- **Spade**: scorta, costo dell'ordine scelto, consumo una tantum all'arruolamento; nessun rimborso sul congedo.
- **Formazione**: coda, persone del gruppo attivo, soldati pronti, barra di completamento e secondi residui in `mm:ss`; il gioco progredisce solo con il cronometro simulato, non in pausa.
- **Ordini**: quantità 1/5/10, «Arruola», «Annulla coda», «Congeda». I pulsanti sono abilitati soltanto se le precondizioni rispecchiano la stessa regola già nel Core; le descrizioni restano leggibili anche quando il pulsante è disabilitato.

### Diagnosticare un ordine non disponibile

| Situazione | Cosa legge il giocatore | Come procedere |
| --- | --- | --- |
| Nessuna Caserma | Manca una Caserma, prima serve una Forgia | Costruire Forgia e Caserma da Edifici |
| Poche Spade | Disponibili e necessarie | Assegnare addetti alla Forgia; controllare Ferro, Carbone e capienza |
| Pochi abitanti liberi | Disponibili rispetto alla quantità selezionata | Liberare addetti oppure ampliare alloggi e attendere la crescita |
| Nessuna recluta annullabile | Numero di reclute **ancora in coda** | Attendere nuove reclute in coda o selezionare un ordine più piccolo |
| Scorta Spade piena | Spazio libero rispetto alle Spade da restituire | Costruire un Magazzino o liberare capacità, poi annullare la coda |
| Nessun soldato disponibile | Solo i soldati addestrati possono essere congedati | Aspettare 60 secondi simulati per il gruppo in formazione |

Il rimborso delle Spade su annullamento è **atomico**: se manca lo spazio per tutte le Spade dell'ordine, non viene congedata alcuna recluta e nessuna Spada viene restituita parzialmente. Gli ordini non annullano gruppi già in addestramento.

## Modifiche e test preparati

- Application: tre handler separati in `Features/Military/` per arruolamento, annullamento reclute in coda e congedo. Ciascuno usa l'esistente `GameRuntime.Execute` sotto lock; nessuna logica parallela dei saldi.
- Desktop: proprietà, etichette, hint e tre comandi in `MainViewModel`; nuovo `MilitaryTab`, `MilitaryNavigation` e dettagli in `MainWindow.axaml`; nuovo pittogramma Caserma e presentazione corretta in `BuildingRow`.
- `MilitaryFeatureTests.cs`: **3** test per la sequenza arruolamento→annullamento→formazione→congedo, rifiuto atomico con magazzino Spade pieno e scenario storico senza militari.
- `DesktopTests.cs`: **5 casi** (di cui due test parametrici a **1040×740** e **1240×850**) per iconografia e descrizione Caserma, capienza insufficiente, navigazione, azioni e geometria dei pulsanti, assenza della scheda militare nella Foundation.

**Conteggio atteso: 268 (v47) + 3 (Application) + 5 (Desktop) = 276 xUnit.** È una previsione; la build e i test non sono stati eseguiti perché manca l'SDK .NET nell'ambiente di preparazione. Non dichiarare il gate M5.3c superato prima del log Windows.

## Verifica Windows richiesta

Dalla cartella principale del progetto v48 estratto:

```powershell
.\validate.cmd
.\run-game.cmd
```

Controllare oltre ai test automatici:

1. Da Edifici, la Caserma costa **280 Tavole / 200 Pietra**, ha prerequisito **Forgia**, ed espone **+2 posti di addestramento**, non abitativi. Nessun addetto assegnabile.
2. Costruire Caserma, produrre Spade con Forgia e aprire Esercito. Quantità 5: si devono consumare **10 Spade**, 2 reclute iniziano subito con una Caserma e 3 attendono. Abitanti totali invariati; lavoratori liberi diminuiscono di cinque.
3. Annullare 1 recluta in coda: deve ritornare un abitante libero e devono comparire **2 Spade** rimborsate. Il pulsante è disabilitato quando non c'è una recluta in coda o lo spazio per restituirne le Spade.
4. Avviare il tempo. Dopo 30 secondi il gruppo attivo indica `00:30` residui; dopo 60 secondi due persone diventano soldati pronti e il gruppo seguente inizia solo allora.
5. Congedare 1 soldato pronto: il lavoratore libero torna disponibile, le Spade non vengono rimborsate. È impossibile congedare un addestrando.
6. Ridimensionare la finestra a **1040×740** e **1240×850**: barra delle risorse, colonna «Si riempie tra», dettagli degli edifici e i tre comandi militari devono restare distinguibili, senza sovrapposizioni. Scorrere il pannello militare per verificare che nessun testo sia tagliato.
7. Avviare `run-game.cmd --scenario economy-foundation`: la scheda Esercito non deve apparire. Verificare salvataggio/ripresa dei gruppi già in formazione anche in una partita civile precedente convertita da v9.

## Stato e prossima milestone

| Milestone | Stato | Gate |
| --- | --- | --- |
| M0–M4, M5.1a/b | Verificate nelle rispettive tappe | Conferme storiche |
| M5.2a/b, benchmark M5.2c/d | Funzioni implementate, confronti predisposti | Log numerici da acquisire |
| M5.3a/b | Core e salvataggi implementati in v46/v47 | Inclusi in `validate.cmd` v48 |
| **M5.3c** | **GUI e 8 casi xUnit preparati (v48)** | **Gate Windows pendente** |
| M5.4 Cantiere, navi e trasporti | Prefattibilità, contratto numerico da scegliere | Non avviare senza decisioni |
| M5.5–M5.7 missione, isola, Castello | Future | Dopo M5.4 e relativi gate |
| M6–M7 vittoria, bilanciamento, qualità | Future | Campagna completata |

La versione di ripresa e i link di riferimento sono in [Handoff](../HANDOFF.md) e [Roadmap](../ROADMAP.md).
