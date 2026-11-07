# RC79 — Revisione statica dell'archivio candidato (06/11/2026)

La ricostruzione parte dallo ZIP RC78 fornito dall'utente. Confronto SHA-256 fra gli **797 file originali** e i corrispondenti file dopo l'incremento: **10 file preesistenti modificati, 8 file nuovi, 0 rimossi** prima dell'aggiunta di questo rapporto. Non sono stati modificati file `SimRisorse.Desktop`, `SimRisorse.Infrastructure`, `SimRisorse.Core/Content` o SQLite. I nuovi oggetti vengono compilati dai progetti SDK-style tramite inclusione implicita dei file `.cs`.

Una verifica aritmetica indipendente in Python ha esercitato **3.000 vettori casuali** di fabbisogno/capacità per controllare la formula di `MinimumInstallments`, il limite di ciascuna rata e la riconciliazione esatta del resto intero. Questa è una verifica dell'algoritmo matematico equivalente, **non** l'esecuzione della nuova implementazione C# né dei test xUnit.

**Non disponibili nell'ambiente:** `dotnet`, compilatore C# e PowerShell. Di conseguenza non sono stati eseguiti `validate.cmd`, `validate-f1.cmd`, test Desktop, smoke, nuovi report CLI o benchmark. Nessun numero o file di report RC79 è presentato come misura effettiva. Lo ZIP è una **release candidata**, da validare su Windows.

[Piano di test completo](E4B_RC79_FINANCING_TEST_PLAN.md).
