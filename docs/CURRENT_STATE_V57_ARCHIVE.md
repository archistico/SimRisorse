# SimRisorse — stato corrente autorevole

**Data: 28 ottobre 2026. Checkpoint: v57 documentale, software v55.** Questo documento ha precedenza sulle descrizioni «correnti» rimaste nei rapporti storici. Nessuna nuova meccanica introdotta nella v57.

## 1. Identità e perimetro

- Gioco gestionale economico/militare **C# / .NET 10 / Avalonia 12.1.4 / SQLite**, con grafica 8-bit originale, per ora organizzato in **elenchi e schede**, senza mappa edilizia o tattica.
- Soluzione: `SimRisorse.slnx`. Livelli: `src/SimRisorse.Core`, `.Application`, `.Infrastructure`, `.Desktop`; suite sotto `tests/`; simulatore sotto `tools/SimRisorse.Simulation`.
- Scenario giocabile corrente: **`civil-growth`**, versione **`civil-growth-12`**. Scenario di riferimento storico separato: `economy-foundation`.
- **21 strutture** nel catalogo attivo: **20** fanno parte delle **22 tipologie della prima campagna** e sono già implementate; il **Municipio** è una struttura supplementare fissa, **non** un obiettivo della campagna. Restano da implementare **Cantiere navale e Castello**. **16 risorse** economiche; navi non implementate.
- [Elenco ufficiale degli ID, dei nomi UI e dei conteggi](CATALOG_CURRENT.md). Nel codice la risorsa `planks` si chiama **Legno lavorato** (alias documentale «Tavole») e `stonecutter` si chiama **Tagliapietra** (non «Cava»).

## 2. Partita iniziale e capacità

- Inizio: **8 abitanti**, **120 Legno lavorato** (detto anche «Tavole» nei documenti di design), **80 Pietra**, **30 Cibo**, nessuna Spada/Moneta; edifici: **1 Taglialegna (`tree-feller`), 1 Segheria (`sawmill`), 1 Tagliapietra (`stonecutter`), 1 Casa piccola, 1 Municipio**. I primi tre hanno inizialmente un addetto ciascuno. Partita inizialmente in pausa.
- **Municipio fisso, unico, senza addetti**, non acquistabile, dà **1.000 posti a ciascuna delle 16 risorse**. La capacità base invisibile dello scenario attivo è **zero**.
- Ogni edificio produttivo aggiunge **50 posti** *per ciascuna risorsa che produce*; il Deposito alimentare produce Cibo, il Forno produce Pane, con identica regola.
- Ogni **Magazzino** aggiunge **500 posti per ciascuna risorsa**; **Casa piccola** aggiunge +10 Cibo e +10 Pane; **Casa grande** +25 e +25; **Caserma** +20 Spade. Le scorte sono **sempre condivise**, non inventari separati per edificio.
- Con gli edifici iniziali: capacità **1.050** di Tronchi/Tavole/Pietra; **1.010** di Cibo/Pane; generalmente **1.000** per le altre risorse non prodotte dalle strutture iniziali. Questi numeri sono previsioni di codice/test, non risultati visuali certificati su Windows per v55.
- In **Risorse**: ordine alfabetico italiano, righe selezionate in ocra, scrollbar affiancata alle colonne, quantità visualizzate arrotondate all'unità, numeri a destra, colonna «Si riempie tra» in `hh:mm:ss` quando stimabile, spiegazione della capacità al sorvolo.
- In **Edifici**: costi, produzioni, consumi e capacità con elementi omogenei (icone 14 px, testo 12 px), diagnosi operative e suggerimenti per spazio insufficiente.

## 3. Economia e militari implementati

- Filiera civile: Tronchi → Tavole; Pietra; Pesce → Cibo; Grano/Acqua/Farina → Pane. Case piccole/grandi, crescita, magazzini e recupero da blocchi produttivi.
- Industria: minerali di Ferro, Carbone e Oro; fonderie; **Forgia** (1 Ferro + 1 Carbone → 2 Spade/s per addetto); **Zecca** (1 Oro + 1 Carbone → 3 Monete/s per addetto). Contesa del Carbone regolata dalla priorità delle fonderie e della Forgia/Zecca.
- **Caserma**: costo 280 Tavole + 200 Pietra; prerequisito Forgia; nessun addetto permanente; 2 Spade per recluta; **2 posti di formazione per Caserma**, **60 secondi per gruppo**, coda FIFO.
- Reclute e soldati **continuano a occupare alloggi e consumare le razioni** della popolazione civile. Si possono annullare solo reclute in attesa, rimborsando le Spade se c'è capacità; si possono congedare soldati pronti senza rimborso Spade. Nessuna perdita definitiva al momento.
- La scheda **Esercito** funziona nello scenario civile e rimane nascosta nella vecchia Foundation. Non ci sono ancora Cantiere, navi, spedizioni, conquiste, Castello, vittoria o classifica finale.

## 4. Persistenza e compatibilità

- **SQLite user_version 2 e snapshot v2** per `civil-growth`; salvataggio manuale, automatico periodico e alla chiusura ordinata; **nessuna produzione a gioco chiuso**.
- Politica **S1 approvata**: conversioni di ruleset dichiarate da `civil-growth-1` fino a `-12`, con **backup originale prima della prima sovrascrittura**. Factory v4…v11 congelate per benchmark/regressioni; niente retro-modifiche per aggiungere contenuti.
- La conversione v11→v12 aggiunge **un solo Municipio** e **non ripete** gli stock iniziali. Conserva tempo, progressi frazionari, popolazione, risorse, edifici, militari e backup.
- Lo stato militare è richiesto dalle versioni `civil-growth-10`, `-11`, `-12`; in `-9` non è presente e viene inizializzato soltanto dal passo di migrazione dichiarato. Nessun salvataggio sconosciuto può essere accettato con default silenziosi.
- Le partite Windows dell'utente sono in `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3`. **Non usarle per test o modifiche automatiche.** I test hanno file temporanei/privati.

## 5. Validazione — distinzione essenziale

| Evidenza | Stato |
| --- | --- |
| Ultima prova Windows con log completo conservato in documentazione precedente | Gate storico fino a **206/206** casi, relativo a una versione molto precedente |
| Conferma successiva dell'utente | «Test passati» dopo la correzione **v45**; non sono disponibili qui TRX/JSON di quella prova |
| Tentativi Windows intermedi v48–v50 | Problemi di compilazione/test riportati, corretti nei pacchetti successivi; **non** certificano v55 |
| **v55 / v57** | Audit statico positivo (XML, nomi XAML, 38 icone, riferimenti progetti, link); **.NET SDK assente qui** |
| **287 test xUnit** | **Numero atteso, non eseguito/attestato per v55/v57** |
| Benchmark, smoke nativi, render su Windows | **Gate corrente ancora pendente** |

Dettagli e checklist: [V55_STATIC_AUDIT.md](validation/V55_STATIC_AUDIT.md). Al prossimo controllo eseguire `.\validate.cmd` su Windows dalla radice del progetto e registrare output, errori e riepilogo. Verificare visivamente 1040×740 e 1240×850. **Non dichiarare tutto verde in assenza del nuovo log.**

## 6. Decisioni e priorità

**Già approvate:** 22 tipi edificabili per la prima campagna, Castello conclusivo, **almeno una conquista obbligatoria** per sbloccarlo; continuità salvataggi S1; regole A di Ferro/Oro/Forgia/Zecca/Caserma e politiche militari M-01…M-04; Municipio e depositi distribuiti; grafica monospace retro e liste con diagnosi. Carne esclusa dalla prima campagna, demolizione rinviata.

**Non approvate:** contratto navale **N-01…N-07**, valori economici e comportamento delle navi in [scheda M5.4 v55](design/M5_4_DECISION_GATE_V55.md); requisiti ed esiti di missioni/conquiste; dettagli Castello e vittoria; bilanciamento globale/classifica. Le **8 ore efficienti / 32+ ore inefficienti** sono **obiettivi**, non tempi misurati del gioco completo.

**Sequenza operativa:** (1) gate Windows v55/v57; (2) eventuali bug e regressioni; (3) approvazione/modifica N-01…N-07; (4) solo allora M5.4a Core/Application, M5.4b SQLite/Desktop, M5.4c preflight, possibile nuovo `civil-growth-13`; (5) M5.5–M5.7 spedizioni, conquista, Castello; (6) M6–M7 conclusione/bilanciamento/polish. Vedere [NEW_CHAT_HANDOFF.md](NEW_CHAT_HANDOFF.md) e [ROADMAP.md](ROADMAP.md).
