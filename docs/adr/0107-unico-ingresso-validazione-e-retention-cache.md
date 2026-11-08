# ADR 0107 — Unico comando di validazione e retention sicura delle evidenze

**Data:** 2026-11-07  
**Stato:** approvato, verifica Windows della RC96 ancora necessaria

## Contesto

`validate.cmd` eseguiva il controllo storico, `validate-f1.cmd` richiamava `validate.cmd` più il gate completo. Due nomi inducevano a lanciare la verifica incompleta; `.local-cache/m4/runs` e `f1-gates` accumulavano decine di cartelle. README conteneva sezioni ripetute, un `MANIFEST_RCxx_SHA256.txt` veniva allegato a ogni ZIP.

## Decisione

1. **Un solo `validate.cmd`**: lancia `scripts/validate-f1-gate.ps1`, che richiama `scripts/validate.ps1` direttamente in un processo PowerShell separato, senza richiamare il batch. Nessun bypass di smoke, screenshot, benchmark o F1.
2. Il gate cancella `validation.json` precedente prima del run e verifica timestamp, counters TRX e screenshot dell'esecuzione corrente. Conserva evidenze per i fallimenti.
3. Lo script `prune-validation-cache.ps1` conserva solo il run e il gate più recenti per GUID e pulisce `m4/publish`. Non elimina pacchetti NuGet, salvataggi personali, junction/symlink, cartelle non-GUID e file fuori dalle destinazioni note. Pulizia automatica nel `finally` del gate; `clean.cmd -CacheOnly` consente una pulizia manuale.
4. README presentazione sintetica e screenshot autentico con ritagli dichiarati; prove della GUI v15 generate dal test su Windows.
5. Nessun file manifest per release nel progetto o nello ZIP. Integrità dell'archivio verificata prima della consegna.

## Conseguenze

Un solo comando documentato. Necessaria una nuova verifica su Windows della catena PowerShell e della retention, pur lasciando invariati i 1245 test della baseline RC95. Gli audit indipendenti restano script interni e le evidenze dell'ultima esecuzione vengono conservate per diagnosi.
