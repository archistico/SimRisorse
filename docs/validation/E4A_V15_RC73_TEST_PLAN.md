# RC73 — Piano di validazione benchmark naturali v15

**Stato:** preparato, non eseguito. **Suite attesa:** 834 test (826 RC72 + 8 casi xUnit RC73). Ultima suite confermata: RC63 (771/771). I risultati dei benchmark **non** sono inclusi.

1. Eseguire `./validate-f1.cmd`: l'argomento atteso predefinito è stato portato a **834** in RC73, senza cambiare la natura del gate F1. Verificare TRX, smoke GUI, audit v15 e report; se il gate fallisce, correggere prima di considerare valido un benchmark.
2. Verificare che gli 8 nuovi casi `E4aV15NaturalProgressionBenchmarkTests` passino e che le fixture iniziali coincidano con una partita v15 nuova.
3. Eseguire `--benchmark-e4a-v15-natural --scenario V15-N01 --max-seconds 120` e verificare `Diagnostic=true`, tempo nullo per outcome diversi da pass e nessun inventario aggiunto artificialmente.
4. Eseguire gli orizzonti completi di V15-N01/N02/N03 in directory nuove. Confrontare due esecuzioni identiche, includendo JSON e CSV; mantenere esplicitamente `blocked`/`timeout` senza inventare durate.
5. Controllare esportazione e non sovrascrittura: riutilizzando la stessa directory il comando deve rifiutarsi di sovrascrivere i risultati.
6. Confermare che `--benchmark-e4a-natural` rimanga quello **v14** e `--audit-v15` resti un test di integrità, non un benchmark naturale.
7. Verifica manuale della GUI: edifici grigi consultabili, dettagli con icone e capacità, pulsanti e posizioni approvate, barra nuovo abitante, tabella Risorse 7 colonne, Scuola ed Esercito; nessun file Desktop è stato modificato in RC73.

**Criterio RC73:** compilazione .NET 10 + suite 834/834 + report runner consistente + conferma che nessun risultato è etichettato come verificato prima dell'esecuzione. **Criterio F1:** richiede in aggiunta tutti i gate della RC72 e verifica visiva GUI.
