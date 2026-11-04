# Piano di validazione RC30 / G1-D44

**Stato iniziale:** RC27 confermata 507/507; RC28/29/30 511 test .NET attesi e non ancora validati. Prove P0 manuali 0/44. Non occorre eseguire nulla finché Windows non è disponibile.

## 1. Suite cumulativa .NET

Da progetto estratto in **cartella nuova**, usando partite di prova o copie dei salvataggi:

```powershell
.\validate.cmd
```

**Atteso:** 511 test xUnit, compilazione senza errori; conservare TRX. Se fallisce, non dichiarare RC30 valida e allegare il log.

## 2. Auto-test dello strumento di audit (non xUnit)

```powershell
powershell.exe -NoProfile -File .\scripts\test-p0-ui-evidence.ps1
```

**Atteso:** controlli locali superati. Il test crea un registro temporaneo e PNG fittizi e lo elimina al termine, senza leggere o alterare partite e senza inserire risultati nel registro reale. È indipendente da `validate.cmd` e non è contato fra i 511 test. **Non è stato eseguito in questo ambiente**: richiede Windows/PowerShell e `System.Drawing`.

Casi verificati: 44 righe da Init, seconda Init non distruttiva, rapporto e Verify su registro non compilato, 44 NA non equivalenti a PASS, PNG corrotto incapace di completare PASS, 44 PASS con file PNG sintetici leggibili (solo test tecnico dello strumento).

## 3. Registro visivo reale

```powershell
.\scripts\p0-ui-evidence.ps1 -Action Init
.\scripts\p0-ui-evidence.ps1 -Action Report
.\scripts\p0-ui-evidence.ps1 -Action Verify
```

- Prima di compilare le prove `Verify` deve uscire con **2** e il rapporto deve indicare P0 aperto.
- Aggiungere solo screenshot reali da v13/v14 per le finestre 1040×740 e 1240×850; non riutilizzare quelli sintetici dell'auto-test.
- Un PNG mancante, corrotto, di formato errato o troppo piccolo per la viewport deve risultare `EVIDENZA_MANCANTE`.
- Le eccezioni `NA` non chiudono la verifica automaticamente; devono essere esaminate e autorizzate manualmente.
- Dopo 44 PASS documentati e immagini valide, `Verify` può uscire con **0**, ma il gate P0 resta aperto finché non ci sono **suite .NET verde e approvazione visiva dell'utente**.

Il report HTML è `./.local-cache/p0-ui-evidence/report.html` e non viene inserito nel pacchetto. La cattura clipboard con `-Action Capture` richiede Windows PowerShell in modalità STA; seguire [il piano D43](G1_D43_RC29_PIANO_EVIDENZE.md).

## 4. Verifiche statiche della consegna

- Differenze rispetto a RC29 circoscritte a `scripts/` e alla documentazione.
- XAML v13/v14, Core, applicazione, SQLite, test xUnit e script di simulazione invariati.
- ZIP integro, file strutturati validi, riferimenti Markdown interni senza interruzioni.
