# SimRisorse v55 — audit tecnico e preparazione al gate Windows

**28 ottobre 2026.** La v55 è una revisione di sicurezza e coerenza della v54: **nessun cambiamento alle regole di gioco** (`civil-growth-12`, 21 strutture catalogate, 16 risorse, Municipio fisso, depositi distribuiti). Il pacchetto v54 era già stato predisposto, non validato con .NET in questo ambiente.

## Esito verifiche disponibili

| Verifica | Risultato | Limite |
| --- | --- | --- |
| Integrità dell'archivio v54 di partenza | ZIP integro; confronto file con v53 limitato alle modifiche dichiarate | Non dimostra che il codice compili |
| XML di Avalonia/MSBuild | **23 file** XAML e XML di progetto analizzati, **0 errori** | Non sostituisce la compilazione Avalonia/XAML |
| Collegamenti XAML ↔ test Desktop | **40 nomi** usati in `FindControl` trovati tra i **56** controlli nominati, **0 mancanti** | Non verifica posizione o visibilità runtime |
| Pittogrammi 8×8 | **38 icone** controllate, tutte con 8 righe, 8 colonne e colori noti | Il rendering visuale richiede DesktopTests |
| Riferimenti di progetto | **15 progetti**, **18 ProjectReference** risolti, **0 mancanti** | Non compila le dipendenze NuGet |
| Link Markdown interni | **611 link** relativi tra **79 documenti** (incluse le due nuove schede v55), **0 interrotti** | Non verifica URL esterni |
| Analisi regole e migrazione | Verificati costi, base di capacità, contributi locali, versioni v10/v11/v12 e obbligatorietà militare in SQLite | Esecuzione test ancora necessaria |

L'SDK `.NET` non è installato nell'ambiente dell'audit. Per questo **non sono stati eseguiti la compilazione, i 287 casi xUnit, i benchmark né i test visuali**; non dichiararli superati sulla base delle verifiche statiche.

## Correzioni e irrobustimenti v55

1. **Integrità degli edifici fissi:** il costruttore dello scenario ora rifiuta l'avvio se un edificio marcato `IsScenarioFixed` non è presente esattamente una volta e senza addetti. Il ripristino dei salvataggi già impediva modifiche al Municipio, ma la configurazione di partenza non era protetta con la stessa forza.
2. **Riapertura del salvataggio migrato:** ampliata la verifica SQLite v11→v12. Dopo conversione, backup e salvataggio, si carica nuovamente la partita e si verifica ancora la presenza di **un solo Municipio** e la conservazione di Pane e Tavole senza doppie assegnazioni.
3. **Controlli Municipio:** quando è selezionata questa struttura non sono mostrati né i controlli di costruzione né i pulsanti per aumentare o diminuire i lavoratori. Verifica GUI aggiunta ai DesktopTests già presenti.
4. **Test migrati:** il Municipio viene inizializzato a 1 anche per i checkpoint storici; tutti gli altri nuovi tipi di edificio continuano a partire da 0. La regressione di migrazione verifica espressamente questa eccezione.
5. **Stato produttivo:** confermata nel codice la separazione tra ruolo produttivo e aumento della capacità. Un Forno può stoccare Pane e deve comunque continuare a segnalare Farina/Acqua mancanti oppure spazio insufficiente.

## Gate obbligatorio sul PC Windows

Aprire una console nella radice del pacchetto e lanciare:

```bat
.\validate.cmd
```

Esito previsto, **ancora da confermare**: ripristino NuGet bloccato al lockfile, build Release, **287 casi xUnit** superati, controlli del catalogo, simulazioni storiche, benchmark e smoke test senza errori. Il numero di casi deve essere letto nel riepilogo del comando e non inferito dalle sole classi di test.

Poi verificare nel gioco a **1040×740** e **1240×850**:

- Nuova partita: 1 Municipio, 8 abitanti, 120 Tavole, 80 Pietra, 30 Cibo, +1000 posti per risorsa; nessun comando di costruzione o personale disponibile nel dettaglio del Municipio.
- Risorse: 1050 per Tronchi/Tavole/Pietra; 1010 per Cibo/Pane; `Capacità` con spiegazione che comprende Municipio, abitazioni, Magazzini e produttori. Quantità senza decimali, resto delle misure non arrotondate impropriamente.
- Dettagli edifici: icone, font e dimensione coerenti per costi, produzione, consumo e capacità; Forno +50 Pane, Deposito alimentare +50 Cibo, case con Cibo/Pane, Caserma con Spade.
- La colonna «Si riempie tra» resta **affiancata** alla scrollbar; selezione ocra per Risorse ed Edifici; nessuna sovrapposizione in entrambe le dimensioni.
- Avvisi: un Forno pieno nomina la capacità effettiva e indica come ampliarla; un Forno senza materiali non perde la diagnosi quando possiede spazio proprio.
- Salvataggi v11: prima fare una copia manuale; caricare, salvare, verificare il backup `.civil-growth-11.backup.sqlite3`, chiudere e riaprire. Municipio sempre 1, scorte e militari invariati.

Se il gate fallisce, acquisire **primo errore di compilazione o primo test fallito**, con file/riga, messaggio e stack. Nessuna modifica alla flotta prima di un gate positivo e dell'approvazione della scheda navale.

## Prossimo incremento autorizzabile

Il contratto M5.4 è ancora una **proposta**, non codice. La [scheda v55](../design/M5_4_DECISION_GATE_V55.md) aggiorna le opzioni alla baseline `civil-growth-12`; un futuro eventuale ruleset navale si chiamerebbe `civil-growth-13`, conservando la factory e il backup delle regole precedenti.
