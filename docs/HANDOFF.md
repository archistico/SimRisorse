# Handoff

Aggiornato il **2026-10-27**. Punto di ripresa corrente; per le fasi precedenti vedere lo [storico dell'handoff](HANDOFF_HISTORY.md), il [changelog](../CHANGELOG.md) e i [rapporti di verifica](README.md#verifiche).

## Stato verificato

**M0–M4 completate; M5.1a Ferro e M5.1b Oro verificate.** Scenario `civil-growth-7`, **17 tipi di edifici e 14 risorse**. I cinque tipi futuri della prima campagna sono Forgia, Zecca, Caserma, Cantiere e Castello.

L'ultimo `validate.cmd` ha completato restore, build Release con **zero avvisi/errori**, **206/206 test**, 16 cronologie più crescita accelerata, quattro benchmark e smoke nativi Windows dei due scenari.

| Suite | Test superati |
| --- | ---: |
| Core | 67 |
| Application | 61 |
| Infrastructure | 30 |
| Desktop | 48 |
| **Totale** | **206** |

Run: `899f6ff3e8ff41288330c3d26b4c4ea7`, sotto `.local-cache/m4/runs/`. Generati **19 PNG**: 17 render richiesti dal manifest più due render del dettaglio. [Report corrente dell'interfaccia](validation/DESKTOP_UI.md). La presente revisione della documentazione non modifica il codice né costituisce una nuova esecuzione del gate.

## Decisioni già ricevute

- **Prima campagna:** i [22 tipi della scheda dedicata](design/FIRST_CAMPAIGN_PROPOSAL.md); gli altri 21 riferimenti sono fuori.
- **Vittoria:** possedere tutti i 22 tipi, con il Castello come costruzione conclusiva; gli edifici iniziali contano.
- **Conquista:** una missione obbligatoria sblocca il Castello. Isola, costi, forze, durata ed esiti restano da scegliere.
- **Demolizione:** rinviata. Nessun rimborso approvato; Carne fuori dalla prima campagna.
- **Salvataggi S1:** continuare le partite civili con conversione e backup, senza uno slot campagna distinto.
- **Ferro e Oro:** configurazioni A approvate con risposte separate. Il Ferro mantiene la priorità sul Carbone scarso.
- **Grafica:** base retro monospace approvata; menu laterale più largo, card abitanti e scorte, costi prima della produzione, icone delle risorse e hover ocra.

Non richiedere nuovamente queste scelte. Le [decisioni residue D0](design/D0_DECISION_BOARD.md) riguardano le nuove fasi, senza riaprire quelle acquisite.

## Regole economiche correnti

| Struttura | Costo: legno lavorato / pietra | Ricetta per addetto e secondo | Prerequisiti |
| --- | ---: | --- | --- |
| Miniera di ferro | 120 / 80 | → 2 minerale di ferro | Casa grande |
| Miniera di carbone | 130 / 90 | → 2 carbone | Casa grande |
| Fonderia di ferro | 200 / 150 | 2 minerale di ferro + 1 carbone → 1 ferro | Miniere di ferro e carbone |
| Miniera d'oro | 180 / 120 | → 1 minerale d'oro | Casa grande |
| Fonderia d'oro | 220 / 170 | 1 minerale d'oro + 1 carbone → 1 oro | Miniere d'oro e carbone |

Un lavoratore per edificio; scorte industriali iniziali zero, capacità 1.000 e +500 per Magazzino. Il materiale estratto è utilizzabile dal secondo successivo. Il pianificatore economico del Core resta quello esistente.

La strategia `gold` raggiunge il primo Oro a **1.502 s**, con Ferro 401, Carbone 402 e popolazione 33; a otto ore Oro e Ferro sono a 1.000, popolazione 48. Sono cronologie di riferimento, non prove di ottimalità. [Ferro](validation/M5_1A_IRON.md), [Oro](validation/M5_1B_GOLD.md).

## Compatibilità da preservare

- Slot civile e formato SQLite/snapshot **v2**. Conversioni concatenate `civil-growth-1…-6 → -7`, nuovi contenuti a zero e backup dell'originale alla prima scrittura.
- Conservare scorte precedenti, lavoratori, Magazzini, tempo e frazioni della crescita. Rifiutare scorte industriali positive prima della versione che le introduce.
- Cataloghi storici v4/v5 per i benchmark e v6 per le verifiche del Ferro; non sostituirli con il catalogo corrente.
- L'[esperimento A/B](../experiments/industry-probe/README.md) resta separato, su colonia sintetica v5.
- Nessuna produzione offline; salvataggi di test e smoke separati dalle partite personali. [Formato dei salvataggi](SAVE_FORMAT.md).

## Interfaccia corrente

Sidebar da **240 px** con Edifici/Risorse e Crescita civile; card condivise di abitanti/posti/liberi e scorte. Dettaglio: descrizione, costi in elenco, produzione o effetto, consumi. Costi e produzione condividono testo monospace normale da **12 px** e icone risorsa da **14 px**.

Hover dei pulsanti abilitati nello stesso ocra della struttura selezionata (`#E6D5AC`), testo scuro; disabilitati grigi e geometria stabile. Font, pittogrammi e regole visive sono descritti nel [contratto grafico](PIXEL_ART_UI.md).

Il [README](../README.md) include ora uno screenshot versionato in [docs/screenshots](screenshots/README.md), distinto dalle immagini del gioco di riferimento.

## Prossimo lavoro

1. **M5.1c:** confronto esteso di scala e contesa del Carbone, mantenendo i parametri già approvati.
2. Prima di M5.2, scegliere parametri di Forgia/Zecca e contratti d'uso di Spade e Monete.
3. Poi popolazione militare, Caserma, Cantiere, flotta, missione obbligatoria e vittoria, secondo la [roadmap](ROADMAP.md).
4. DPI fisici, altre piattaforme e accessibilità restano verifiche di rifinitura; non considerarli già coperti dai render headless.

Per ripartire: leggere questa pagina, preservare le modifiche Git presenti e usare i comandi della [guida di sviluppo](DEVELOPMENT.md).

Riordino documentale verificato: **62 file Markdown e 468 collegamenti locali**, ancore comprese, senza errori. Screenshot del README identico al render del gate, 1240×850, versionabile in Git. Rapporto locale: `.local-cache/docs-audit.json`.
