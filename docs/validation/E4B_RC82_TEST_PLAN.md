# RC82 / E4b.2d — Piano di validazione, stato 06/11/2026

**Stato della consegna: candidata.** [Audit statico](RC82_STATIC_AUDIT.md). SDK .NET 10 non presente in questo ambiente; **nessun nuovo test xUnit eseguito**, nessun TRX RC79–82 prodotto. I 926 test RC78 sono l'ultima verifica confermata dall'utente; non riportare successi inesistenti.

| Versione | Test aggiuntivi | Cumulativi attesi | Esecuzione |
| --- | ---: | ---: | --- |
| RC78 | — | 926 | 926 superati (confermati) |
| RC79 | +17 | 943 | da verificare |
| RC80 | +22 | 965 | da verificare |
| RC81 | +20 | 985 | da verificare |
| **RC82** | **+17 Application** | **1002** | **da verificare** |

## Nuove verifiche scritte

- Due politiche `MintTen` su partenza naturale; `ScenarioId`, `RulesVersion`, inventario identico a `Legacy` e zero grant.
- Contratto `staff-mint-team` che sostituisce **solo** `staff-mint`; stesso albero di requisiti e stessi acquisti; assunzione atomica di dieci persone.
- Modalità sconosciute, scenario diverso o orizzonte negativo rifiutati.
- Replay diagnostici da 0/1/61 secondi senza false milestone, confronto deterministico degli output e assenza di mutazioni del punto di partenza.
- Analisi con entrambe le modalità presente, ma nessuna classifica temporale se entrambe non completano a orizzonte pieno.
- Default `Legacy` esplicito e implicito con risultati identici.

## Gate quando possibile su Windows

```powershell
.\validate-f1.cmd
```

Attesi **1002/1002**, `failed=0`, `skipped=0`, smoke GUI, audit alimentari, RC77 recupero, RC78 curve, RC79 finanziamento, RC80 studio squadre e RC82 diagnostica naturale 0 secondi. Il gate confronta versioni `Legacy` e `MintTen`, tutte `fresh-game/no-grants`, e verifica nove file del report. Il controllo istantaneo NON dimostra che la nuova Zecca sia raggiungibile.

Eseguire poi separatamente il **run a orizzonte completo** per entrambe le strategie (vedere specifica) e conservare output e checksum. Esaminare popolazione/alloggi, lavoratori liberi, assegnazioni, capacità input/output e cause di attesa. Non riportare un confronto numerico RC77/RC82 prima di avere completamenti effettivi. Verifica visiva a 1040×740 e 1240×850 della GUI v14 canonica ancora aperta.

## Invarianti non toccati

Nessun file Desktop, Core, Infrastructure, SQLite, catalogo o bilanciamento modificato da RC82; cambia soltanto il runner di benchmark Application (senza modificare i vecchi percorsi) e la CLI separata. GUI v14/v15, sette colonne Risorse, crescita ×20 e deposito unico Cibo sono invariati.
