# Piano validazione — RC31 / G1-D45

**Ultimo gate confermato dall'utente:** RC30, 511 test .NET attesi e dichiarati superati. **RC31:** +4 casi xUnit, 515 attesi, compilazione/test ancora da eseguire.

## Automatico

Dalla cartella del progetto estratto, su Windows con SDK .NET 10:

```powershell
.\validate.cmd
powershell.exe -NoProfile -File .\scripts\test-p0-ui-evidence.ps1
```

Lo script PowerShell è separato e **non** compreso nel conteggio xUnit. Il vecchio smoke `--smoke-test` rimane legacy per compatibilità con `validate.ps1`. I nuovi test coprono scelta della finestra, CLI, percorsi riservati, modalità storiche e shortcut v14.

## Verifica manuale in sicurezza

1. **Fare backup** di `%LOCALAPPDATA%\SimRisorse\experiments\civil-growth-v14.sqlite3` e, se esiste, `%LOCALAPPDATA%\SimRisorse\civil-growth.sqlite3`, oltre agli eventuali file SQLite di supporto (`-wal`, `-shm`). Chiudere il gioco prima di copiare i file.
2. Eseguire `.\run-game.cmd` senza argomenti: deve aprirsi **SimRisorse · Economia v14** con cinque schede (Edifici, Risorse, Esercito, Scuola, Albero tecnologico).
3. Se è presente un salvataggio v14, caricarlo dal comando Carica v14: devono essere presenti gli stessi edifici, le Matite, i ricercatori e le tecnologie precedenti; non va creata una nuova partita al suo posto.
4. Chiudere senza nuovo salvataggio manuale e verificare che il DB v13 non sia cambiato. Preferire copie di prova per eventuali salvataggi intenzionali.
5. Eseguire `.\run-game.cmd --legacy-v13` per verificare la modalità di recupero e il percorso v13 separato.
6. Eseguire `.\run-game.cmd --experimental-v14`: deve ancora funzionare e aprire lo **stesso** slot v14 del nuovo avvio normale.
7. Verificare che passare un percorso riservato alla modalità errata produca un errore senza aprire/sovrascrivere il file.

## Criteri di accettazione

- Suite automatica RC31 verde, nessuna differenza di dati v13/v14, apertura v14 predefinita senza argomenti.
- Le 44 prove P0 non acquisite **non** diventano PASS per effetto della promozione. L'accettazione visuale è dichiarata direttamente dall'utente, non attribuita a screenshot inesistenti.
