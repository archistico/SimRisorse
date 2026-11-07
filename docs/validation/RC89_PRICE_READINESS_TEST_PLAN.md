# RC89 — Piano di validazione economica preliminare

**Baseline confermata:** RC88.1, 1105 test Windows superati. **Candidata:** RC89, 18 test nuovi, **1123 cumulativi attesi**.

## Verifica automatica

1. Eseguire `./validate-f1.cmd` su Windows con SDK .NET 10. Il gate esegue tutti i test, smoke Desktop e audit precedenti, inclusa la RC89.
2. Eseguire il comando senza prove naturali: `dotnet run --project tools/SimRisorse.Simulation -c Release -- --study-e4b-price-readiness --output .local-cache/rc89-proposte`. Deve dare 9 righe e 0 esecuzioni importate, senza prezzi attivi.
3. Usare **una cartella nuova** per rielaborare le ricevute del gate RC88: `--study-e4b-price-readiness --input percorso/rc88-industry-chain-results.json --output cartella-nuova`. Il report a zero secondi deve riportare 7 diagnostiche, 0 completamenti, 0 MintTen completati, SHA-256 dell'input.
4. Usare file malformati o con `Outcome = pass` forzato: il controllo deve rifiutarli, non attribuire un successo alle curve.
5. Verificare che in ogni riga quote prima+seconda=lotto atomico e lavoratori = 10/20 Base, 30/60 Grande, 100/200 Avanzato; capienza iniziale e percorso Magazzini devono rimanere distinti.
6. Esaminare CSV, Markdown e JSON. In caso di assenza di una risorsa dallo stock/capacità si assume zero e si elenca il deficit; non è permessa `KeyNotFoundException`.

## Limiti della prova

Il gate usa solo run RC88 diagnostici a zero secondi per tenere breve la validazione. Eseguire i benchmark naturali completi **separatamente**, conservandone tutti i report; un timeout non è prova di blocco strutturale. I report RC89 non implementano acquisti rateali né cambiano i costi o la GUI.
