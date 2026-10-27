# M4 — Scala della produzione di Pane oltre 200 residenti avanzati

Data: 2026-10-27. **Stato: gate v17 con quattro strategie, inclusa M4.7 “filiera snella”, dichiarato superato dall'utente.** Non sono allegati i risultati grezzi TRX/JSON e i numeri osservati restano da acquisire per un'analisi autonoma. Non modifica `civil-growth-4`, ricette, costi, capacità, GUI o salvataggi. Il gate comprendeva **163 casi xUnit attesi**. Il nuovo stress M4.8 oltre 308 abitanti ha un [report separato](BREAD_HIGH_DENSITY_BENCHMARK.md), non ancora validato.

## Domanda

Quando la colonia arriva a molte Case grandi, il Pane deve diventare un vero vincolo economico. Un Forno produce al massimo **3 Pane/s** utilizzando una filiera completa (due Fattorie, un Pozzo, un Mulino e un Forno). Ogni residente avanzato consuma **0,015 Pane/s** in aggiunta al Cibo. Il limite nominale continuativo è quindi `3 / 0,015 = 200 residenti avanzati`, purché input e magazzini lo permettano.

Oltre la soglia, la scorta può assorbire temporaneamente la differenza: il gioco non deve eliminare abitanti, ma la crescita può diventare intermittente. Con M4.7 si confronta anche un investimento **meno costoso ma limitato dal Grano**, per verificare se un aumento completo della capacità sia davvero necessario nella fascia di popolazione considerata.

## Scenari controllati

Il test **non parte dalla dotazione iniziale**: usa lo stesso snapshot sintetico di metà partita per confrontare soltanto la capacità della filiera Pane. Si conservano le ricette vere e il codice di produzione/prenotazione del Core.

- **220 abitanti**, di cui 212 avanzati, con una Casa piccola e otto Case grandi: **264 posti totali**.
- Tre Pescatori e tre Depositi alimentari, con sei lavoratori, per evitare di confondere la carenza di Pane con quella di Cibo.
- Una filiera Pane completa, già costruita e impiegata, con cinque lavoratori.
- Riserva iniziale: 100 Pane, 1.000 Cibo, 1.000 tavole e 1.000 pietra; le altre scorte partono da zero. Le strutture iniziali del nucleo economico restano assegnate.
- **30 minuti** di tempo di gioco, passi da un secondo, crescita massima teorica da 220 a 250 abitanti.

| Politica | Quando aggiunge la seconda filiera | Tavole addizionali | Pietra addizionale | Lavoratori addizionali |
| --- | --- | ---: | ---: | ---: |
| `one-chain` | Mai | 0 | 0 | 0 |
| `two-chains` | Prima di riprendere la simulazione | 420 | 280 | 5 |
| `second-chain-late` | Allo scoccare del 15° minuto | 420 | 280 | 5 |
| **`lean-chain`** | **Subito: una Fattoria + Pozzo + Mulino + Forno** | **360** | **250** | **4** |

La filiera supplementare è realmente comprata e dotata di lavoratori tramite i comandi di `GameSession`, non inserita artificialmente nello snapshot. Lo snapshot sintetico iniziale serve solo a isolare il confronto. Non sono inclusi i materiali già investiti nelle strutture iniziali o il tempo necessario a raggiungere 220 abitanti.

**Capacità nominali medie massime, da non confondere con le produzioni misurate:** la filiera iniziale, con due Fattorie, fornisce **3 Pane/s**; il potenziamento `lean-chain` porta le Fattorie a tre e Pozzi/Mulini/Forni a due, facendo del Grano il limite teorico di **4,5 Pane/s**; `two-chains` porta quattro Fattorie e due strutture per ogni altro anello, con limite teorico di **6 Pane/s**. Lo studio dei limiti è in [BREAD_CHAIN_CAPACITY.md](BREAD_CHAIN_CAPACITY.md).

## Dati prodotti e controlli

Per ogni politica si registra:

1. popolazione finale, residenti avanzati, tempi per 240 e 250 abitanti, capacità abitativa;
2. secondi in cui la crescita è fermata da carenza di Pane, Cibo o case piene;
3. secondi-lavoratore effettivi di **Fattorie, Pozzi, Mulini e Forni**, secondi con spazio produttivo insufficiente, Pane prodotto/consumato e scorte finali di Grano, Acqua, Farina, Pane e Cibo;
4. lavoratori complessivamente assegnati e costi effettivi del secondo impianto;
5. **1.800 secondi riconciliati** fra forecast, input/output ricette, razioni, scorte e progressione effettiva, per ciascuna delle **quattro** strategie; una riconciliazione addizionale verifica `Pane finale = 100 + Pane prodotto − Pane consumato`.

L'ordine di risultato atteso per le politiche storiche è `one-chain` inferiore a `second-chain-late`, a sua volta non superiore a `two-chains`. La nuova `lean-chain` deve avere costi/personale inferiori alla seconda filiera completa, e la validazione chiede che **non peggiori rispetto a `one-chain`** e **non superi `two-chains` in popolazione finale**. Si verifica anche che non aumenti i secondi di carenza del Pane rispetto a una sola filiera. Queste sono **asserzioni da eseguire**, non risultati già misurati. Il benchmark dovrà stabilire se la filiera snella raggiunge 250 persone nello stesso tempo della completa e quali anelli lavorano realmente: non anticipiamo l'esito.

### Criteri di valutazione economica

Il risparmio della snella è **60 legno lavorato, 30 pietra e 1 lavoratore**. Un investimento completo potrebbe comunque essere preferibile quando la popolazione o i consumi aumentano oltre il margine sostenuto da 4,5 Pane/s, ma ciò andrà confrontato in un orizzonte più ampio. Un singolo stress da 30 minuti partendo da 220 abitanti **non prova** quale strategia minimizzi il tempo necessario a vincere lo scenario intero.

**Limite conoscitivo importante:** dopo 1.800 secondi la popolazione non può superare 250; con otto posti di Casa piccola ciò corrisponde a **242 residenti avanzati e 3,630 Pane/s** richiesti, quindi *sotto* il limite teorico della snella (4,5 Pane/s). Il test misura pertanto il valore del risparmio **in questa fascia**, ma non dimostra la convenienza dopo la soglia di circa **308 abitanti** (300 avanzati) né quando servono ulteriori Case grandi. È ora predisposto un [secondo profilo a 320 abitanti](BREAD_HIGH_DENSITY_BENCHMARK.md) per questa domanda, ancora da verificare.

## Esecuzione

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-bread-scale
```

Il validatore generale esegue lo stesso comando dopo il confronto abitativo e salva `bread-scale-benchmark.json` in `.local-cache/m4/`; il percorso è riportato anche in `validation.json`. Il risultato non sostituisce il benchmark su otto ore delle [tre politiche abitative](HOUSING_BENCHMARK.md): questo è uno **stress mirato** su una colonia già sviluppata, non una stima del tempo per costruirla.

**Punto di ripresa:** il confronto originario a tre politiche e poi la versione v17 con diagnosi numerica, capacità dell'intera filiera e quarto profilo `lean-chain` sono stati dichiarati superati dall'utente. Il prossimo **gate Windows** riguarda la prova nuova a 320 abitanti; serve leggere i due JSON per una valutazione economica quantitativa e non dedurre numeri specifici dalla sola conferma. Non sono stati introdotti acquisti automatici o nuovi consumi.
