# Piano validazione RC63 — GUI v15 senza regressioni v14

**Base:** RC60 756/756 confermata; RC61 760/761 (test di versione), RC62 762 attesi non confermati. **RC63:** +9 nuovi test Desktop; **771 cumulativi attesi**. Nessuna esecuzione .NET 10 in questo ambiente.

## Passaggi automatici

```powershell
.\validate.cmd
```

Confermare **771/771** senza errori C# né XAML. I nuovi test verificano crescita in sidebar, icone/ricette/capienze delle nuove filiere, prezzi/visibilità degli edifici bloccati, livelli indipendenti, batch personale, tabella risorse, Scuola ed Esercito, controlli visuali della finestra Avalonia e navigazione a cinque sezioni.

## Verifica visiva obbligatoria (confronto diretto v14/v15)

Eseguire `./run-game.cmd --v14` e `./run-game.cmd` e confrontare le cinque schede alle risoluzioni **1040×740** e **1240×850**. Verificare che non ci siano spostamenti su hover/focus, sovrapposizioni, scrollbar che nascondono ETA, testo tagliato o pulsanti fuori schermo. Verificare almeno due selezioni di edificio bloccato e costruibile, e un edificio con consumo e capacità. Controllare i progressi di crescita/ricerca/soldati, i tooltip di capienza e la persistenza della selezione.

Per attivare una filiera con partita di sviluppo occorrono edifici prerequisito, materiali sufficienti e lavoratori liberi. Verificare che il vecchio `food-store` non sia acquistabile o mostrato nella v15 e che il nuovo Deposito consumi esattamente gli ingredienti del clock.

**Gate:** non procedere alla rimozione del checkpoint legacy se l'interfaccia RC63 non è compilata, verificata e accettata. In caso di nuovo errore, correggere la RC63 senza avviare altre milestone.
