# RC59 — F1-D05e.2a — Stress combinato delle filiere v15

**Data:** 5 novembre 2026. **Base validata:** RC58 con 744/744 test superati dall'utente e audit standard v15 su 24 ore completato. **Stato RC59:** implementata, da verificare con .NET 10 su Windows.

## Perché questo incremento

L'audit precedente `--audit-v15 --hours 24` impiegava Pescatore e Deposito: il risultato **86.400 secondi**, **86.400 razioni servite**, **0 mancanti**, **0 squilibri**, **0 Cibo dal `food-store`** conferma il percorso pesce/Deposito, ma non dimostra l'esecuzione congiunta delle altre filiere.

## Scenario combinato deterministico

- Città sintetica: **24 abitanti**, **3 Case piccole**; nessun salvataggio personale letto o modificato.
- Produttori storici alimentari: **1 Pescatore**, **3 Fattorie**, **7 Pozzi**, **1 Mulino**, **1 Forno**; **13 lavoratori aggiuntivi**. Sono inoltre mantenuti **3 lavoratori iniziali** (Taglialegna, Segheria, Tagliapietra), per **16 lavoratori delle famiglie produttive storiche**.
- Nuovi siti: **1 Stalla**, **1 Cacciatore**, **1 Macellaio**, **1 Orto**, **1 Frutteto**, **1 Deposito alimentare**, con **1 addetto per ciascuno**; **6 lavoratori**.
- Budget corretto in RC60: **22 occupati su 24**, **2 disponibili** (3 iniziali + 13 alimentari storici + 6 filiere v15). La RC59 indicava erroneamente 19/5, trascurando i lavoratori già assegnati alla colonia iniziale. Le scorte iniziali di Grano (50), Acqua (50) e Farina (10) sono **finanziamenti sintetici per lo stress**, non ricompense di gioco né un benchmark di progressione naturale.
- Il clock v15 reale produce e consuma risorse; nessuna nuova formula economica o ricetta viene introdotta.

## Nuovo comando

```powershell
# Audit precedente, invariato:
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --hours 24

# Nuovo audit con tutte le filiere:
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --hours 24 --profile combined

# Smoke test breve:
dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 120 --profile combined
```

Il comando aggiuntivo accetta fino a **48 ore simulate** e verifica il bilancio fisico **a ogni secondo**. Il report JSON fornisce secondi, razioni servite/mancate, popolazione finale, Cibo erroneamente prodotto nella città, squilibri, e produzione lorda cumulativa (in millesimi) di Bovini, Selvaggina, Carne, Verdura, Frutta, Pane e Cibo pronto. Un bilancio scorretto causa errore esplicito con secondo e ID della risorsa. Per `--seconds 1`, il Macellaio non ha ancora animali del tick precedente; il gate delle sette produzioni richiede almeno due secondi.

## Regole e limiti

La produzione lorda indica che lo stadio è stato effettivamente esercitato, **non** che tutti gli ingredienti siano stati convertiti nel Deposito nello stesso periodo: le priorità delle ricette, la capienza e il personale influenzano il mix di conversione. La verifica indipendente include tutti i compartimenti, senza conteggiare gli spostamenti come nuova produzione. Lo stress non sostituisce E4a: lo scenario è intenzionalmente finanziato e la popolazione parte già alloggiata.

La presenza strutturale di `food-store` nel `V14CampaignSnapshot` inglobato dalla v15 **non è stata rimossa in RC59**; resta inattiva nei comandi v15 e nel clock. La sua eliminazione dal formato persistente richiede il passaggio successivo **F1-D05e.2b**, con versione SQLite di sviluppo esplicitamente nuova, adattatore dei comandi legacy e regressioni per migrazione, ricerca e salvataggio. I database v14 personali rimangono intatti; la compatibilità dei database sperimentali è sacrificabile su decisione dell'utente.

## File interessati

- `src/SimRisorse.Application/Benchmarks/V15CombinedFoodStressScenario.cs`
- `tools/SimRisorse.Simulation/V15CombinedStressAudit.cs`
- `tools/SimRisorse.Simulation/Program.cs`
- `tests/SimRisorse.Application.Tests/F1D05e2CombinedFoodStressTests.cs`
- `tests/SimRisorse.Infrastructure.Tests/F1D05e2CombinedFoodPersistenceTests.cs`

[Roadmap](../ROADMAP.md) · [Piano test](../validation/F1_D05E2A_RC59_TEST_PLAN.md) · [ADR 0068](../adr/0068-stress-combinato-filiere-e-checkpoint-legacy.md).
