# Handoff

Aggiornato il **2026-10-28**. Punto di ripresa corrente; per le fasi precedenti vedere lo [storico dell'handoff](HANDOFF_HISTORY.md), il [changelog](../CHANGELOG.md) e i [rapporti di verifica](README.md#verifiche).

## Stato della baseline e validazione pendente

**M0–M4 completate; M5.1a Ferro e M5.1b Oro verificate.** Scenario `civil-growth-7`, **17 tipi di edifici e 14 risorse**. I cinque tipi futuri della prima campagna sono Forgia, Zecca, Caserma, Cantiere e Castello.

L'ultimo `validate.cmd` **riportato dai documenti della copia locale dell'utente** ha completato restore, build Release con **zero avvisi/errori**, **206/206 test**, 16 cronologie più crescita accelerata, quattro benchmark e smoke nativi Windows dei due scenari. I relativi TRX e artefatti non erano nell'archivio, quindi in questo ambiente non è stata possibile una verifica indipendente di quell'esecuzione.

**Nuovo incremento M5.1c preparato:** quinto benchmark `--benchmark-industry-scale` con cinque profili di contesa del Carbone, controllo dei flussi a ogni secondo e replay a tempi frazionari. Sono stati cambiati solo simulatore, validatore e documentazione; **nessuna regola, GUI, migrazione o test xUnit è stata modificata**. [Report di progetto e valori teorici attesi](validation/M5_1C_INDUSTRY_SCALE.md). **La build e il nuovo gate Windows non sono ancora stati eseguiti.**

**Preparazione documentale successiva (v31):** ampliato il [dossier M5.2](design/M5_2_FORGE_MINT_PREFEASIBILITY.md) con scelta A/B da sottoporre all'utente, ipotesi di Spade/Monete, priorità, capienza a lotti, test F1–F10 e confronto **241 s con una miniera a fasi / 122 s con due miniere** per 240 Spade + 360 Monete, a condizioni sintetiche ideali. Sono **solo calcoli teorici**, non benchmark .NET; nessuna nuova regola approvata e nessuna modifica al codice rispetto alla v30.

**Prova sperimentale v32 preparata:** [ForgeMintProbe](../experiments/forge-mint-probe/README.md) è un progetto .NET separato dalla soluzione, con catalogo ipotetico in memoria, sei profili A/B, acquisti reali, contabilità a ogni secondo e replay a passi interi/frazionari. **Non modifica `src/`, `tests/`, `tools/`, `scripts/`, `SimRisorse.slnx` o il gioco**, e non è stato eseguito in .NET qui. Le stime aritmetiche 241/122 s sono state confermate da un modello distinto, non ancora dal nuovo eseguibile.

**Incremento v33 preparato:** la tabella delle risorse nel Desktop mostra ora i nomi in **ordine alfabetico italiano**. Si ordina una sola volta all'avvio; aggiornamenti e snapshot seguono ancora gli ID esistenti. Le tre card sintetiche preservano l'ordine storico. Aggiunti due casi xUnit Desktop (scenario base e civile); **non ancora compilati/eseguiti su .NET**. Il numero 206 indica esclusivamente l'ultimo gate confermato, non questa modifica.

**Conferma utente del 28/10:** «test passati» per la v33. Il log di `validate.cmd` non è stato fornito; il numero **208** rimane il conteggio **atteso** della suite dopo i due nuovi casi Desktop, mentre il rapporto storico da **206/206** è l'ultima evidenza dettagliata conservata. Non attribuire un esito alla prova ForgeMintProbe: è fuori dal gate ordinario.

**Incremento v34 preparato:** l'[esperimento ForgeMintProbe](../experiments/forge-mint-probe/README.md) include ora, oltre ai sei profili A/B, **sei guardie F8/F9**: lotti bloccati con 1–2 posti residui, lotti che riempiono esattamente il magazzino e precedenza del Carbone alle due fonderie. Ogni guardia confronta replay interi/frazionari e snapshot. **Non compilato né eseguito qui**, prezzi e significati economici restano candidati; gioco e suite xUnit invariati rispetto alla v33.

**Incremento v35 preparato (richiesta UI):** tabella Risorse con colonna «Si riempie tra» in `hh:mm:ss` a saldo netto positivo (stima a flusso costante), `00:00:00` se già piena, `—` con saldo nullo/negativo; quantità della sola tabella arrotondata all'unità, dati precisi e card invariati. Allineamento a destra di intestazioni e valori numerici. Consumi per addetto/secondo nel dettaglio Edifici resi come elenco con icona tramite `ResourceDetailTemplate`, identico ai costi e alle produzioni. Nuovo test numerico e regressioni visuali ampliate: **209 casi xUnit attesi**. **Non compilato né eseguito qui**; solo Desktop/test/documenti, nessuna nuova regola o migrazione.

**Incremento v36 preparato (correzione UI):** Risorse è ora un `ListBox` scrollabile autonomamente, con selezione persistente della riga nel medesimo ocra dell'elenco Edifici. Entrambi gli elenchi riservano **20 px** alla scrollbar verticale tramite classe `gutter-list` e intestazioni arretrate; il fondo di selezione non deve coprire la barra. Due nuovi casi Desktop a **1040×740 e 1240×850** confrontano la posizione del margine delle righe con la barra, il clic, il colore e l'avanzamento: **211 casi xUnit attesi**. **Non compilati né eseguiti qui**, in attesa di gate Windows. Nessuna modifica al motore, al catalogo o ai salvataggi.

**Decisione M5.2 ricevuta il 28/10:** l'utente ha confermato **l'intero contratto A** del [dossier Forgia/Zecca](design/M5_2_FORGE_MINT_PREFEASIBILITY.md): Forgia costo **240 Tavole/180 Pietra**, 1 addetto, **1 Ferro + 1 Carbone → 2 Spade/s**; Zecca **210 Tavole/160 Pietra**, 1 addetto, **1 Oro + 1 Carbone → 3 Monete/s**. Le Spade saranno consumate dal futuro arruolamento (quantità non ancora definita), le Monete saranno scorta per **pagamenti espliciti**, non valuta generale (costo nave rinviato). Priorità fisse Ferro 90, Oro 91, **Forgia 100, Zecca 101**, capacità standard di deposito, **lotti indivisibili**. È un'**approvazione progettuale**, non implementazione; M5.2a/b saranno i prossimi incrementi dopo i gate. Non richiedere nuovamente queste scelte.

| Suite | Test superati |
| --- | ---: |
| Core | 67 |
| Application | 61 |
| Infrastructure | 30 |
| Desktop | 48 |
| **Totale** | **206** |

Run precedente **riportata**: `899f6ff3e8ff41288330c3d26b4c4ea7`, sotto `.local-cache/m4/runs/`. Indicati **19 PNG**: 17 render richiesti dal manifest più due render del dettaglio. [Report dell'interfaccia](validation/DESKTOP_UI.md). Il nuovo benchmark non costituisce una nuova esecuzione del gate.

## Decisioni già ricevute

- **Prima campagna:** i [22 tipi della scheda dedicata](design/FIRST_CAMPAIGN_PROPOSAL.md); gli altri 21 riferimenti sono fuori.
- **Vittoria:** possedere tutti i 22 tipi, con il Castello come costruzione conclusiva; gli edifici iniziali contano.
- **Conquista:** una missione obbligatoria sblocca il Castello. Isola, costi, forze, durata ed esiti restano da scegliere.
- **Demolizione:** rinviata. Nessun rimborso approvato; Carne fuori dalla prima campagna.
- **Salvataggi S1:** continuare le partite civili con conversione e backup, senza uno slot campagna distinto.
- **Ferro e Oro:** configurazioni A approvate con risposte separate. Il Ferro mantiene la priorità sul Carbone scarso.
- **Forgia e Zecca:** l'intero contratto A approvato il 28/10, non ancora implementato; quantità future per reclute/navi non scelte.
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

La tabella Risorse applica ordinamento alfabetico ai **nomi italiani mostrati** (senza distinzione maiuscolo/minuscolo, parità risolta dall'ID). Le card delle tre risorse in evidenza non vengono riordinate. L'ordinamento del catalogo, il motore e i salvataggi restano invariati.

La v35 aggiunge tempo stimato di riempimento al **saldo netto del prossimo secondo** (`hh:mm:ss`), `—` se non cresce; la stima non considera variazioni future degli addetti e dei materiali. Le cinque colonne numeriche precedenti e quella nuova sono allineate a destra; la quantità nella sola tabella è intera, nelle card resta precisa. Costi, produzioni e consumi della struttura usano lo stesso template icona + testo normale da 12 px. La **v36** separa le righe dalle scrollbar e rende selezionabili in ocra anche le risorse; l'ordinamento alfabetico resta stabile.

Hover dei pulsanti abilitati nello stesso ocra della struttura selezionata (`#E6D5AC`), testo scuro; disabilitati grigi e geometria stabile. Font, pittogrammi e regole visive sono descritti nel [contratto grafico](PIXEL_ART_UI.md).

Il [README](../README.md) include ora uno screenshot versionato in [docs/screenshots](screenshots/README.md), distinto dalle immagini del gioco di riferimento.

## Prossimo lavoro

1. **Validare la v36 in Windows** (211 casi xUnit attesi) e controllare visivamente scrollbar, colore della selezione, colonne Risorse e consumi Edifici a 1040×740 / 1240×850. Conservare l'esito Windows dichiarato per la v33; se disponibile, acquisire il riepilogo del comando completo per distinguere il gate M5.1c dai soli test xUnit. L'esperimento M5.2 è facoltativo e separato:

   ```powershell
   dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-industry-scale
   .\validate.cmd
   ```

2. Dopo il gate, **implementare M5.2a Forgia/Spade**, poi **M5.2b Zecca/Monete** secondo il [contratto già approvato](design/M5_2_FORGE_MINT_PREFEASIBILITY.md), preservando versione di regole S1, migrazione, backup, scorte e precisione. La [prova isolata](../experiments/forge-mint-probe/README.md) può informare i controlli, ma non sostituisce i gate. Per M5.3/M5.4 rimangono da scegliere i quantitativi di impiego delle Spade e delle Monete.
3. Poi popolazione militare, Caserma, Cantiere, flotta, missione obbligatoria e vittoria, secondo la [roadmap](ROADMAP.md).
4. DPI fisici, altre piattaforme e accessibilità restano verifiche di rifinitura; non considerarli già coperti dai render headless.

Per ripartire: leggere questa pagina, preservare le modifiche Git presenti e usare i comandi della [guida di sviluppo](DEVELOPMENT.md).

Riordino documentale della baseline precedente: **62 file Markdown e 468 collegamenti locali**, ancore comprese, senza errori secondo il rapporto locale dichiarato `.local-cache/docs-audit.json`. Il nuovo file M5.1c richiede di aggiornare il controllo dei collegamenti; screenshot del README preservato, 1240×850.
