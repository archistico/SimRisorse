# SimRisorse — M5.3: scheda decisionale per Caserma e soldati

**28 ottobre 2026 · scheda storica v43, con decisione A approvata il 28/10 e primo modello isolato v46.**

La baseline giocabile resta `civil-growth-9` (**19 edifici / 16 risorse**). Forgia e Spade sono implementate. **Successivamente alla stesura di questa scheda, l'utente ha scelto il contratto A**: 280 Tavole/200 Pietra, prerequisito Forgia, 2 Spade per recluta, 2 posti/Caserma, 60 s/gruppo, coda FIFO. Le ulteriori scelte **M-01…M-04** (alimentazione, alloggi, annullamento/congedo e perdite) **non sono ancora approvate**. La v46 introduce soltanto un [modello isolato nel Core](../validation/M5_3A_MILITARY_CORE.md), senza collegarlo al catalogo, alla GUI o ai salvataggi. I 234 test v45 sono stati dichiarati passati dall'utente; i 15 nuovi test v46 attendono il gate.

## 1. Che cosa è già deciso e cosa no

| Elemento | Stato | Conseguenza |
| --- | --- | --- |
| Caserma (`barracks`) nella prima campagna da 22 tipi | **Approvato** | Sarà un edificio, distinto da una risorsa «Soldati» |
| Forgia → Spade | **Implementato** | 1 Ferro + 1 Carbone produce 2 Spade/s per addetto |
| Spade consumate all'arruolamento | **Approvato come principio** | Il comando dovrà scalarle una sola volta, senza duplicazioni |
| Popolazione unica | **Vincolo di integrità** | Un individuo non può essere contemporaneamente lavoratore, recluta e soldato |
| Vittoria e isole | **Ruolo approvato** | Prima conquista obbligatoria; Castello come edificio finale, senza inventare ora le forze necessarie |
| Prezzi e formazione modello A | **Approvati 28/10** | Valori del contratto A; primo modello isolato v46 |
| Congedi, cancellazioni, razioni, alloggi e perdite | **Da approvare** | Nessuna implementazione implicita |

## 2. Tre contratti confrontabili

I numeri sotto sono **ipotesi di bilanciamento**, non ricette dedotte dal diagramma originale.

| Proprietà | **A — Formazione semplice (raccomandata per il primo test)** | B — Arruolamento immediato | C — Addestramento più impegnativo |
| --- | --- | --- | --- |
| Costo Caserma | **280 Tavole + 200 Pietra** | 240 Tavole + 180 Pietra | 360 Tavole + 260 Pietra |
| Prerequisito | Forgia costruita | Forgia costruita | Forgia costruita |
| Addetti civili richiesti alla Caserma | 0 | 0 | 1 |
| Spade consumate per persona | **2** | 2 | 3 |
| Formazione | **60 s per gruppo** | Istantanea | 90 s per gruppo |
| Reclute simultanee per Caserma | **2** | Nessun limite di formazione | 2 |
| Ordini oltre i posti simultanei | **Coda FIFO**, persone e Spade riservate alla conferma | Non applicabile | Coda FIFO |
| Vantaggio | La Caserma ha un ruolo e più Caserme accelerano la formazione | Pochissimi stati e controlli | Più tensione economica |
| Svantaggio | Occorre persistere coda e progresso | Caserma ridotta a sblocco | Può penalizzare la crescita troppo presto |

**Decisione successiva:** scelto il contratto **A** per M5.3a/b. Il rapporto **2 Spade → 1 recluta** è un prezzo di gameplay, non una equivalenza fisica di equipaggiamento; le Spade sono **consumate**, non conservate come oggetti per unità. A 2 posti per Caserma e gruppi da 60 s, 20 reclute richiedono almeno 10 gruppi (**600 s**) con una Caserma, oppure 5 (**300 s**) con due, assumendo che le 20 persone e 40 Spade siano già disponibili. Questi sono conti teorici di sola formazione, senza i tempi di costruzione, produzione o crescita.

Per 30 reclute, la medesima ipotesi impiega **900 s** con una Caserma (15 gruppi) e **480 s** con due (8 gruppi). Il vantaggio di costruire una seconda Caserma va confrontato con il suo costo reale e con le esigenze di personale civile; **non** si può dedurre una durata totale della campagna da questi numeri.

## 3. Regola fondamentale della popolazione

Il campo attuale `Population` continua a significare **residenti vivi complessivi**, anche dopo la futura aggiunta di militari:

\[
P = W + F + Q + T + S
\]

con:

- `W`: civili assegnati ai lavori produttivi;
- `F`: civili liberi, che possono essere assegnati o reclutati;
- `Q`: reclute in attesa nella coda;
- `T`: reclute attualmente in formazione;
- `S`: soldati formati e disponibili (in futuro anche quelli in missione, da contare separatamente senza duplicarli).

**Nessuna persona nuova** è creata da Caserme e Forgia. Il comando di arruolamento fa `F−N`, `Q/T+N` e `Spade−2N` nel modello A, solo se **tutte** le condizioni sono soddisfatte. Comando fallito = nessuna variazione. Il completamento fa `T−N`, `S+N`, senza modificare P né consumare di nuovo Spade.

Nella futura integrazione del Core la proprietà `AvailableWorkers` dovrà sottrarre anche `Q + T + S`, altrimenti il giocatore potrà assegnare al lavoro persone già reclutate. Lo stesso controllo deve bloccare arruolamenti di civili occupati. La formula **non** va duplicata nella GUI.

**Esempio di limite reale:** con 40 residenti e 15 lavoratori già assegnati, si possono reclutare al massimo **25 persone**, anche con 240 Spade in magazzino. Se occorrono 30 soldati, bisogna aumentare la popolazione o liberare almeno cinque lavoratori. Questo spiega perché il benchmark industriale da 240 Spade non corrisponde automaticamente a 120 soldati.

## 4. Formazione per gruppi, coda e tempo

Per mantenere il modello A semplice e riproducibile propongo **una coda globale FIFO e un unico cronometro per il gruppo attivo**:

1. Un comando `Arruola(N)` riserva subito N civili liberi e consuma `2 × N` Spade. N è intero positivo. Le persone entrano nella coda, o nel gruppo attivo se non ce n'è già uno.
2. Un gruppo contiene fino a **2 × Caserme** reclute. Ogni gruppo impiega **60 secondi simulati**, indipendentemente dal numero di posti occupati.
3. Se arriva una nuova richiesta durante la formazione di un gruppo, entra nella **coda successiva**, anche se il gruppo corrente ha posti liberi: nessuna persona può completare la formazione in meno dei propri 60 s.
4. A fine gruppo, i suoi membri diventano soldati disponibili **una sola volta**. Si avvia il gruppo seguente e il cronometro riparte da zero.
5. Una nuova Caserma aumenta i posti **dal gruppo successivo**. Nessuna demolizione è prevista per ora. Pausa e chiusura non fanno trascorrere tempo di formazione.
6. Passi equivalenti — `Advance(600 s)`, 600 volte `Advance(1 s)` o passi frazionari — devono produrre lo stesso stato, incluse code e tempi residui.

L'interfaccia dovrà distinguere **In attesa**, **In formazione** e **Soldati disponibili**, oltre a mostrare il tempo al prossimo gruppo. Una Caserma priva di reclute non produce soldati automaticamente.

## 5. Quattro ulteriori scelte, con indicazione consigliata

| ID | Decisione da approvare | Proposta prudente | Conseguenza |
| --- | --- | --- | --- |
| **M-01** | Alloggi | Reclute e soldati occupano gli **stessi posti abitativi** dei civili | Nessuna nuova capacità abitativa nascosta |
| **M-02** | Alimentazione | Stesse razioni Cibo/Pane dei residenti, senza bonus o penalità militari iniziali | La crescita civile continua a usare `Population` totale; evitare di contare due volte i consumi |
| **M-03** | Annullamento e congedo | Consentire il congedo dei soldati disponibili, **senza restituzione delle Spade consumate**; annullare reclute in attesa con rimborso delle Spade **solo se la capienza lo consente**; non interrompere il gruppo già in formazione nella prima versione | Evita di bloccare per sempre la forza lavoro; la cancellazione deve essere atomica e spiegare eventuale deposito pieno |
| **M-04** | Perdite e missioni | **Nessuna perdita permanente in M5.3**; le missioni e gli eventuali caduti richiedono un contratto separato in M5.5–M5.6 | Nessuna diminuzione imprevista di popolazione o soldati |

La proposta di annullamento **M-03** è deliberatamente più stretta del congedo: un gruppo che ha iniziato la formazione termina, poi i soldati possono essere congedati. Rimborsare Spade quando lo spazio è insufficiente causerebbe una scorta oltre capienza; bloccare con una spiegazione operativa è coerente con le regole dei Magazzini già adottate.

## 6. Stato persistente e S1: scelta tecnica da verificare

M5.3 introduce dati che **non** possono essere dedotti dal numero di Caserme né dalle sole risorse. Proposta per la versione successiva delle regole: **`civil-growth-10`**, con conversione esplicita da v9:

- Caserma aggiunta in catalogo con `Count=0`, `AssignedWorkers=0`;
- `MilitaryState` iniziale con coda, gruppo attivo, soldati e cronometro a zero;
- vecchie scorte, alloggi, lavoratori, popolazione, tempo, frazioni e backup preservati.

**Non applicare questa conversione adesso.** Va prima decisa la rappresentazione: un nuovo formato snapshot/SQLite, oppure una tabella complementare del formato v2 con lettura obbligatoria per le nuove regole. La seconda strada mantiene più compatibilità con i salvataggi v2, ma richiede una guardia esplicita: **v10 senza stato militare non può essere interpretato come “zero militari” se si tratta di un file incompleto o corrotto**. Solo un salvataggio realmente v9 può migrare a zero mediante S1. Il backup dell'originale deve precedere la prima scrittura convertita.

Campi logici minimi: `WaitingRecruits`, `TrainingRecruits`, `AvailableSoldiers`, `TrainingProgressSeconds`. Lo stato del gruppo attivo e della coda è sufficiente finché non si introducono identità dei singoli soldati. Per missioni future servirà distinguere soldati disponibili e impegnati, senza rimuovere il conteggio degli abitanti in modo implicito.

## 7. Gate tecnico e casi che dovranno passare

| Area | Prove richieste |
| --- | --- |
| Transazioni | N non valido, nessuna Caserma, Spade insufficienti, civili liberi insufficienti: rifiuto completo, zero mutazioni |
| Conservazione | Nessun lavoratore può anche essere recluta; popolazione totale invariata nel reclutamento e nella formazione |
| Formazione | Confini a 59/60/61 s, gruppo non pieno, coda lunga, nuova Caserma a metà gruppo, gruppi consecutivi |
| Orario | Pausa, passi frazionari, passo lungo, avvio e termine esatti, nessun progresso offline |
| Persistenza | Salva/ricarica con gruppo parziale, con coda e gruppo appena completato; nessun doppio soldato o pagamento |
| S1 | Vecchia partita v9 con risorse e lavori già avviati → v10 con militari zero; file vecchio non sovrascritto senza backup |
| Capienza | Annullamento con deposito Spade pieno e con spazio sufficiente, nessuna creazione di Spade oltre limite |
| GUI | Nuova scheda con costo, persone disponibili, coda, prossimo completamento e blocchi; render 1040×740 e 1240×850, selezione ocra e scrollbar visibili |
| Filiera | Dalla partita civile a Spade, poi Caserma e primo soldato; tempo reale misurato, nessuna risorsa militare iniziale regalata |

## 8. Decisione richiesta prima del codice

**Scelta effettuata:** contratto A (280/200, due posti per Caserma, 2 Spade/persona, 60 s/gruppo, coda FIFO). **Restano da decidere M-01/M-02/M-03/M-04**, proposte nella sezione 5; l'approvazione del contratto A **non** equivale ad approvare automaticamente queste altre quattro regole. Non collegare `civil-growth-10`, nuovi comandi, un nuovo formato di salvataggio o la GUI militare prima di completare il progetto di persistenza e la coerenza con le scelte residue. Il modello isolato v46 non altera ancora la partita.

Riferimenti: [prefattibilità militare precedente](M5_3_ARMY_PREFEASIBILITY.md), [scheda decisioni D0](D0_DECISION_BOARD.md), [ruolo della prima campagna](FIRST_CAMPAIGN_PROPOSAL.md), [M5.2d dalla prima colonia](../validation/M5_2D_REAL_START.md).
