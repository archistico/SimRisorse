# SimRisorse — D0: scheda operativa delle decisioni prima di M5

Data: 2026-10-27 · aggiornamento M5.1b e interfaccia · **Scheda delle scelte ricevute e delle proposte ancora aperte.**

**Decisioni successive registrate:** M4 chiusa; prototipo monospace approvato, demolizione rinviata. D0.2: possedere tutti i 22 tipi, Castello conclusivo; D0.3: una conquista obbligatoria sblocca il Castello. Scelta **S1** e configurazione **A per la sola M5.1a**, ora [implementata e verificata](../validation/M5_1A_IRON.md). Restano aperti i parametri delle fasi successive, senza riaprire le decisioni già ricevute.

**M5.1b successiva:** configurazione **A dell'Oro scelta separatamente e verificata**, con Ferro prioritario sul carbone scarso. Corrente `civil-growth-7`, 17 edifici / 14 risorse; gate di integrazione Oro da **204 test** e [gate corrente dell'interfaccia da **206 test**](../validation/DESKTOP_UI.md). [Report Oro](../validation/M5_1B_GOLD.md). Le scelte già ricevute non si riaprono; il confronto esteso di scala M5.1c precede i nuovi contratti di Forgia/Zecca.

Questa pagina serve a prendere le decisioni una alla volta. Il [dossier completo D0](PRE_M5_DECISION_GATE.md) conserva l'inventario dei 43 riferimenti e le alternative ragionate; qui sono raccolti ordine, responsabilità e condizioni per iniziare il codice. Le preferenze tecniche sottostanti **non sono risposte già fornite dall'utente**.

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
| **D0.4** Militari | Soldati/reclute occupano case e mangiano? Coda o formazione istantanea? Congedi, perdite e rimborsi? | Unica popolazione contabilizzata; **B: coda** come prima prova, non ancora decisa | `totale = civili impiegati + civili liberi + reclute + soldati`, senza doppioni; casi di pausa/save e transizioni |
| **D0.5** Tempo | Velocità 1:1 o accelerabile? Come contano pause, riprese, livello difficoltà e punteggio? | Cronometrare **solo secondi simulati validi**; scegliere esplicitamente eventuale accelerazione | Cronologie riproducibili, classifica confrontabile, niente avanzamento offline |
| **D0.6** Raggiungibilità | Quale percorso di materiali, addetti, capacità e sblocchi rende possibile il primo obiettivo e la vittoria? | Grafo senza cicli, prova dei primi lotti a scorte nuove zero | Almeno una cronologia fattibile per costi, magazzini, cibo e forza lavoro; precede M5.1 |
| **D0.7** Interfaccia 8-bit | **Base approvata dall'utente**, incluso il font monospace | Conservare elenchi/schede, pittogramma più testo e geometria stabile | Gate tecnico e giudizio positivo acquisiti; DPI fisici nella rifinitura |
| **D0.8** Spade/Monete | Le **Spade si consumano** all'arruolamento o equipaggiano i soldati? Le **Monete sono risorsa per comandi specifici** o valuta di tutta l'economia? | Primo modello più contenuto da valutare: Spade consumate, Monete come scorta mirata | Contratti di pagamento atomico, annullamento e save; richiesto prima di M5.2–M5.4 |

**Non risolto con questa tabella:** prezzi, ricette e addetti delle fasi future, durata addestramento, eventuale rimborso della demolizione rinviata, numero/parametri delle isole, probabilità di fallimento e uso coloniale delle Monete. Vanno scelti con i dossier specifici, senza inferirli dalle illustrazioni. La Carne è già fuori dalla prima campagna.

## 3. Ordine di approvazione e dipendenze

1. **M4 chiusa:** gate tecnico, benchmark e revisione dei render acquisiti; giudizio utente positivo sul monospace, Carne fuori dalla prima campagna e demolizione rinviata. La chiusura non approva il rimborso del 25% o nuove ricette.
2. **D0.1–D0.3 scelti nel loro perimetro:** catalogo da 22, possesso al termine e Castello dopo una conquista obbligatoria. Dimensionare le singole fasi senza introdurre automaticamente altri contenuti.
3. **Contratti trasversali, D0.4, D0.5 e D0.8:** conservazione popolazione, tempo, consumi militari e significato delle Monete. Servono per progettare interfacce e persistenza corrette; non implicano già Caserma, Forgia o Zecca.
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
| **M5.1c — Prossima** | Confronto esteso di scala e contesa del carbone | Strategie reali, contatori riconciliati, nessun parametro nuovo implicito |
| **M5.2a–c** | Forgia, Zecca, Spade, Monete (**+2/+2**), utilizzo da definire | D0.8 approvata, transazioni e produzione a fasi/contemporanea confrontate, mai saldo negativo |
| **M5.3a–c** | Caserma, arruolamento, addestramento, conservazione persone | D0.4 approvata, code deterministiche, nessuna duplicazione su salvataggio/congedo |
| **M5.4** | Cantiere, navi e trasporto: [prefattibilità](M5_4_NAVAL_PREFEASIBILITY.md); costi, riutilizzo e capienza **non approvati** | Primo acquisto raggiungibile, nave salvabile, preflight senza doppio impiego, nessuna missione anticipata |
| **M5.5–M5.7** | Esplorazioni, isole, colonie, castelli e strutture avanzate **solo se scelti** | Missioni a tempo simulato, obiettivi e ricompense idempotenti, scenario completo salvabile |
| **M6** | Vittoria, classifica locale, confronto di strategie complete e bilanciamento | Percorso reale alla vittoria; obiettivi **8 h / 32+ h** valutati con misure, non stimati a priori |
| **M7** | Qualità, accessibilità, regressioni, packaging Windows | Verifica pubblicabile, migrazioni e interfaccia robuste |

Le matrici tecniche estese per [M5.1](M5_1_INDUSTRY_PREFEASIBILITY.md), [M5.2](M5_2_FORGE_MINT_PREFEASIBILITY.md), [M5.3](M5_3_ARMY_PREFEASIBILITY.md) e [M5.4](M5_4_NAVAL_PREFEASIBILITY.md) restano la fonte dei *casi da testare*, non di costi adottati. Gli eventuali **15/12, 17/14 e 19/16** sono conteggi *ipotetici* di edifici/risorse se tutte le proposte precedenti fossero accettate; il Cantiere e le navi sono ulteriori candidati separati.

## 5. Cosa chiedere all'utente quando può rivedere il progetto

- **Adesso**, confronto di scala M5.1c; i prezzi A di Ferro e Oro sono già adottati.
- **Prima di M5.2**, ruolo di Spade e Monete e relativi contratti.
- **Prima di M5.3–M5.6**, popolazione militare, reclutamento, trasporto, durata ed esiti della conquista già scelta come obbligatoria.
- **A ogni incremento**, raggiungibilità e compatibilità S1; completare la politica temporale e di classifica prima della funzione di vittoria.

Prossimo lavoro: confronto di scala M5.1c. Vittoria, conquista, base grafica, rinvio demolizione, S1 e parametri A di Ferro/Oro sono già scelti. Contratti militari/navali e Spade/Monete vanno definiti prima delle rispettive implementazioni.
