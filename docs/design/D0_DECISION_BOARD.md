# SimRisorse — D0: scheda operativa delle decisioni prima di M5

Aggiornata il **2026-11-02**: contratto A M5.2 approvato il 28/10; **M5.2 completata e Verificata**, compreso confronto a fasi/parallelo. Prossimo passo: contratto militare M5.3. Scheda delle scelte ricevute e delle proposte ancora aperte.

**Decisioni successive registrate:** M4 chiusa; prototipo monospace approvato, demolizione rinviata. D0.2: possedere tutti i 22 tipi, Castello conclusivo; D0.3: una conquista obbligatoria sblocca il Castello. Scelta **S1** e configurazione **A per la sola M5.1a**, ora [implementata e verificata](../validation/M5_1A_IRON.md). Restano aperti i parametri delle fasi successive, senza riaprire le decisioni già ricevute.

**Baseline corrente:** M5.1 e M5.2 verificate, `civil-growth-9`, 19 edifici/16 risorse, **260/260 test**; [report M5.2c](../validation/M5_2C_FORGE_MINT.md). Prima del codice M5.3 servono prezzo/prerequisiti Caserma, quantità Spade, addestramento, alloggi/razioni, cancellazione/congedo. Il consumo delle Spade all’arruolamento è già scelto.

Questa pagina serve a prendere le decisioni una alla volta. Il [dossier completo D0](PRE_M5_DECISION_GATE.md) conserva l'inventario dei 43 riferimenti e le alternative ragionate; qui sono raccolti ordine, responsabilità e condizioni per iniziare il codice. Le proposte ancora indicate come tali **non sono risposte già fornite dall'utente**.

**D0.1 approvata il 2026-10-27:** prima campagna con i **22 tipi scelti** nella [scheda del catalogo e della vittoria](FIRST_CAMPAIGN_PROPOSAL.md), altri 21 riferimenti fuori. Dopo M5.2b sono **19 tipi presenti e 3 futuri**. Vittoria, S1 e parametri A di Ferro e Oro sono stati scelti con risposte distinte.

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
| Regole e dati | `civil-growth-9` attiva; snapshot SQLite v2, 19 tipi di edifici e 16 risorse | S1 e backup verificati; cataloghi v4/v5/v6 preservati |
| Catalogo della prima campagna | **22 tipi scelti dall'utente**, di cui 19 implementati e 3 futuri; altri 21 riferimenti fuori | Vittoria e ruolo della conquista scelti; parametri successivi da fissare |

**Vincoli confermati:** gestione con elenchi/schede, niente posizionamento su mappa; identità visiva pixel art retro 8-bit essenziale; pausa senza progresso offline; la durata di circa 8 ore con strategia efficiente e di 32 ore o più con strategie lente è **un obiettivo**, non un risultato misurato.

## 2. Otto decisioni da discutere, senza assumerle

| ID | Domanda concreta all'utente | Indicazione tecnica **provvisoria** | Blocca / criterio di chiusura |
| --- | --- | --- | --- |
| **D0.1** Catalogo | **Approvata:** i 22 tipi della scheda prima campagna, altri 21 fuori | 19 presenti + 3 futuri; parametri successivi da dimensionare | Perimetro scelto; raggiungibilità da verificare per ogni incremento |
| **D0.2** Vittoria | **Approvata:** possedere tutti i 22 tipi, Castello conclusivo; edifici iniziali validi | Contratto scelto, funzione di vittoria ancora da implementare | Sette casi di accettazione nella scheda prima campagna |
| **D0.3** Isole | **Ruolo approvato:** una conquista obbligatoria sblocca il Castello | Isola, forze, costi, durata ed esiti da dimensionare in M5.5–M5.6 | Primo viaggio senza dipendenze da risorse coloniali irraggiungibili |
| **D0.4** Militari | **Approvata il 03/11 (a–d, f):** modello **C, posti per Caserma**. Caserma 260 Legno lavorato/200 Pietra, 0 addetti, prerequisito Forgia; **5 Spade per recluta**; ogni Caserma addestra **1 recluta alla volta in 30 s validi**; reclute e soldati **restano residenti** (case, Cibo, Pane); **annullamento** con restituzione di persona e Spade (il tempo già svolto si perde); **congedo rinviato** alle missioni. Perdite (D0.4e) non decise | Implementata in `civil-growth-10` (v41), da verificare su Windows; [ADR 0017](../adr/0017-barracks-recruitment-and-v9-save-upgrade.md) | `totale = impiegati + liberi + reclute + soldati`, senza doppioni; 308 test attesi |
| **D0.5** Tempo | Velocità 1:1 o accelerabile? Come contano pause, riprese, livello difficoltà e punteggio? | Cronometrare **solo secondi simulati validi**; scegliere esplicitamente eventuale accelerazione | Cronologie riproducibili, classifica confrontabile, niente avanzamento offline |
| **D0.6** Raggiungibilità | Quale percorso di materiali, addetti, capacità e sblocchi rende possibile il primo obiettivo e la vittoria? | Grafo senza cicli, prova dei primi lotti a scorte nuove zero | Almeno una cronologia fattibile per costi, magazzini, cibo e forza lavoro; precede M5.1 |
| **D0.7** Interfaccia 8-bit | **Base approvata dall'utente**, incluso il font monospace | Conservare elenchi/schede, pittogramma più testo e geometria stabile | Gate tecnico e giudizio positivo acquisiti; DPI fisici nella rifinitura |
| **D0.8** Spade/Monete | **Approvata per M5.2:** Spade risorsa da consumare nell'arruolamento futuro; Monete scorta per acquisti specifici, non valuta generale | Un lotto intero per ricetta, depositi ordinari, priorità fisse; quantità d'impiego rinviate a M5.3/M5.4 | Verificare F1–F10, pagamenti atomici nei moduli futuri, annullamento e salvataggio |

**Ancora da decidere:** prezzi, ricette e addetti **dopo M5.2**, quantità di Spade per recluta e Monete per nave, durata addestramento, eventuale rimborso della demolizione rinviata, numero/parametri delle isole, probabilità di fallimento e uso coloniale delle Monete. Prezzi, ricette, addetti, priorità e capienza **di Forgia e Zecca sono approvati nel contratto M5.2**; Forgia e Zecca verificate nel gioco. La Carne è già fuori dalla prima campagna.

## 3. Ordine di approvazione e dipendenze

1. **M4 chiusa:** gate tecnico, benchmark e revisione dei render acquisiti; giudizio utente positivo sul monospace, Carne fuori dalla prima campagna e demolizione rinviata. La chiusura non approva il rimborso del 25% o nuove ricette.
2. **D0.1–D0.3 scelti nel loro perimetro:** catalogo da 22, possesso al termine e Castello dopo una conquista obbligatoria. Dimensionare le singole fasi senza introdurre automaticamente altri contenuti.
3. **Contratti trasversali:** D0.8 è ora approvata nel perimetro M5.2; D0.4 e D0.5 e i quantitativi dei consumi militari restano da scegliere. Caserma resta futura; Forgia e Zecca sono verificate nel catalogo.
4. **Fattibilità incrementale D0.6:** M5.1a ha un percorso giocato da risorse iniziali a primo ferro in 1.102 s. La raggiungibilità della campagna completa richiederà i parametri militari/navali successivi. Il linguaggio visivo è accettato.
5. **S1 scelta e verificata:** estensione di `civil-growth`, introdotta con `-6` e ora a `-9`, conversione e backup. File/snapshot v2 conservati; eventuali nuovi stati militari/navali richiederanno valutazioni e test propri.

### Contratto dei salvataggi: non ripetere il numero di versione

| Percorso candidato | Politica proposta | Che cosa richiede per l'approvazione |
| --- | --- | --- |
| **S1 — Scelta dall'utente** | Corrente **`civil-growth-9`**, snapshot SQLite v2 compatibile e verificato | Conversioni concatenate `-1…-8`, backup originale, round-trip e pausa/ripresa testati |
| **S2 — Nuovo scenario campagna** | Mantenere lo scenario civile e creare ID distinto; trasferire partite soltanto mediante importazione visibile | Contratto tra identità di scenario, checkpoint, nuova UI e copie originali, senza migrazione silenziosa |

**Oggi vale `civil-growth-9`, con S1 scelta e M5.2a/b verificate.** V4–v8 sono conservate per benchmark e compatibilità. Il [formato salvataggi](../SAVE_FORMAT.md) descrive anche la conversione della Zecca, preservando Forgia e Spade.

## 4. Sequenza realizzativa *dopo* le approvazioni

| Fase | Aggiunte adottate o proposte secondo lo stato | Gate per passare alla successiva |
| --- | --- | --- |
| **M5.1a — Verificata** | Configurazione A scelta: miniera ferro, miniera carbone, fonderia; **15 edifici / 12 risorse** al relativo gate | 191/191 test, primo ferro, deposito pieno, backup e ripresa verificati |
| **M5.1b — Verificata** | Oro configurazione A, due nuovi edifici e due risorse | 204/204 test, contesa, backup e ripresa verificati |
| **M5.1c — Verificata** | Benchmark con 5 profili, contabilità per secondo e replay frazionari | Gate Windows da 211/211 prima di M5.2a, confermato nel gate corrente |
| **M5.2a — Verificata** | Forgia/Spade in v8, 18 edifici/15 risorse | 235/235 test, primo lotto, capacità, priorità, conversioni e UI |
| **M5.2b — Verificata** | Zecca/Monete in v9, 19 edifici/16 risorse | 258/258 test, lotti interi, priorità, conversione e UI |
| **M5.2c — Verificata** | Nove profili e quattro guardie, confronto sostenuto e cronologia `crafting` | 260/260 test, 241/122 s nel confronto e 2.320 s da nuova partita; M5.2 chiusa |
| **M5.3a–c** | Caserma, arruolamento, addestramento, conservazione persone | D0.4 approvata, code deterministiche, nessuna duplicazione su salvataggio/congedo |
| **M5.4** | Cantiere, navi e trasporto: [prefattibilità](M5_4_NAVAL_PREFEASIBILITY.md); costi, riutilizzo e capienza **non approvati** | Primo acquisto raggiungibile, nave salvabile, preflight senza doppio impiego, nessuna missione anticipata |
| **M5.5–M5.7** | Esplorazioni, isole, colonie, castelli e strutture avanzate **solo se scelti** | Missioni a tempo simulato, obiettivi e ricompense idempotenti, scenario completo salvabile |
| **M6** | Vittoria, classifica locale, confronto di strategie complete e bilanciamento | Percorso reale alla vittoria; obiettivi **8 h / 32+ h** valutati con misure, non stimati a priori |
| **M7** | Qualità, accessibilità, regressioni, packaging Windows | Verifica pubblicabile, migrazioni e interfaccia robuste |

Le matrici tecniche estese per [M5.1](M5_1_INDUSTRY_PREFEASIBILITY.md), [M5.2](M5_2_FORGE_MINT_PREFEASIBILITY.md), [M5.3](M5_3_ARMY_PREFEASIBILITY.md) e [M5.4](M5_4_NAVAL_PREFEASIBILITY.md) conservano i casi di verifica e le ipotesi originarie. I prezzi A Ferro/Oro/Forgia/Zecca sono approvati; altri valori candidati restano proposte. I conteggi 15/12 e 17/14 sono baseline storiche, 18/15 la baseline Forgia e 19/16 quella corrente con la Zecca. Navi e soldati sono entità distinte dagli edifici.

## 5. Cosa chiedere all'utente quando può rivedere il progetto

- **M5.2 chiusa:** produzioni, prezzi, priorità e deposito di Forgia/Zecca acquisiti e verificati; definire ora i contratti M5.3 senza riaprire questi valori.
- **Contratto M5.2 deciso il 28/10:** prezzi/ricette A, ruolo Spade/Monete, priorità e deposito. Forgia e Zecca implementate; **241/122 s confermati nell'esperimento .NET isolato** per 240 Spade/360 Monete su checkpoint sintetico storico v7, senza chiudere il confronto M5.2c nel gioco corrente.
- **Prima di M5.3–M5.6**, popolazione militare, reclutamento, trasporto, durata ed esiti della conquista già scelta come obbligatoria.
- **A ogni incremento**, raggiungibilità e compatibilità S1; completare la politica temporale e di classifica prima della funzione di vittoria.

Prossimo lavoro: **contratto M5.3 Caserma e reclutamento**, usando il [dossier aggiornato](M5_3_ARMY_PREFEASIBILITY.md). Vittoria, conquista, base grafica, rinvio demolizione, S1 e parametri A Ferro/Oro/Forgia/Zecca sono acquisiti. M5.2c è verificata; non ripetere le scelte già ricevute.