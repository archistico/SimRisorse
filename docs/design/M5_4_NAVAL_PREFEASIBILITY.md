# M5.4 — Cantiere navale, flotta e requisiti di trasporto: prefattibilità

Data: 2026-10-27 · v29/M4.15 · **Solo progettazione: contenuti non approvati, codice M5 non implementato.**

La base eseguibile resta **`civil-growth-5`**, snapshot SQLite **v2**, **12 edifici e 9 risorse**. Il gate più recente dichiarato superato dall'utente è v24/M4.10 (174 xUnit attesi); la GUI 8-bit v25 e gli incrementi fino a v29 attendono verifica Windows (177 xUnit attesi). Questa pagina estende gli studi [M5.1](M5_1_INDUSTRY_PREFEASIBILITY.md), [M5.2](M5_2_FORGE_MINT_PREFEASIBILITY.md) e [M5.3](M5_3_ARMY_PREFEASIBILITY.md), senza superarli né autorizzarne l'implementazione.

## 1. Cosa dicono realmente i riferimenti

Il [diagramma economico e militare](../references/RESOURCE_AND_PRODUCTION_CHAINS.md) mostra **tavole/legno lavorato + Monete → Cantiere navale → Nave**. Non chiarisce se i materiali siano costo di costruzione del *cantiere*, delle *singole navi* o di entrambi. Non indica quantità, lavoratori, velocità, capacità di trasporto, numero di scafi, consumo della nave, durate, perdite o manutenzione.

Lo stesso materiale raffigura successivamente **spedizioni → territori scoperti → crociate/conquiste → colonie**, e dalle piantagioni coloniali possibili **navi dedicate → Monete**. Queste navi da commercio potrebbero essere mezzi, rotte o servizi: **non** sono confermate come equivalenti alle navi militari M5.4. Niente entrate coloniali prima di raggiungere una colonia e niente progressione automatica senza una regola esplicita.

**Vincoli già confermati:** interfaccia desktop 8-bit basata su schede/elenchi, senza piazzare cantieri su una mappa; missioni successive governate dal tempo simulato, non dall'orologio reale; possibilità di pausa/ripresa. La scelta esatta degli obiettivi e delle conquiste resta D0.2/D0.3.

## 2. Definire che cosa sia una nave

| Modello candidato | Nave dopo la costruzione | Vantaggio | Rischio/decisione |
| --- | --- | --- | --- |
| **A — bene monouso** | Una quantità in inventario consumata all'avvio della spedizione | Stato e rollback relativamente semplici | Ogni viaggio richiede una nuova nave; flotta poco riconoscibile |
| **B — entità persistente, riutilizzabile** | Ogni nave conserva ID e stato; una missione la prenota, poi la restituisce se previsto | Scelte sulla flotta, ripresa precisa, prevenzione del doppio impiego | Richiede entità, persistenza e regole per ritorno/perdita |
| **C — capacità astratta del Cantiere** | Numero di Cantieri sblocca o limita spedizioni, senza navi individuali | UI e persistenza minime | Non rappresenta davvero le navi disegnate, crea possibili ambiguità dopo demolizione |

**Preferenza tecnica da discutere: B**, perché distingue chiaramente l'edificio che produce dal mezzo utilizzato. Non è approvata. La persistenza individuale **non** autorizza automaticamente danni, riparazioni, equipaggio, rarità, potenziamenti o combattimento navale. Una prima versione potrebbe avere un solo tipo di nave, **se l'utente lo sceglie**.

### Separare Cantiere, costruzione della nave e spedizione

1. **Cantiere:** struttura del catalogo economico; eventuale costo, prerequisiti e addetti da decidere. Non è una nave e non parte in missione.
2. **Ordine di costruzione della nave:** comando candidato che verifica il Cantiere, scorte e capacità; paga esattamente una volta e crea una nave con ID. Costruzione immediata o temporizzata **ancora da scegliere**.
3. **Flotta:** collezione di navi disponibili e, successivamente, impegnate; vanno definite le condizioni in cui il Cantiere può essere rimosso senza cancellare retroattivamente scafi pagati.
4. **Missione (M5.5):** prenota navi e, se necessario, soldati; fa trascorrere il tempo simulato; applica il risultato una sola volta. **Non appartiene al codice di M5.4.**
5. **Conquista e commercio (M5.6):** regole territoriali e navi commerciali dedicate, se approvate; non sono introdotte per deduzione dal diagramma.

## 3. Il primo viaggio deve essere raggiungibile

Il primo acquisto possibile deve dipendere solo da risorse ottenibili **prima** di partire:

| Requisito candidato | Fonte ammissibile prima della prima nave | Errore da prevenire |
| --- | --- | --- |
| Legno lavorato / tavole | Filiera civile Segheria già esistente | Richiedere più di quanto sia accumulabile con i Magazzini correnti |
| Monete, **se approvate per la nave** | Zecca M5.2, alimentata da Oro e Carbone M5.1 | Pagare la **prima nave** solo con Monete ricavate dalla prima colonia raggiunta da quella nave |
| Cantiere navale | Costruibile con input ottenibili dalle filiere precedenti | Imporre Navi o beni coloniali nel costo del **primo Cantiere** |
| Manodopera | Civili realmente liberi e alloggiati, senza sottrarre reclute/soldati per errore | Contare gli stessi abitanti come lavoratori, reclute ed equipaggio |
| Soldati, **se richiesti** | Arruolamento M5.3 con popolazione conservata | Rendere obbligatorio un contingente impossibile da trasportare |

**Soglia simbolica di trasporto:** se una nave trasporta `C` soldati e una missione richiede `S` soldati, il fabbisogno minimo teorico è `ceil(S / C)` navi, con `C > 0`. **C e S non sono stabiliti.** Includere successivamente navi già occupate, equipaggio, scorte e disponibilità effettive senza inventare capienze. La capacità di deposito e le Monete da spendere devono permettere **anche il primo lotto**, non soltanto una produzione a regime.

## 4. Prezzi, durate e addetti: alternative ancora aperte

| ID di scelta | Domanda all'utente | Opzione di lavoro da simulare | Da non assumere |
| --- | --- | --- | --- |
| **N-01** | Nave monouso, riutilizzabile o capacità astratta? | B: riutilizzabile con ID | Perdita/ritorno automatici |
| **N-02** | Tavole e Monete pagano il Cantiere, la singola nave o entrambi? | Separare due contratti di costo | Numeri dedotti dalle icone |
| **N-03** | Costruzione nave immediata o con coda temporizzata? Il Cantiere impiega lavoratori? | Prima iterazione immediata, solo come **possibile** baseline | Un secondo timer deciso senza dati M4/M5 |
| **N-04** | Quanti soldati e quante altre risorse trasporta una nave? Serve equipaggio civile? | Capacità positiva esplicita, un solo tipo iniziale da valutare | Valori predefiniti o abitanti creati dal nulla |
| **N-05** | Le navi impegnate ritornano? Possono andare perdute? Sono rimborsabili/congedabili? | Separare il trasporto M5.4 dagli esiti M5.5/M5.6 | Eventi casuali, affondamenti e riparazioni non richiesti |
| **N-06** | Esiste un limite di flotta per Cantiere o Magazzino? Cosa accade quando lo si demolisce? | Nessun tetto implicito fino a scelta contraria | Navi distrutte soltanto perché un edificio sparisce |
| **N-07** | Navi commerciali coloniali sono le stesse unità o entità separate? | Rinviare a M5.6 | Commercio che genera Monete automaticamente |

**Tutti i prezzi sono intenzionalmente lasciati senza cifre.** Gli studi M5.1/M5.2 contengono esempi **A/B non approvati**; usare Monete come valuta generale oppure scorta per azioni mirate cambia il contratto di spesa e richiede D0.8. L'opzione S1/S2 sui salvataggi va approvata prima di qualunque nuova entità persistente.

## 5. Contratto di integrità per una futura flotta persistente (modello B)

Una nave candidata ha `ShipId` stabile, `ShipTypeId`, `Status`, ed eventualmente `MissionId`, dati di acquisto e versione delle regole. **Campi proposti**, non schema già introdotto. Stati minimi futuri:

| Stato | Significato | Transizione possibile |
| --- | --- | --- |
| **Disponibile** | Nave costruita, libera e selezionabile | Prenotazione atomica da M5.5 |
| **Impegnata** | Associata a una sola missione | Esito e ritorno/perdita in M5.5–M5.6, se approvati |
| **Rimossa/perduta** | Non utilizzabile; eventuale evento storico permanente | Soltanto dopo regola esplicita su demolizione/perdite |

In **M5.4** basta creare e mostrare navi *Disponibili*: non simulare missioni fittizie per anticipare M5.5. Quando M5.5 verrà approvata, la prenotazione deve controllare nello stesso passo disponibilità di navi, soldati, eventuali Monete e capacità; **o avvengono tutte le mutazioni o nessuna**. Due comandi concorrenti non possono assegnare lo stesso `ShipId` a due missioni, nemmeno dopo un caricamento o una ripetizione dell'azione.

- **Tempo:** produzione temporizzata, se scelta, segue soltanto i secondi simulati; pausa, passo lungo/corto e riapertura non devono alterare esiti o residui.
- **Acquisto:** rifiuto per Monete/tavole insufficienti, Cantiere assente o capacità esaurita = **zero spesa e zero nave**. Un clic non può generare due scafi.
- **Salvataggi:** identificatori e ordine stabile, numero e stato delle navi, eventuali ordini pendenti e cronometro simulato persistono. Proteggere gli originali `civil-growth-1…-5` con conversione approvata (solo in caso S1) o import visibile (S2). Snapshot SQLite v2 corrente **non dimostra** che futuri dati navali vi entrino senza migrazione.
- **Referenze:** una `MissionId` non può puntare a una nave assente; una nave impegnata non può partecipare a più missioni. Lo stato *Disponibile* non è deducibile dal solo numero di Cantieri.
- **Interfaccia:** pannello 8-bit Cantiere/Flotta con costo effettivo, quantità di navi disponibili/impegnate, prerequisiti e motivo del pulsante disabilitato. Mantiene invariata la geometria in hover, messaggi in area visibile e testo associato alle icone alle risoluzioni **1040×740** e **1240×850**. Non introdurre mappa di posizionamento.

## 6. Piano incrementale proposto per M5.4

| Fase futura | Perimetro limitato | Evidenza necessaria |
| --- | --- | --- |
| **M5.4a — Cantiere e acquisto** | Catalogo Cantiere, prerequisiti, comando atomico per una nave; costo e modello scelti in D0/N-01–N-03 | Primo acquisto da stock precedenti alla navigazione, errore senza spesa, costo e persone riconciliati |
| **M5.4b — Flotta salvabile** | Lettura tipizzata delle navi, identità persistenti, UI di elenco e round trip SQLite | Navi distinte dopo restart, nessun ID duplicato, backup e vecchi salvataggi preservati |
| **M5.4c — Preflight trasporto** | Verifica **soltanto preparatoria** di disponibilità/capacità, senza far partire missioni | Casi di capacità `C`, soldati `S`, navi insufficienti e occupate, niente mutazioni inattese |

Il contenuto M5.4 può quindi essere validato **senza** aver implementato spedizioni: nessun comando di partenza, avanzamento viaggio, conquista o commercio è autorizzato. M5.5 aggiungerà le transizioni di prenotazione e missione; M5.6 i risultati territoriali, sempre in incrementi e gate separati.

## 7. Matrice di test da realizzare quando il modello sarà approvato

| Caso futuro | Risultato verificabile |
| --- | --- |
| Cantiere costruito con materiali validi | Edificio presente e stock scalati una volta, nessuna nave gratuita |
| Primo acquisto nave con tavole/Monete sufficienti | Una nave con ID distinto e costo scalato **una sola volta** |
| Materiali o Monete insufficienti; Cantiere assente | Rifiuto senza consumo parziale, ID non assegnato a nave inesistente |
| Due acquisti successivi e acquisto multiplo | ID univoci, spesa totale identica alla somma, regole di capienza rispettate |
| Save/reload prima e dopo il comando | Numero navi, identificatori, scorte e tempo coerenti; nessuna duplicazione |
| Vecchio salvataggio M4 con nessuna nave | Conversione/import **espliciti**, copie originali protette, nessuna nave creata implicitamente |
| Capacità insufficiente `ceil(S/C)`; navi in parte impegnate | Blocco spiegato nel preflight; nessuna missione avviata in M5.4 |
| Pause e intervalli equivalenti, se costruzione temporizzata | `Advance(600s)` = 600 passi da 1 s; nessuna costruzione offline |
| Demolizione del Cantiere dopo acquisto, **solo se approvata** | Regola esplicita su flotta conservata o meno; mai cancellazione implicita |
| Mouse/tastiera/DPI su 1040×740 e 1240×850 | Costi/prerequisiti leggibili, hover fisso, navi disponibili e bloccate distinguibili |

**Benchmark futuri:** tempo e materiali per primo Cantiere/prima nave; quanta Moneta produce realmente la Zecca sotto contesa per il Carbone; intervallo necessario per costruire `N` navi; confronto fra investire prima nella seconda filiera industriale o nella flotta. Il percorso **prima nave → prima missione → prima conquista** dovrà essere misurato solo quando M5.5/M5.6 esisteranno. Nessuna cifra di ore viene dedotta da questo studio.

## 8. Decisione di uscita e prossima dipendenza

M5.4 potrà iniziare **solo dopo**: gate Windows M4, [approvazione D0](D0_DECISION_BOARD.md) pertinente (catalogo, isole, popolazione/tempo, Monete e raggiungibilità), scelta S1/S2, validazione M5.1–M5.3 e approvazione delle scelte N-01…N-06. N-07 può restare rinviata a M5.6 se il modello navale scelto non la presuppone.

**Stato finale di questa scheda: prefattibilità completata; nessuna nave, Cantiere, Moneta, missione o migrazione implementata.** La fase successiva di progettazione è [M5.5 — spedizioni e tempo di viaggio](../ROADMAP.md), dove andranno decisi prenotazione, ritorni, insuccessi e applicazione unica degli esiti.
