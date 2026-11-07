# RC94.1 — Piano di validazione della riserva Municipio

**Baseline confermata:** RC93.1 (1220 test). **RC94 non confermata:** +12 test. **RC94.1:** +1 nuovo test Application; **1233 attesi**, nessuna verifica .NET 10 locale.

Eseguire su Windows dalla radice del progetto:

```powershell
.\validate-f1.cmd
```

## Contratti verificati

1. Nuova v15: 2 Matite + 100 Carbone, oltre a Legno lavorato, Pietra e Cibo del catalogo originale.
2. Municipio fisico: capacità Carbone 1.000, dotazione non superiore al massimo.
3. Scuola costruita secondo prerequisiti ordinari: `CraftPencils(1)` prima di `Estrazione I` consuma 1 Legno lavorato + 1 Carbone e produce 4 Matite. Nessuna scorciatoia su ricerca o Miniera.
4. La migrazione/importazione da v14 non inventa né Matite né Carbone.
5. Salvataggio e ricaricamento di una partita v15 che ha consumato le scorte: quantità persistite senza refill; nuova partita separata riceve la dotazione una sola volta.
6. Benchmark naturali, industriali e audit di bootstrap RC87 osservano la medesima dotazione e rifiutano inventari alterati.
7. RC94 sandbox e gate F1 grafico rimangono invariati.

**Da dichiarare conclusa soltanto** dopo 1233 test superati e gate F1 verde.

## Esito successivo e correzione RC94.2

Esecuzione Windows RC94.1: 1228 superati, 5 falliti. I cinque casi di `Rc81LiveMintCrewTests` confrontavano erroneamente il saldo assoluto di Carbone con 6/5/4. Il codice di produzione non viene corretto: il test è stato aggiornato in RC94.2 per misurare il delta di consumo. Vedi [Piano RC94.2](RC94_2_MINT_CREW_STARTER_COAL_HOTFIX.md).
