# M5.4 — scheda decisionale Cantiere e Flotta aggiornata alla v55

**28 ottobre 2026 · Solo progettazione, nessuna implementazione o approvazione navale.** Questa scheda sostituisce la *baseline tecnica* della [proposta v51](M5_4_DECISION_GATE_V51.md), ma ne mantiene le alternative economiche finché l'utente non sceglie. Prima di introdurre nuovi contenuti occorre concludere il [gate v55](../validation/V55_STATIC_AUDIT.md).

## Base di gioco attuale

- `civil-growth-12`: **21 strutture** in catalogo, delle quali 20 appartengono ai **22 tipi edificabili della prima campagna**; il Municipio iniziale è una ventunesima struttura extra, non un obiettivo da costruire.
- Capacità: **Municipio +1000** per risorsa, produttori **+50** per ciascun output, abitazioni **+10/+25** Cibo e Pane, Caserma **+20** Spade, Magazzino **+500** per risorsa. Il Cantiere e le navi non richiedono un inventario privato.
- Scorte iniziali: 120 Tavole, 80 Pietra, 30 Cibo; 8 abitanti. Monete e Spade partono a zero. La Zecca produce **3 Monete/s per addetto** da 1 Oro e 1 Carbone; le Monete non sono valuta generica.
- Caserma: 2 posti per addestramento, gruppi di 60 s di gioco, 2 Spade per recluta. I soldati fanno parte della popolazione.
- Salvataggio SQLite v2, conversioni S1 con backup e stato militare obbligatorio da v10 in poi. L'eventuale aggiunta navale deve partire da **`civil-growth-13`** e **non** cambiare retroattivamente le factory v10, v11, v12.

## Contratto A — valori da approvare o modificare

| ID | Proposta non approvata | Alternativa indicativa |
| --- | --- | --- |
| **N-01** | Navi persistenti con ID univoco | Navi monouso |
| **N-02** | Cantiere: **320 Tavole + 220 Pietra**, prerequisito Zecca, nessun addetto permanente | Cambiare prezzo/prerequisito |
| **N-03** | Ogni nave: **160 Tavole + 120 Monete**, acquisto immediato e atomico | 240 Tavole + 180 Monete, oppure 120 s per costruzione |
| **N-04** | **5 soldati addestrati** per nave, senza equipaggio civile separato | 10 soldati, oppure equipaggio dedicato |
| **N-05** | Navi riutilizzabili; nessuna perdita o viaggio in M5.4 | Consumabili/perdite nelle missioni future |
| **N-06** | Almeno un Cantiere per acquistare; nessun limite di navi per Cantiere | Limite per cantiere o coda |
| **N-07** | Commercio navale e navi mercantili rinviati | Modulo successivo separato |

Con A, Cantiere e **prima nave** richiedono complessivamente **480 Tavole, 220 Pietra e 120 Monete**, oltre ai costi già sostenuti per sbloccare la Zecca e le filiere. Con 3 Monete/s della Zecca, la prima nave corrisponde a **40 s teorici** di produzione continua di Monete, senza considerare estrazione, carbone, personale e interruzioni. È una stima aritmetica, **non un benchmark misurato dalla partita iniziale**.

## Impatto preciso sul catalogo e sui salvataggi

- Dopo l'eventuale Cantiere: **22 strutture a catalogo**, di cui 21 costruibili di campagna + Municipio. Il Castello porterebbe in seguito a **23 strutture a catalogo** e 22 edifici-obiettivo costruibili.
- Le **navi non sono una nuova risorsa**: sarebbero entità persistenti identificate, mentre resterebbero 16 risorse economiche.
- Il Cantiere non deve aggiungere una capienza arbitraria a Tavole o Monete se non ne produce. Un eventuale deposito di navi è **un altro concetto** da progettare, non uno stock economico condiviso.
- Eventuali dati navali devono essere scritti **atomicamente** insieme a risorse, edifici, popolazione ed esercito. Partite v12 convertite riceverebbero **flotta vuota**, senza bonus di risorse; backup S1 originale conservato.
- Primo ordine con materiali insufficienti, Cantiere assente o quantità invalida: **nessun consumo parziale**. Due navi devono avere ID distinti dopo acquisto e riapertura del salvataggio.

## Sequenza proposta dopo approvazione

1. **M5.4a — Core e Application:** Cantiere nel catalogo, acquisto atomico di nave, ID monotoni, test su costo e precondizioni; nuova factory `civil-growth-12` congelata.
2. **M5.4b — SQLite e Desktop:** dati persistenti, migrazione S1 v12→v13, tab Flotta senza collisioni con navigazione/scrollbar, test a 1040×740 e 1240×850.
3. **M5.4c — preflight:** conteggio teorico di soldati addestrati trasportabili; **nessuna missione** e nessuna modifica ad esercito/scorte durante il controllo.

M5.5 e successivi tratteranno **spedizioni, conquista obbligatoria, Castello e condizione di vittoria**. Nessuna di queste meccaniche è autorizzata da un semplice «continua».

## Decisione necessaria

Per passare dalla progettazione al codice: **«Approvo M5.4 proposta A»**, oppure indicare quali ID cambiare (esempio: «N-03: attesa 120 s» o «N-04: 10 soldati»). Prima del codice occorre anche il risultato positivo di `validate.cmd` sulla v55.
