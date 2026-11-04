# E4a-D01 / RC37 — Piano di validazione

**Checkpoint confermato:** RC30 / 511 test. **RC37:** 541 test attesi, **non eseguiti**. I test RC31–RC36, inclusi in questa release, sono ancora pendenti.

## 1. Gate .NET e benchmark

```powershell
.\validate.cmd
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-school --output .local-cache/e4a/school-run-001
dotnet run --project tools/SimRisorse.Simulation -c Release -- --benchmark-e4a-school --output .local-cache/e4a/school-run-002
```

Controllare `541/541` e l'eventuale TRX; se c'è un errore, **non** marcare il gate superato. Il benchmark deve stampare gli esiti di E4A-06 e E4A-07, creare due file JSON/CSV per ogni run e uscire con codice 0 soltanto se entrambi sono `pass`.

## 2. Accettazione E4a-D01

- La dotazione controllata è documentata e identica inizialmente: +600 Legno lavorato, +400 Pietra, +10 Carbone; la seconda Scuola è pagata davvero.
- La coda iniziale è diversa: una Scuola ha 1 studente in formazione e 1 in attesa, due Scuole hanno 2 in formazione. La prova testa il clock reale, non un contatore artificiale.
- La formazione applica le regole correnti (120 s, due Matite a testa) e ogni campione registra le scorte e il personale senza valori inventati.
- La suite aggiunge **3 test** su esito/tempi delle due Scuole, ripetibilità e serializzazione CSV senza checkpoint o hash fittizi.
- Verificare che il secondo comando con la **stessa directory** non sovrascriva i risultati; usare un percorso nuovo.
- Non modificare `benchmarks/e4a/scenarios.csv` da `not-run` a `pass` finché non è stata effettivamente eseguita una misurazione e registrata la relativa evidenza.

## 3. Protezioni

- Non scrivere mai nella directory `%LOCALAPPDATA%\SimRisorse` e non aprire salvataggi reali.
- Nessun cambiamento ai coefficienti economici o ai cataloghi di gioco; `run-game.cmd` resta v14 predefinito.
- La GUI v13 e i suoi salvataggi rimangono disponibili tramite `--legacy-v13`.
- I risultati di E4A-06/07 **non** dimostrano raggiungibilità della Scuola in una partita naturale.

[Rapporto D01](../design/E4A_D01_RC37_BENCHMARK_SCUOLE.md) · [Roadmap](../ROADMAP.md) · [Handoff](../NEW_CHAT_HANDOFF.md)
