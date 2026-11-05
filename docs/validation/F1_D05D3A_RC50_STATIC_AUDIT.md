# RC50 — Audit statico della consegna

**Data:** 5 novembre 2026. Questo audit non equivale a eseguire `.\validate.cmd`.

## Evidenze locali

- Base verificata: ZIP RC49 integrale con **619 file**.
- Diff previsto: **nessuna modifica a sorgenti C# preesistenti, XML/XAML, progetto, file `*.sqlite3`, file JSON o grafica**. Sono stati aggiunti 2 nuovi file Core, 17 test `[Fact]` Application, 2 test `[Fact]` Infrastructure, specifica, piano test, ADR e audit statico; aggiornati README, changelog, stato, roadmap, handoff, punto d'ingresso e indice ADR.
- **26 file XML/XAML/props/progetti** elaborati dal parser XML standard, nessun errore.
- **39 file JSON** elaborati dal parser JSON standard, nessun errore.
- **1.671 collegamenti Markdown locali** controllati nel pacchetto RC50: nessun percorso mancante. I collegamenti testati non includono URL esterni né ancore di sezione.
- **19 nuovi casi `[Fact]`** individuati nel codice. Compilazione, esecuzione suite, carico e benchmark .NET 10 **non possibili** qui per assenza dell'SDK.
- Manifest SHA-256 previsto alla radice della consegna, generato al confezionamento. Integrità ZIP da verificare dopo la scrittura.

## Rischi residui e condizioni di uscita

1. **Compilazione C# non verificata**: la RC50 rimane candidata. Gli errori eventuali vanno riprodotti con `.\validate.cmd` e risolti senza eliminare test.
2. `V15FoodIndustryCampaignSession.StepFood` è **un tick alimentare manuale** e non il clock unico di gioco. Non lo si deve sommare direttamente a `V14CampaignClock`, perché il vecchio `food-store` continuerebbe a creare Cibo dalla città.
3. Costo, capacità, prerequisiti sono valori provvisori. La Zecca e l'economia E4b non sono state toccate.
4. Non caricare il database v14 personale nello store SQLite sperimentale, né pretendere supporto per `user_version=4`.

[Verifica Windows](F1_D05D3A_RC50_TEST_PLAN.md) · [Architettura](../design/F1_D05D3A_RC50_COSTRUZIONI_FILIERA.md) · [Roadmap](../ROADMAP.md).
