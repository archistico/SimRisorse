# SimRisorse — D0: scheda operativa delle decisioni prima di M5

Data: 2026-10-28 · contratto M5.2 approvato e implementato in due incrementi **v38 Forgia / v40 Zecca**, gate Windows pendente · **Scheda delle scelte ricevute e delle proposte ancora aperte.**

**Stato aggiornato v48 (prevale sulle tappe storiche sotto):** `civil-growth-10`, **20 edifici e 16 risorse**. Ferro/Oro verificati, Forgia/Spade e Zecca/Monete implementati; **test v45 dichiarati passati dall'utente**, senza nuova trascrizione di benchmark/smoke. Contratto A Caserma approvato: 280 Tavole/200 Pietra, prerequisito Forgia, 2 Spade/recluta, 2 posti/Caserma, 60 s/gruppo. **M-01…M-04 sono approvate**: alloggi e razioni condivisi, congedo senza rimborso, annullamento delle sole reclute in coda con restituzione condizionata alla capienza e nessuna perdita definitiva finché non saranno progettate le missioni. **v47 collega il Core militare a partita, snapshot, SQLite e migrazione S1; v48 rende disponibili i comandi nell'interfaccia Esercito**, mantenendo i benchmark storici v9 congelati. [Gate v48](../validation/M5_3C_MILITARY_DESKTOP.md), **276 xUnit attesi, non ancora verificati**. Restano le regole navali e delle missioni successive.

**Decisioni successive registrate:** M4 chiusa; prototipo monospace approvato, demolizione rinviata. D0.2: possedere tutti i 22 tipi, Castello conclusivo; D0.3: una conquista obbligatoria sblocca il Castello. Scelta **S1** e configurazione **A per la sola M5.1a**, ora [implementata e verificata](../validation/M5_1A_IRON.md). Restano aperti i parametri delle fasi successive, senza riaprire le decisioni già ricevute.

**M5.1b successiva:** configurazione **A dell'Oro scelta separatamente e verificata**, con Ferro prioritario sul carbone scarso. Corrente `civil-growth-7`, 17 edifici / 14 risorse; gate di integrazione Oro da **204 test** e [ultimo gate dell'interfaccia da **206 test**](../validation/DESKTOP_UI.md). [Report Oro](../validation/M5_1B_GOLD.md). **M5.1c preparata, gate completo da acquisire**; il 28/10 l'utente ha **approvato la proposta completa A di M5.2** descritta nel [dossier](M5_2_FORGE_MINT_PREFEASIBILITY.md), senza introdurre nuovi contenuti nel gioco.

Questa pagina serve a prendere le decisioni una alla volta. Il [dossier completo D0](PRE_M5_DECISION_GATE.md) conserva l'inventario dei 43 riferimenti e le alternative ragionate; qui sono raccolti ordine, responsabilità e condizioni per iniziare il codice. Le proposte ancora indicate come tali **non sono risposte già fornite dall'utente**.

**D0.1 approvata il 2026-10-27:** prima campagna con i **22 tipi scelti** nella [scheda del catalogo e della vittoria](FIRST_CAMPAIGN_PROPOSAL.md), altri 21 riferimenti fuori. Dopo M5.1b sono **17 tipi presenti e 5 futuri**. Vittoria, S1 e parametri A di Ferro e Oro sono stati scelti con risposte distinte.

## 1. Dove siamo davvero

| Elemento | Stato e riferimento storico | Conseguenza |
| --- | --- | --- |
| M0–M3 | Completate; test confermati nei rispettivi checkpoint | Baseline da preservare |
| M4 fino a v24/M4.10 | **Gate Windows riferito superato dall'utente**, 174 xUnit attesi; nessun TRX/JSON disponibile qui | Ultima conferma utilizzabile |
| Prototipo 8-bit e monospace | **Gate verificato e giudizio utente positivo** | Base grafica accettata; prove DPI fisici da ampliare in rifinitura |
| v26/M4.12 | Lettore PowerShell **eseguito**, quattro benchmark consolidati | [Misure e limiti](../validation/M4_MEASURED_RESULTS.md), input conservati nel run |
| v27/M4.13 | Audit matematico delle Case, **non benchmark eseguito** | Confrontare poi con dati reali |
| v28/M4.14 | Introduzione di questa scheda e allineamento documentale | Le scelte successive sono registrate sotto; contratti militari/navali ancora aperti |
| v29/M4.15 | Prefattibilità navale M5.4; **solo documentazione** | Decisioni e test futuri, nessuna nave introdotta |
| Regole e dati | `civil-growth-7` attiva; snapshot SQLite v2, 17 tipi di edifici e 14 risorse | S1 e backup verificati; cataloghi v4/v5/v6 preservati |
| Catalogo della prima campagna | **22 tipi scelti dall'utente**, di cui 17 implementati e 5 futuri; altri 21 riferimenti fuori | Vittoria e ruolo della conquista scelti; parametri successivi da fissare |

**Vincoli confermati:** gestione con elenchi/schede, niente posizionamento su mappa; identità visiva pixel art retro 8-bit essenziale; pausa senza progresso offline; la durata di circa 8 ore con strategia efficiente e di 32 ore o più con strategie lente è **un obiettivo**, non un risultato misurato.

## 2. Otto decisioni da discutere, senza assumerle

| ID | Domanda concreta all'utente | Indicazione tecnica **provvisoria** | Blocca / criterio di chiusura |
| --- | --- | --- | --- |
| **D0.1** Catalogo | **Approvata:** i 22 tipi della scheda prima campagna, altri 21 fuori | 17 presenti + 5 futuri; parametri successivi da dimensionare | Perimetro scelto; raggiungibilità da verificare per ogni incremento |
| **D0.2** Vittoria | **Approvata:** possedere tutti i 22 tipi, Castello conclusivo; edifici iniziali validi | Contratto scelto, funzione di vittoria ancora da implementare | Sette casi di accettazione nella scheda prima campagna |
| **D0.3** Isole | **Ruolo approvato:** una conquista obbligatoria sblocca il Castello | Isola, forze, costi, durata ed esiti da dimensionare in M5.5–M5.6 | Primo viaggio senza dipendenze da risorse coloniali irraggiungibili |
| **D0.4** Militari | **Approvata M5.3 il 28/10:** modello A e regole M-01…M-04 di popolazione unica, razioni, congedi, annullamenti e nessuna perdita definitiva | [Contratto scelto](M5_3_DECISION_GATE_V43.md), [integrazione v47](../validation/M5_3B_MILITARY_SESSION_SAVE.md) | `totale = civili impiegati + civili liberi + reclute + soldati`; gate Windows ancora pendente |
| **D0.5** Tempo | Velocità 1:1 o accelerabile? Come contano pause, riprese, livello difficoltà e punteggio? | Cronometrare **solo secondi simulati validi**; scegliere esplicitamente eventuale accelerazione | Cronologie riproducibili, classifica confrontabile, niente avanzamento offline |
| **D0.6** Raggiungibilità | Quale percorso di materiali, addetti, capacità e sblocchi rende possibile il primo obiettivo e la vittoria? | Grafo senza cicli, prova dei primi lotti a scorte nuove zero | Almeno una cronologia fattibile per costi, magazzini, cibo e forza lavoro; precede M5.1 |
| **D0.7** Interfaccia 8-bit | **Base approvata dall'utente**, incluso il font monospace | Conservare elenchi/schede, pittogramma più testo e geometria stabile | Gate tecnico e giudizio positivo acquisiti; DPI fisici nella rifinitura |
| **D0.8** Spade/Monete | **Approvata per M5.2/M5.3:** Spade consumate, **2 per recluta** con modello A; Monete scorta per acquisti specifici, non valuta generale | Lotti interi, depositi ordinari, priorità fisse; quantità delle Monete per nave rinviata a M5.4 | Verificare F1–F10, pagamenti atomici nei moduli futuri, annullamento e salvataggio |

**Ancora da decidere:** alloggi/razioni militari, cancellazioni e congedi, eventuali perdite, Monete per nave, eventuale rimborso della demolizione rinviata, numero/parametri delle isole, probabilità di fallimento e uso coloniale delle Monete. **Prezzo Caserma, prerequisito Forgia, 2 Spade, 2 posti e 60 s sono approvati.** Prezzi, ricette, addetti, priorità e capienza di Forgia e Zecca sono implementati nel contratto M5.2. La Carne è già fuori dalla prima campagna.

## 3. Ordine di approvazione e dipendenze

1. **M4 chiusa:** gate tecnico, benchmark e revisione dei render acquisiti; giudizio utente positivo sul monospace, Carne fuori dalla prima campagna e demolizione rinviata. La chiusura non approva il rimborso del 25% o nuove ricette.
2. **D0.1–D0.3 scelti nel loro perimetro:** catalogo da 22, possesso al termine e Castello dopo una conquista obbligatoria. Dimensionare le singole fasi senza introdurre automaticamente altri contenuti.
3. **Contratti trasversali:** D0.8 è ora approvata nel perimetro M5.2; D0.4 e D0.5 e i quantitativi dei consumi militari restano da scegliere. Non implicano già Caserma, Forgia o Zecca nel catalogo.
4. **Fattibilità incrementale D0.6:** M5.1a ha un percorso giocato da risorse iniziali a primo ferro in 1.102 s. La raggiungibilità della campagna completa richiederà i parametri militari/navali successivi. Il linguaggio visivo è accettato.
5. **S1 scelta e verificata:** estensione di `civil-growth`, introdotta con `-6` e ora a `-7`, conversione e backup. File/snapshot v2 conservati; eventuali nuovi stati militari/navali richiederanno valutazioni e test propri.

### Contratto dei salvataggi: non ripetere il numero di versione

| Percorso candidato | Politica proposta | Che cosa richiede per l'approvazione |
| --- | --- | --- |
| **S1 — Scelta dall'utente** | Corrente **`civil-growth-7`**, snapshot SQLite v2 compatibile e verificato | Conversioni concatenate `-1…-6`, backup originale, round-trip e pausa/ripresa testati |
| **S2 — Nuovo scenario campagna** | Mantenere lo scenario civile e creare ID distinto; trasferire partite soltanto mediante importazione visibile | Contratto tra identità di scenario, checkpoint, nuova UI e copie originali, senza migrazione silenziosa |

**Oggi vale `civil-growth-7`, con S1 scelta e M5.1b verificata.** V4/v5 sono conservate per i benchmark e v6 per le verifiche di compatibilità del primo ferro. Il [formato salvataggi](../SAVE_FORMAT.md) descrive anche la conversione dell'Oro.

## 4. Sequenza realizzativa *dopo* le approvazioni

| Fase | Aggiunte adottate o proposte secondo lo stato | Gate per passare alla successiva |
| --- | --- | --- |
| **M5.1a — Verificata** | Configurazione A scelta: miniera ferro, miniera carbone, fonderia; **15 edifici / 12 risorse** al relativo gate | 191/191 test, primo ferro, deposito pieno, backup e ripresa verificati |
| **M5.1b — Verificata** | Oro configurazione A, due nuovi edifici e due risorse | 204/204 test, contesa, backup e ripresa verificati |
| **M5.1c — Preparata, test pendenti** | Benchmark con 5 profili, contabilità per secondo e replay frazionari | Gate Windows prima di chiudere M5.1 e avviare codice M5.2 |
| **M5.2a–d — Implementate/preparate, gate pendente** | Forgia, Zecca, Spade, Monete (**+2/+2**) in v9; benchmark sintetico v41 e prima colonia v42 | Contratto di design acquisito; transazioni e produzione a fasi/contemporanea ancora da verificare su Windows, mai saldo negativo |
| **M5.3a–c — Contratto A scelto, Core iniziale isolato v46** | Caserma, arruolamento, addestramento, conservazione persone | [15 nuovi test e primo componente](../validation/M5_3A_MILITARY_CORE.md) da validare; poi integrare Core/GameSession, S1/SQLite, GUI e scelte M-01…M-04 |
| **M5.4** | Cantiere, navi e trasporto: [prefattibilità](M5_4_NAVAL_PREFEASIBILITY.md); costi, riutilizzo e capienza **non approvati** | Primo acquisto raggiungibile, nave salvabile, preflight senza doppio impiego, nessuna missione anticipata |
| **M5.5–M5.7** | Esplorazioni, isole, colonie, castelli e strutture avanzate **solo se scelti** | Missioni a tempo simulato, obiettivi e ricompense idempotenti, scenario completo salvabile |
| **M6** | Vittoria, classifica locale, confronto di strategie complete e bilanciamento | Percorso reale alla vittoria; obiettivi **8 h / 32+ h** valutati con misure, non stimati a priori |
| **M7** | Qualità, accessibilità, regressioni, packaging Windows | Verifica pubblicabile, migrazioni e interfaccia robuste |

Le matrici tecniche estese per [M5.1](M5_1_INDUSTRY_PREFEASIBILITY.md), [M5.2](M5_2_FORGE_MINT_PREFEASIBILITY.md), [M5.3](M5_3_ARMY_PREFEASIBILITY.md) e [M5.4](M5_4_NAVAL_PREFEASIBILITY.md) restano la fonte dei *casi da testare*, non di costi adottati. Gli eventuali **15/12, 17/14 e 19/16** sono conteggi *ipotetici* di edifici/risorse se tutte le proposte precedenti fossero accettate; il Cantiere e le navi sono ulteriori candidati separati.

## 5. Cosa chiedere all'utente quando può rivedere il progetto

- **Adesso**, eseguire quando possibile il gate Windows di v42, che comprende M5.1c, Forgia/Zecca e i nuovi confronti M5.2c/d; i prezzi A di Ferro e Oro sono già adottati.
- **Contratto M5.2 deciso il 28/10:** prezzi/ricette A Forgia e Zecca, ruolo di Spade e Monete, priorità e deposito. Il [dossier aggiornato](M5_2_FORGE_MINT_PREFEASIBILITY.md) registra la scelta e confronta **241/122 s teorici** per uno stesso lotto di 240 Spade e 360 Monete. Non si tratta ancora di risultati .NET o regole implementate.
- **M5.3 approvata nel perimetro A** (Caserma, Spade e formazione); decidere ancora i punti **M-01…M-04** su razioni/alloggi, annullamenti/congedi e perdite prima dell'integrazione del gioco. Trasporto, durata ed esiti della conquista restano delle fasi successive.
- **A ogni incremento**, raggiungibilità e compatibilità S1; completare la politica temporale e di classifica prima della funzione di vittoria.

Prossimo lavoro: registrare il gate Windows **v46** sui 15 test del modello militare isolato; acquisire i dati quantitativi M5.2c/d se disponibili. Prima di integrare il modello in `GameSession` e nei salvataggi v10, decidere **M-01…M-04**. Vittoria, conquista, base grafica, rinvio demolizione, S1 e parametri A di Ferro/Oro/Forgia/Zecca/**Caserma** sono già scelti e **non vanno richiesti nuovamente**.
