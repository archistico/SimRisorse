# RC94 — Piano di validazione del checkpoint composito in sandbox

**Baseline confermata:** RC93.1, 1220 test passati (comunicazione dell'utente).  
**RC94:** +12 test Application, **1232 attesi**, non ancora eseguiti qui. Compilazione .NET 10 e gate Windows da effettuare.

## Riproduzione

Dalla radice del progetto su Windows con SDK .NET 10:

```powershell
.\validate-f1.cmd
```

Per verificare il nuovo smoke in modo isolato, dopo la build:

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --study-e4b-v15-shadow
```

La CLI deve restituire JSON `Version=rc94-v15-shadow-1`, `InitialRevision=0`, `DepositedRevision=2`, `RefundedRevision=3`; le sei condizioni di riconciliazione/isolamento devono essere `true` e `ApprovalStatus` deve indicare esplicitamente `not-approved`. Il gate F1 rifiuta un report diverso.

## Casi test coperti

1. Apertura soltanto da un checkpoint v15 validato: capacità fisica, risorse, storico `mint`.
2. Congelamento del preventivo senza debitare la città o creare edifici.
3. Versamento di 1 unità di Legno lavorato: scarico coerente da città e credito all'escrow.
4. Round-trip del checkpoint composito con prezzo e revisione conservati.
5. Annullamento che ricostruisce esattamente le scorte iniziali.
6. Rifiuto delle revisioni obsolete, senza mutazioni.
7. Rifiuto di materiale assente, pagamento vuoto ed eccedenze.
8. Rifiuto di inventario città diverso dallo stock del contratto.
9. Rifiuto di rifornimenti ipotetici del vecchio sandbox.
10. Rifiuto di contatore o capacità falsificati.
11. Un preventivo non pagato non equivale a un edificio pronto.
12. Rifiuto di versione/revisione non valida e spese sandbox completate non registrate nel gioco.

## Limitazioni vincolanti

Questo non collauda una costruzione finita nella partita: `Readiness` è soltanto una verifica preliminare, non un commit. Non salvare `E4bV15FundingShadowCheckpoint` nello schema SQLite della partita e non trasformarlo in `BuildCityProducer` senza un nuovo contratto transazionale. I tempi di progressione naturale con Curve A non sono stati misurati.
