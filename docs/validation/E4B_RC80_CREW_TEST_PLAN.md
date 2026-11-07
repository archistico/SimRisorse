# RC80 — Piano di validazione squadre minime (E4b.2b)

**Stato:** codice candidato, NON compilato né eseguito in questo ambiente (manca SDK .NET 10). Ultimo gate effettivamente confermato: **RC78, 926/926**. RC79 resta non validata con 17 test aggiunti, RC80 aggiunge **22 test** (18 Core e 4 Application): **965 cumulativi attesi**, senza dichiararli passati.

## Matrice

- **Soglie reali del personale**: 0/9/10/19/20 su due Zecche Base, con 0/0/1/1/2 moduli attivi, senza perdita di risorse per squadre incomplete.
- **Taglie**: 1 Base = 10 persone, 1 Grande = 30 per tre moduli, 1 Avanzato = 100 per dieci moduli.
- **Nessun pooling illegittimo**: 9 addetti Base + 9 Grande non diventano una squadra da 18; completare solo una delle due squadre attiva esattamente un modulo.
- **Vettori fisici**: Oro e Carbone decurtati soltanto per ricette intere; Monete nei limiti di spazio; con input esauriti o output pieno non c'è consumo fantasma.
- **Concorrenza priorità**: produttore con priorità superiore usa il Carbone disponibile prima della Zecca, senza doppia spesa.
- **Invarianti strutturali**: roster completo, identificatori validi, team entro capienza, abitanti disponibili inclusa riserva esterna, nessun overflow, modifiche immutabili. Il percorso `CrewSize=1` conserva la semantica dei lavoratori singoli.
- **Determinismo**: nessuna mutazione del catalogo, report identici a parità di input e cultura linguistica; dieci casi CLI controllati.

## Gate da eseguire quando disponibili i test su Windows

```powershell
.\validate-f1.cmd
# 965 test attesi, zero falliti o ignorati, smoke desktop e audit F1;
# inclusi gli studi RC78/RC79 e il nuovo studio RC80.

dotnet run --project tools/SimRisorse.Simulation -c Release -- --study-e4b-crews --output .local-cache/e4b-v15/rc80-manuale-001
```

Ripetere il report in una directory diversa e confrontare JSON/CSV/Markdown byte per byte; verificare che il comando rifiuti la sovrascrittura delle proprie tre uscite. La verifica interattiva della GUI v14/v15 è ancora pendente e non deve essere sostituita da una cattura automatica.

**Non compreso:** assegnazioni con la popolazione reale v15, ricette simultanee del deposito alimentare, transazioni SQLite, upgrade comandati, evoluzione dei prezzi, payoff con Magazzini o durata della partita. Sono gate distinti per il prossimo incremento.

[Specifica](../design/E4B_RC80_CREW_MINIMUM_PROJECTION.md) · [ADR 0088](../adr/0088-proiezione-squadre-minime-v15-isolata.md).

[Audit statico pacchetto](RC80_STATIC_AUDIT.md).
