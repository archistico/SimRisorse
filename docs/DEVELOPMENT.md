# Sviluppare e verificare SimRisorse

## Prerequisiti

Windows, **SDK .NET 10**, PowerShell 5.1 o superiore; ripristino NuGet online oppure pacchetti già presenti in `.local-cache/nuget`. Soluzione `SimRisorse.slnx`, test xUnit, GUI Avalonia. 

## Comandi ufficiali

```powershell
.\run-game.cmd                     # Avvia la GUI v15
.\validate.cmd                     # Unico gate obbligatorio: restore/build/xUnit/render/native smoke/audit F1
.\validate.cmd -StressSeconds 300  # Gate completo con audit di 300 secondi
.\validate.cmd -Combined24Hours    # Aggiunge 24 ore di audit (più costoso)
.\validate.cmd -OfflinePackageSource .local-cache/nuget  # Restore locale, se i pacchetti sono già disponibili
.\clean.cmd -DryRun                # Mostra bin/obj che verrebbero rimossi
.\clean.cmd                        # Pulisce bin e obj, non NuGet
.\clean.cmd -CacheOnly -DryRun     # Anteprima pulizia vecchi risultati
.\clean.cmd -CacheOnly             # Mantiene solo ultimo run e ultimo gate F1
```

Non esiste più `validate-f1.cmd`. Gli script PowerShell `scripts/validate-f1-gate.ps1` e `scripts/validate.ps1` sono componenti **interni**: non sono due diversi livelli di validazione pubblica. Il gate non chiama `validate.cmd` per evitare ricorsioni.

## Evidenze e politica di cache

- **`.local-cache/nuget/`**: cache di pacchetti, preservata anche dalla pulizia.
- **`.local-cache/m4/validation.json`**: resoconto del run xUnit/render/native più recente riuscito. Eliminato quando inizia una nuova validazione, per evitare report obsoleti.
- **`.local-cache/m4/runs/<GUID>/`**: TRX, screenshot, log dell'ultima prova, anche se fallita.
- **`.local-cache/m4/f1-gates/<GUID>/`**: rapporto gate F1 e audit dell'ultima prova, anche se fallita.
- **`.local-cache/m4/publish/`**: directory temporanea usata dal native smoke; rimossa a fine gate.

Le vecchie cartelle GUID vengono eliminate automaticamente a fine esecuzione. La pulizia opera **soltanto** in `m4/runs` e `m4/f1-gates`, non elimina file con nomi non riconosciuti o directory collegate, salvataggi, `.git`, sorgenti o dati fuori dal repository. In caso di permessi negati il comando segnala il percorso non eliminato.

I report controllati storici restano possibili con `scripts/report-m4.ps1` sui JSON dell'ultima esecuzione; non confonderli con risultati di progressione naturale. Gli screenshot della documentazione sono reali e storici; le nuove catture v15 si ottengono da `runs/<GUID>/screenshots`.

## Regole di manutenzione

Conservare il layout pixel art approvato, gli edifici non ancora sbloccati consultabili, il tooltip dei flussi e i sette campi della tabella Risorse. Non alterare la baseline dei test xUnit né sostituire il gate con `dotnet test` isolato. Mantenere un solo documento autorevole per stato, roadmap, handoff e decisioni, con ADR per le scelte tecniche. Consegne ZIP complete **senza** `MANIFEST_XXX_SHA256.txt`.

**Estrazione su una cartella esistente:** un archivio ZIP normalmente non elimina i file rimasti dalla versione precedente. Se compare ancora `validate-f1.cmd` o un vecchio `MANIFEST_RCxx_SHA256.txt`, sono residui locali: eliminarli manualmente o estrarre in una cartella pulita. Il gate avvisa se trova il vecchio batch ma non interrompe il test per questo motivo.
