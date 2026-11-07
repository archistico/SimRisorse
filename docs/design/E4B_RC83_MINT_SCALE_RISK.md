# RC83 / E4b.2e — Matrice di rischio per 1 → 2 Zecche

## Scopo e perimetro

Proiezione *analitica* che unisce tre pezzi già esistenti: prezzi/filiera (RC78), capienza/rate (RC79), moduli e squadre complete (RC80/81). **Nessuna curva A/B/C è stata scelta o installata.** Nessun benchmark a tempo è stato eseguito dall'analisi. La RC82 è confermata dall'utente con 1002 test superati.

**Nove righe:** A/B/C × Base/Grande/Avanzato. Ogni riga conserva i preventivi dell'investimento con una e due Zecche, quota della seconda copia, costo della seconda copia acquistata in lotto con la prima e relativo fabbisogno aggiuntivo di tutta la filiera (incluse case, Scuola e produttori degli input; i punti ricerca sono separati). La lista di edifici aggiuntivi è esplicita, senza conteggiare due volte le strutture già presenti.

| Tier | Moduli per edificio | Lavoratori 1 Zecca | Lavoratori 2 Zecche |
| --- | ---: | ---: | ---: |
| Base | 1 | 10 | 20 |
| Grande | 3 | 30 | 60 |
| Avanzato | 10 | 100 | 200 |

Il report distingue lavoratori supplementari della Zecca, residenti supplementari richiesti dal budget statico, addetti supplementari a supporto (filiera/alimenti/ricerca) e case grandi supplementari. Lavoratori non fungibili: seconda Zecca significa ulteriori squadre, non riassegnazione della prima.

## Vincoli economici

- Il **prezzo della seconda Zecca** è `Quote(curva,"mint",tier,previouslyAcquired:1)`. Il lotto atomico di due Zecche è `Quote(..., previouslyAcquired:0, quantity:2)`, riconciliato con la somma delle due quote singole.
- Il **costo marginale della filiera** è `Investment(curva,2,tier).ConstructionWithoutWarehouses - Investment(curva,1,tier).ConstructionWithoutWarehouses`. Non rappresenta costo di un'azione singola: confronta due distinte configurazioni-obiettivo di produttori, alloggi, ricerca e alimenti.
- Due percorsi di capienza si confrontano separatamente: seconda Zecca singola e acquisto atomico di due. La costruzione progressiva dei Magazzini viene arrestata quando il costo del prossimo Magazzino supera la capacità corrente. Questo NON prova un softlock generale; potrebbero esistere altri percorsi nel gameplay.
- I limiti inferiori di rate via `ProgressiveConstructionEscrow.MinimumInstallments` assumono deposito finanziato in seguito, non modellano disponibilità risorse, rendimenti o tempi. Escrow NON attivo in partita.
- Rapporto "equivalenti Tagliapietre" = lavoro materiale ricorsivo del solo prezzo diretto della Zecca / lavoro materiale ricorsivo del prezzo del primo Tagliapietre. Fasce sotto 100 / 100–1000 / oltre 1000. Non affermare che queste equivalenze rappresentino i costi totali dell'espansione o la durata in secondi.

## Esecuzione su Windows con SDK .NET 10

```powershell
.\validate-f1.cmd
# Report su cartella NUOVA; rifiuta la sovrascrittura dei tre file
 dotnet run --project tools/SimRisorse.Simulation -c Release -- --study-e4b-mint-scale --output .local-cache/e4b-v15/rc83-scale-001
# Richiede un orizzonte intero: misura reale e indipendente dalla matrice RC83
 dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4b-mint-ten --compare-legacy --output .local-cache/e4b-v15/rc83-mint-full-001
```

Output nuova CLI: `e4b-mint-scale.json`, `e4b-mint-scale.csv`, `e4b-mint-scale.md`. Sintesi JSON su stdout. L'output include `Mode=analytical-proposal/not-applied`, `StorageScope`, `WorkforceScope` e nove righe, senza etichette di raggiungibilità o milestone non osservate.

## Non regressione

Nessuna modifica a catalogo prezzi v15, Core, GUI/XAML, salvataggi o campagna. Preservare sidebar, disposizione storica, icone e sette colonne della tabella Risorse, pane, stoccaggio diffuso e crescita popolazione ×20.
