# Handoff

Aggiornato il **2026-11-02**, checkpoint **v40 / M5.2c Verificato — M5.2 chiusa**. [Report corrente](validation/M5_2C_FORGE_MINT.md), [storico](HANDOFF_HISTORY.md), [roadmap](ROADMAP.md).

## Stato verificato

M0–M4 completate; **M5.1 e M5.2 Verificate**. Regole **`civil-growth-9`**, **19 tipi di edifici / 16 risorse**. Restano Caserma, Cantiere e Castello per i 22 tipi della prima campagna. Il prossimo incremento è il contratto M5.3 Caserma/reclutamento: mancano scelte militari, non le produzioni di Spade/Monete.

Gate completo `validate.cmd`: **260/260 test**, Release **zero avvisi/errori**, raggiungibilità dei due cataloghi, **23 cronologie più crescita accelerata**, **sei benchmark**, pubblicazione e smoke nativi Windows dei due scenari. Run **`1c74597068ab4f96b4f6fd6d30518b45`**. TRX e **33 PNG** (31 richiesti più due dettagli); nessuna nuova modifica UI in v40. Il sesto benchmark e le tre cronologie `crafting` sono conservati anche nella cartella della run, con percorsi in `validation.json`.

| Suite | Superati |
| --- | ---: |
| Core | 67 |
| Application | 97 |
| Infrastructure | 35 |
| Desktop | 61 |
| **Totale** | **260** |

## M5.2c: misure e limiti

- Nuovo `--benchmark-forge-mint` sul **catalogo corrente**, senza cambiare prezzi, ricette o regole: **nove profili e quattro guardie**, ogni caso a passi interi e 250+750 ms, con ripristino di un checkpoint frazionario e confronto finale di metriche/snapshot.
- Obiettivo controllato **240 Spade /360 Monete**: **241 s** a fasi con una miniera, **122 s** paralleli con due. Acquisti industriali **1.300/950** contro **1.430/1.040** Legno lavorato/Pietra. Con estrazione attiva, picco industriale **5** contro **8** addetti, esclusi i sette addetti alimentari preesistenti.
- Colonia sintetica comune 40/40, due Magazzini, Cibo/Pane attivi; le infrastrutture civili sono preesistenti ed escluse dai costi. Controlli con minerali precaricati; prove sostenute da **1.800 s e otto ore** con miniere minerali attive da scorte zero, contabilità di tutte le risorse a ogni secondo e nessuna carenza alimentare.
- A 30 minuti: fasi **1.680 Spade /1.998 Monete**, parallelo **2.000/1.998**, una miniera con tutto attivo **0/0**. Nel terzo caso le prime produzioni arrivano a **2.002 s** dopo il riempimento dei depositi dei metalli; obiettivo a **2.240 s**. A otto ore tutti i profili raggiungono **2.000/1.998**, senza lotti parziali.
- Nuova strategia **`crafting` dalla nuova partita**: Oro a 1.500 s; seconda miniera Carbone, Forgia e Zecca a **2.200 s**, acquisti aggiuntivi **580/430**. Primo lotto a **2.201 s**; **240 Spade e 360 Monete a 2.320 s**, popolazione 48. A otto ore **1.000/999** alla capienza base. Cronologia fattibile, non ottima né misura della durata della campagna completa.
- Test aggiunti: replay integrato da checkpoint a 2.200,5 s; SQLite con Forgia/Zecca attive, prodotti e residuo di mezzo secondo. Il Carbone rimasto viene assegnato prima alla Forgia anche dopo il caricamento, senza spese o prodotti duplicati.

Artefatti: `.local-cache/m4/validation.json`, `crafting-validation.log`; nella run `forge-mint-benchmark.json`, `crafting-start.json`, `crafting-target.json`, `crafting-8-hours.json`. Una copia corrente è anche sotto `.local-cache/m4/`. L'esperimento A/B storico resta separato su v7; B non entra nel catalogo giocabile e non è stato rieseguito in v40.

## Regole, salvataggi e UI acquisiti

- Forgia: **240/180**, 1 addetto, Fonderia di ferro posseduta, **1 Ferro +1 Carbone → 2 Spade/s**, priorità 100.
- Zecca: **210/160**, 1 addetto, Fonderia d'oro posseduta, **1 Oro +1 Carbone → 3 Monete/s**, priorità 101, dopo Ferro 90 e Oro 91.
- Spade e Monete iniziali zero, capacità **1.000 +500 per Magazzino**, lotti interi. Spade consumate dal futuro arruolamento; Monete per pagamenti espliciti, non valuta generale.
- S1, stesso slot e SQLite/snapshot **v2**; conversioni v1–v8→v9 con backup originale alla prima scrittura. Nuovi contenuti a zero, scorte, lavoratori, persone, tempo e frazioni preservati. Nessun cambio di regole/formato in v40.
- Cataloghi storici v4–v8 preservati. UI retro, ordinamento italiano, saldi e stima riempimento; effetti v38 nello stesso elenco con icone e monospace 12 px di costi/produzione/consumi.
- Campagna da 22 tipi, vittoria col possesso di tutti e Castello conclusivo dopo conquista obbligatoria; grafica e ruolo della conquista già scelti. Carne fuori e demolizione **Rinviata**. Non richiedere nuovamente queste scelte.

## Prossimo lavoro e decisioni aperte

1. **M5.3, prima del codice militare:** aggiornato il [dossier Caserma](design/M5_3_ARMY_PREFEASIBILITY.md). Scegliere prezzo/prerequisiti Caserma, numero di Spade per recluta, modello di addestramento (immediato/coda/posti simultanei), durata e capacità, alloggio/razioni, cancellazione/congedo. Il consumo delle Spade all'arruolamento è già deciso; non riaprirlo.
2. Dopo il contratto, M5.3a comando atomico e popolazione condivisa, M5.3b addestramento e salvataggi, M5.3c impatto civile. Non introdurre valori plausibili per deduzione. Perdite e missioni richiedono contratti propri, senza anticipare navi o combattimenti.
3. Poi M5.4 flotta, M5.5–M5.7 missione/conquista/Castello e M6 vittoria. Le circa 8 ore efficienti /32+ ore lente restano obiettivi del gioco completo.

Restano aperti DPI fisici, altre piattaforme e accessibilità. [Guida di sviluppo](DEVELOPMENT.md).
