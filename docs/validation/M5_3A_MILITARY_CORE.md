# M5.3a — modello isolato per la Caserma (v46)

**28 ottobre 2026 · Implementazione iniziale del Core, ancora non collegata alla partita.**

## Decisione ricevuta

L'utente ha confermato il **contratto A — Formazione semplice** della [scheda decisionale v43](../design/M5_3_DECISION_GATE_V43.md):

| Parametro | Valore approvato |
| --- | --- |
| Edificio | Caserma (`barracks`) |
| Costo di costruzione | **280 Tavole + 200 Pietra** |
| Prerequisito | **Forgia** |
| Addetti produttivi alla Caserma | **0** |
| Arruolamento | **2 Spade consumate per persona** |
| Formazione | **2 posti simultanei per Caserma; 60 s per gruppo** |
| Eccedenze | **Coda FIFO**, i nuovi arruolati non entrano in un gruppo già in corso |

Le scelte accessorie **M-01/M-02/M-03/M-04** (alloggi, razioni, congedi/cancellazioni, perdite) restano proposte fino a conferma distinta. È comunque già vincolante **non duplicare gli abitanti**: chi è in formazione o soldato non potrà essere anche un lavoratore libero o assegnato.

## Incremento v46

Introdotto in `src/SimRisorse.Core/Military/` un componente **puro**, indipendente da UI, SQLite e tempo reale:

- `MilitaryTrainingRules.SimpleBarracks` conserva in un unico luogo costo e parametri approvati.
- `MilitaryTrainingState` registra **reclute in attesa**, **reclute in formazione**, **soldati disponibili** e **progresso in secondi**; `EnlistedPopulation` restituisce il totale di persone già riservate all'esercito.
- `MilitaryTraining.Enlist` verifica ordine positivo, presenza della Caserma, civili disponibili dopo sottrazione degli addetti e militari già impegnati, scorta di Spade e limiti numerici; **un rifiuto lascia risorse e stato immutati**.
- I primi arruolati occupano fino a due posti per Caserma. I nuovi arrivati durante un gruppo attivo vanno nella coda successiva e non sfruttano posti rimasti vuoti nel gruppo corrente.
- `MilitaryTraining.Advance` fa avanzare soltanto il **tempo simulato intero** passato dal chiamante; un gruppo termina a 60 s precisi, poi parte il successivo immediatamente. Aggiungere una Caserma aumenta i posti **a partire dal gruppo seguente**.
- `MilitaryTraining.Validate` rifiuta tempi incongruenti, progressi fuori intervallo, capienza di formazione superata o conteggi negativi.

Questi metodi non modificano ancora alcun `GameSession`. **Il catalogo attivo rimane `civil-growth-9` (19 edifici, 16 risorse), i salvataggi SQLite v2, il gioco e la GUI sono invariati.** Nessun soldato, nuova Caserma o pulsante sono ancora visibili. Il modello non produce soldati autonomamente durante la pausa: la futura integrazione dovrà invocare `Advance` soltanto attraverso il ciclo di simulazione già in uso.

## Gate dei test

`tests/SimRisorse.Core.Tests/MilitaryTrainingTests.cs` aggiunge **15 casi xUnit** per:

- parametri approvati; ordini nulli/negativi; assenza di Caserma;
- Spade insufficienti, lavoratori occupati e arruolamento ripetuto;
- pagamento **una sola volta** e conservazione della popolazione riservata;
- reclute tardive, confini 59/60/61 s e attivazione immediata del gruppo successivo;
- 20 reclute in 600 s con una Caserma e 30 in 480 s con due;
- Caserma costruita durante un gruppo, replay identico con passi temporali suddivisi in più blocchi;
- inattività senza reclute, dati militari invalidi, limiti numerici e atomicità dei rifiuti.

**249 casi xUnit attesi in totale (234 della v45 + 15 nuovi)**. Su Windows la v45 è stata dichiarata passata dall'utente, **senza log dettagliato per la nuova esecuzione**. Il gate v46 deve ancora essere eseguito: questo ambiente non dispone di .NET SDK. Il motore isolato **non equivale** alla M5.3 completa.

## Dipendenze per l'integrazione M5.3a/b

1. Configurare il catalogo `civil-growth-10` con Caserma e mantenere una factory storica **v9** per i benchmark M5.2.
2. Collegare arruolamento e formazione al `GameSession` con `AvailableWorkers = Population − addetti − reclute − soldati` e alimentazione/alloggi coerenti con le scelte M-01/M-02.
3. Estendere lo snapshot e il SQLite v2 con stato militare **obbligatorio per v10**; conversione S1 **v9→v10**, backup prima della sostituzione, restore in pausa, rigetto dello stato militare mancante/corrotto. Nessuna conversione automatica silenziosa di file v10 incompleti.
4. Esporre comando, cronometro e diagnostica nell'interfaccia, rispettando dimensioni e regole della GUI retro.
5. Decidere esplicitamente congedi e annullamenti M-03 prima di abilitarli; missioni e perdite M-04 resteranno per le milestone successive.

**Non cambiare l'ID dello scenario corrente né dichiarare la feature disponibile prima che questi passaggi e i test di persistenza siano conclusi.**
