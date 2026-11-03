# M5.3b — integrazione militare in partita e salvataggi (v47)

**28 ottobre 2026 · Da validare con .NET 10 su Windows.**

## Contratto approvato

Il contratto A della Caserma rimane invariato: costo 280 Tavole e 200 Pietra, Forgia prerequisito, nessun addetto produttivo, 2 Spade consumate all'arruolamento, due posti per Caserma, formazione in gruppi da 60 secondi e attesa FIFO.

L'utente ha approvato anche:

- **M-01:** reclute e soldati continuano a occupare gli alloggi civili.
- **M-02:** consumano le stesse razioni della popolazione, senza aggiungere una seconda popolazione da sfamare.
- **M-03:** congedo soltanto di soldati disponibili, senza restituzione delle Spade; annullamento delle sole reclute in coda, con rimborso intero delle Spade solo se la capienza libera lo consente; formazione in corso non annullabile.
- **M-04:** nessuna perdita definitiva di soldati prima delle missioni successive.

## Codice introdotto

1. `CivilGrowthScenario.Create()` passa a **`civil-growth-10`**, **20 edifici / 16 risorse**; Caserma aggiunta, con prerequisito Forgia. `CreateMintForBenchmarks()` conserva **v9, 19/16** e i benchmark Forgia/Zecca vengono ancorati esplicitamente alla vecchia factory.
2. `GameSession.MilitaryState` è parte dell'aggregato. `AvailableWorkers` sottrae **lavoratori assegnati + reclute in attesa + reclute in formazione + soldati disponibili**. I metodi `Enlist`, `DischargeSoldiers`, `CancelQueuedRecruits` applicano i risultati puri in modo atomico. `Advance` incrementa i gruppi di addestramento soltanto nei secondi simulati quando la partita non è in pausa.
3. `GameSnapshot.Military` è opzionale nella firma per non alterare i vecchi costruttori, ma **obbligatorio per le partite v10 e vietato negli snapshot anteriori**. `Restore` rifiuta gruppi invalidi, superamento dei posti di Caserma, duplicazione tra lavoratori e militari, dati mancanti e più militari degli abitanti.
4. Il file **SQLite resta di formato 2**, con tabella aggiuntiva `military_state` nella stessa transazione delle risorse, degli edifici e della crescita. Mancanza/corruzione dello stato militare in una partita **v10 viene rifiutata**. La conversione esplicita **v9→v10** aggiunge Caserma a zero e stato militare vuoto; i salvataggi originali restano intatti fino al primo salvataggio successivo, che crea il backup.
5. Il read model `GameView.Military` espone coda, addestramento, soldati, progresso, posti, costo in Spade. **La GUI non contiene ancora pulsanti di arruolamento/congedo**: sono previsti per M5.3c.

## Verifiche introdotte

- Modello puro: annullamento della coda, rimborsi, magazzino pieno, congedo senza Spade, rifiuti atomici.
- Sessione: costo e prerequisiti della Caserma, conservazione del numero degli abitanti, assegnazioni dei lavoratori, stop in pausa, replay con snapshot a 30,5 secondi e razioni/alloggi condivisi.
- SQLite: migrazione v9→v10 con backup, coda e progresso salvati, assenza della tabella, riga mancante, valori negativi, interi fuori limite.
- Aggiornati i test storici che ricostruiscono snapshot v1–v9 da un v10 e il conteggio del catalogo.

**Totale atteso: 268 test xUnit** (249 dichiarati per v46 + 19 casi nuovi, includendo una nuova variante della migrazione storica). Il conteggio è una previsione: questo ambiente non dispone di SDK .NET, quindi **nessuna compilazione, prova Windows o benchmark v47 è stata dichiarata riuscita**.

## Gate Windows prima di proseguire

Dal progetto estratto, avviare `./validate.cmd` su Windows (`.\validate.cmd` in PowerShell). Verificare build Release, 268 casi xUnit, benchmark M5.1c e M5.2c/d congelati e smoke nativi. Se fallisce, riportare il primo errore della compilazione o il primo test non riuscito. Il salvataggio preesistente v9 deve poter essere letto e migrato senza perdere popolazione, scorte, fabbriche o cronometro.

## Passo successivo: M5.3c

Collegare i comandi a `GameRuntime`/feature handler, costruire una vista dedicata nel Desktop per Caserma e coda, mostrare **abitanti liberi / reclute / soldati**, posti occupati e progresso `mm:ss`, messaggi espliciti per Spade insufficienti, Caserma mancante e magazzino pieno su annullamento. Testare entrambi i layout **1040×740** e **1240×850**. Solo dopo valutare il Cantiere navale M5.4. Per ora non introdurre missioni o perdite di soldati.
