# RC96 — Piano di verifica del comando unico e della pulizia cache

**Baseline:** RC95 / 1245 test confermati. **RC96:** stessi test, nessuna modifica di gameplay.

1. In una copia di lavoro Windows eseguire `.\validate.cmd` (non è incluso `validate-f1.cmd` nel nuovo ZIP). Verificare build, 1245 test, render e gate F1 green. Lo script non deve richiamare ricorsivamente il batch.
2. Verificare screenshot generati dalla stessa esecuzione: `v15-buildings-all|food|industry` alle larghezze 1040/1240, più risorse, Scuola e Militari. Nessun nome obsoleto e nessuna prova del run precedente.
3. Eseguire `.\clean.cmd -CacheOnly -DryRun` e controllare i percorsi prima di rimuovere cartelle. `.\clean.cmd -CacheOnly` deve conservare un solo GUID in `m4/runs` e uno in `m4/f1-gates`; la cache NuGet e i salvataggi non cambiano.
4. Creare due esecuzioni consecutive, anche con una che fallisce: il gate deve conservare solo l'ultima prova, con `f1-gate.json`, TRX e screenshot disponibili; `validation.json` non deve restare dichiarato verde dopo una prova non riuscita.
5. Verificare restore offline con `.\validate.cmd -OfflinePackageSource .local-cache/nuget` solo se il repository contiene i pacchetti necessari. `-Combined24Hours` rimane esplicitamente opt-in.
6. Verificare il README e tutte le immagini dichiarate; nessun `MANIFEST_*_SHA256.txt` nel pacchetto. Il vecchio screenshot è identificato come riferimento storico.

**Criterio di accettazione:** gate F1 completato, 1245 test superati, vecchi run eliminati senza toccare NuGet/salvataggi, documenti coerenti. Non sostituire validazione completa con il solo `dotnet test`.

**Nota su estrazioni sovrapposte:** un vecchio batch rimasto fisicamente sul disco produce un avviso, non un errore; va eliminato a mano per avere realmente un solo file `.cmd` di validazione.
