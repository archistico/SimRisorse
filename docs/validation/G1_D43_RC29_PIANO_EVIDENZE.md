> **Aggiornamento RC30 / G1-D44:** la procedura continua a valere ma il validatore ora controlla l'effettiva leggibilità dei PNG, il formato e dimensioni compatibili con la viewport. `NA` non fa risultare completo l'audit: richiede approvazione esplicita. Dopo `-Action Report` si può eseguire `-Action Verify` (**0** = 44 PASS formalmente completi, **2** = incomplete/FAIL/NA; nessuno dei due chiude da solo P0). Per provare il validatore senza screenshot reali usare `powershell.exe -NoProfile -File .\scripts\test-p0-ui-evidence.ps1`: crea solo immagini temporanee fittizie e non tocca il registro reale. Vedere [piano RC30](G1_D44_RC30_TEST_PLAN.md).

# G1-D43 — Istruzioni per la raccolta delle evidenze P0

**Non occorre eseguire nulla adesso.** Questa procedura serve quando Windows e una copia dei salvataggi saranno disponibili. RC27 ha 507 test confermati; RC28/RC29 hanno **511 test attesi ma non ancora eseguiti**. Non sostituire screenshot reali con simulazioni o layout ricostruiti.

## 1. Prima di iniziare

1. Estrarre RC29 in una nuova cartella e lavorare su **partite dimostrative o copie**: non usare DB personali per test di Nuova partita/Salva/Carica.
2. Eseguire `./validate.cmd` una sola volta sulla RC29 cumulativa, conservando il TRX. Se fallisce, registrare il difetto senza dichiarare P0 chiuso.
3. A pari DPI/scaling aprire `./run-game.cmd` (v13) e `./run-game.cmd --experimental-v14` **separatamente**, impostando 1040×740 e 1240×850. Usare le stesse condizioni di gioco per le parti confrontabili.
4. In Windows PowerShell, dalla cartella del progetto, inizializzare il registro:

```powershell
.\scripts\p0-ui-evidence.ps1 -Action Init
```

Crea `.local-cache/p0-ui-evidence/results.csv` con **44 scenari** inizialmente `NON_ESEGUITA`, e `screenshots/`. Se il registro esiste già **non viene sovrascritto**. La directory resta fuori dallo ZIP ed è intenzionalmente locale. Una [copia CSV vuota](G1_D43_UI_REGISTER_TEMPLATE.csv) è inclusa nel progetto come modello consultabile anche senza PowerShell.

## 2. Immagini

Per ogni scenario usare il nome esatto `UI-XX_1040x740_v13.png`, `UI-XX_1040x740_v14.png`, `UI-XX_1240x850_v13.png` e `UI-XX_1240x850_v14.png`. Per **UI-16 Scuola** e **UI-17 Albero tecnologico** serve solo v14: v13 non ha una pagina equivalente. Le immagini v13/v14 vanno acquisite nello stesso viewport e con DPI registrato nelle note; eventuale barra titolo compresa nello screenshot non altera il nome dello scenario.

Si possono copiare PNG manualmente in `screenshots/`. In alternativa, su Windows con la finestra di gioco attiva usare `Alt+Stamp` (copia finestra negli Appunti), quindi:

```powershell
powershell.exe -STA -File .\scripts\p0-ui-evidence.ps1 -Action Capture -UiId UI-02 -Viewport 1040x740 -Version v14
```

La cattura **non** assegna PASS, non avvia il gioco e non sovrascrive screenshot già presenti. Non inserire nelle immagini dati reali non necessari. Per i difetti hover/focus, un singolo PNG può non bastare: descrivere le azioni e conservare più prove con nomi aggiuntivi, se servono.

## 3. Registrazione e rapporto

Modificare `results.csv` con Excel o un editor (CSV UTF-8), senza cambiare le 44 chiavi `UI_ID,Viewport`. Campi: `Stato` (`NON_ESEGUITA`, `PASS`, `FAIL`, `NA`), `Passi` effettivamente riprodotti, `Testatore`, `Data`, `Note` (anche DPI, differenze autorizzate o motivazione `NA`), `Issue` per FAIL e `RiferimentoTRX`.

- `PASS`: riportare passi, testatore e data; screenshot v13+v14 presenti, tranne UI-16/UI-17 dove basta v14.
- `FAIL`: descrivere l'anomalia in `Issue`, condizioni e riproduzione; allegare screenshot quando possibile.
- `NA`: usare **solo con motivazione specifica** nelle Note e passi riprodotti; non usare NA perché non si è avuto tempo di testare.
- `NON_ESEGUITA`: mantenere quando la prova manca.

Per generare il confronto affiancato, senza rete:

```powershell
.\scripts\p0-ui-evidence.ps1 -Action Report
```

Aprire `.local-cache/p0-ui-evidence/report.html` nel browser. La pagina segnala `EVIDENZA_MANCANTE` se una riga segnata PASS non è completa; include il confronto screenshot dove disponibili. **Nemmeno 44 PASS equivalgono a chiusura automatica di P0:** serve anche il gate della suite .NET, la verifica dei salvataggi e l'approvazione visiva dell'utente.

## 4. Percorso operativo

Eseguire in ordine i gruppi del [runbook D42](G1_D42_P0_RUNBOOK.md): (A) UI-01/18/19, (B) UI-02…06, (C) UI-07…13, (D) UI-14/15/22, (E) UI-16/17, (F) UI-20/21. Utilizzare la [matrice UI-01…UI-22](G1_D39_P0_UI_MATRIX.md) per i criteri per ogni ID. Dopo tutte le prove, D44 potrà correggere esclusivamente i FAIL riproducibili; D45 sarà il gate conclusivo.

**Attenzione:** lo script PowerShell non è stato eseguito su Windows in questo ambiente; appartiene agli strumenti di verifica da collaudare quando possibile, non alla suite xUnit di 511 test.
