# M4.12 — Lettura comparativa dei benchmark già prodotti

Data: 2026-10-27. **Stato: eseguita e verificata su Windows**, dopo il gate completo da **177/177 test**. Il testo salvato è stato confrontato con una seconda lettura delle copie isolate dei JSON, ottenendo lo stesso rapporto. [Risultati misurati, provenienza e limiti](M4_MEASURED_RESULTS.md). Nessuna modifica ai benchmark, alle simulazioni o al gameplay; il giudizio dell'utente sul prototipo resta aperto.

## Il problema da risolvere

`validate.cmd` produce quattro report JSON contenenti i dati quantitativi della crescita e del Pane, ma la sola notizia «test passati» non indica *quanto* ciascuna strategia abbia guadagnato, quali scorte si siano esaurite o quanto siano frequenti le carenze. Le formule teoriche del progetto sono punti di confronto, non sostituti dei numeri misurati.

Il nuovo `scripts/report-m4.ps1` **legge i risultati già esistenti** e restituisce un resoconto Markdown. Non avvia .NET, non cambia i risultati e non tocca alcun salvataggio personale.

## Come usarlo su Windows

Dalla radice del progetto, dopo aver eseguito `validate.cmd` con esito positivo:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\report-m4.ps1
```

Per **salvare esplicitamente** il documento, senza sovrascrivere file esistenti:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\report-m4.ps1 -OutputPath .local-cache\m4\m4-report.md
```

È previsto `-Force` soltanto per chi voglia **sovrascrivere consapevolmente** un precedente `m4-report.md`. Con `-ArtifactsDirectory` puoi indicare un'altra cartella di report. I percorsi predefiniti sono relativi alla posizione dello script e del progetto, non alla cartella personale delle partite.

Se il JSON manca, è malformato, contiene una versione incompatibile, strategie duplicate o secondi non riconciliati, il comando termina con un errore esplicito. Un report parziale non viene presentato come confronto completo.

## Quali dati legge

| Input JSON nella cartella `.local-cache/m4` | Regole | Cosa misura |
| --- | --- | --- |
| `housing-benchmark.json` | `civil-growth-4` | Tre strategie, tempi per 48/100/136 abitanti, otto ore |
| `housing-accelerated-benchmark.json` | `civil-growth-5` | Stesse strategie e soglie con il bonus delle case |
| `bread-scale-benchmark.json` | `civil-growth-4` | Quattro strategie, 220→240/250 abitanti, 30 minuti |
| `bread-high-density-benchmark.json` | `civil-growth-4` | Quattro strategie, 320→340/350 abitanti, 30 minuti |
| `validation.json` | Gate locale (facoltativo) | Conteggi xUnit e stato smoke dichiarati dal validatore |

Il resoconto include:

1. **Accelerazione delle case**: confronti affiancati prima/dopo per ciascuna soglia, secondi risparmiati e abitanti finali.
2. **Filiera Pane a 220 e 320**: abitanti e tempi, secondi senza Pane, produzione e consumo effettivi, scorta finale, personale e investimento aggiuntivo.
3. **Lettura della soglia critica**: valori effettivi della filiera snella rispetto a due filiere complete a 320 abitanti.
4. **Tracciabilità limitata**: versione regole per ogni profilo, durata e secondi riconciliati; stato della validazione se `validation.json` è disponibile.

## Confini importanti

- Gli input si trovano nella cartella comune `.local-cache/m4`; eseguire una validazione incompleta e poi altre prove può lasciare JSON di **sessioni diverse**. Questa utility controlla la coerenza di forma e di versione, **non può provare che tutti i file appartengano a un unico run**. Per una certificazione conservare anche TRX, PNG e `runs/<id>`.
- Il rapporto misura **strategie predeterminate**, non cerca la migliore strategia possibile.
- I benchmark Pane partono da colonie **sintetiche** già popolate; i materiali riportati sono *aggiuntivi*, senza costo di costruzione iniziale.
- In mancanza di un raggiungimento di soglia viene indicato **«non raggiunto»**; non si inventa un tempo pari alla durata del test.
- La conferma dell'utente che v24 ha passato i test rimane valida come **gate riferito superato**, ma senza i JSON non si conoscono i valori quantitativi che il report stamperà.
- Questa utility **non sostituisce la validazione v25** e non approva Carne, demolizione, filiere M5 o prezzi industriali.

**Uscita verificata:** quattro JSON letti e risultati consolidati nel [rapporto misurato M4](M4_MEASURED_RESULTS.md). Per l'esecuzione analizzata sono conservate copie dei cinque input, incluse le informazioni di validazione, in `runs/98d961210607483a8afe8d708039efab/benchmark-inputs/`, con manifest SHA-256 accanto alla cartella. I controlli di forma del lettore non cambiano: per nuovi run resta necessario verificarne separatamente la provenienza.
