# Validazione v60-E3b3-RC2

## Provenienza

Base: ZIP integrale v60-E3b3-RC1, tutti i test confermati dall'utente; codice nuovo in RC2 non convalidato .NET.

## Test da eseguire su Windows

```powershell
.\validate.cmd
```

**Attesi:** 396 test xUnit se nessuna suite è stata rimossa; successivamente tutti i gate di simulazione previsti dal validatore.

Verifiche specifiche:

1. Parità simulazione 60 × 1s contro 60s e 120 × 1s contro 120s.
2. Passaggio formativo al secondo 120, avvio nuovo studente dalla coda e ricercatori qualificati non automaticamente attivi.
3. Conservazione dei tick frazionari (500ms + 500ms), pausa e ripresa.
4. Parità v13/v14 sul catalogo civile senza elementi v14; produzione con i lavoratori effettivi per tier.
5. Overflow del tempo senza commit e salvataggio SQLite v3 durante l'avanzamento della formazione.

## Controlli da non confondere

I controlli statici/di archivio non sono compilazione .NET o xUnit. La convalida su database personali non è autorizzata: usare solo copie. L'attivazione v14 in UI resta rinviata.
