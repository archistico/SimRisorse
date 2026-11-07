# RC83 — Piano test E4b.2e / 06/11/2026

**Baseline dichiarata dall'utente:** RC82 = **1002 test passati**. Non è stato consegnato un TRX in questa conversazione, ma il risultato dell'utente è il checkpoint operativo di riferimento.

**RC83:** 18 nuovi casi xUnit Application; **1020 test cumulativi attesi**. Il runtime di questa consegna non dispone di SDK .NET 10, pertanto nuovi test, compilazione e CLI **non eseguiti**: versione **candidata**.

## Copertura aggiunta

- 9 casi parametrizzati per ogni curva A/B/C × tier Base/Grande/Avanzato: capienza lavoratori 10/30/100 e 20/60/200, contatori e quote progressive; differenze cumulative di costo/edifici dell'intera filiera, popolazione e personale non riutilizzato; verifica del prezzo atomico e delle quote singole; valutazione route Warehouse e minimo rate; equivalenza materiale Tagliapietre.
- 5 casi sui bordi delle fasce lavoro equivalente: 0, 99.999, 100, 1000, 1000.001.
- 4 casi: esattamente nove righe con provenienza proposta; serializzazione deterministica e immutabilità del catalogo; esportazioni CSV/Markdown e dichiarazione dei limiti; rifiuto di input negativo o nullo.

## Gate Windows

```powershell
.\validate-f1.cmd
```

Attesi 1020/1020 test verdi, failed/skipped=0, smoke nativo e audit precedenti. Il gate deve eseguire il comando CLI `--study-e4b-mint-scale --output <directory>` e verificare 9 righe, 3 curve, 3 tier, `Mode=analytical-proposal/not-applied`, percorso Magazzini e tre file prodotti. Proibito prendere un gate verde come prova che una seconda Zecca sia costruibile nella partita.

## Controlli manuali pendenti

1. Eseguire le due policy naturali MintTen a orizzonte completo, senza `--max-seconds`, con confronto opzionale Legacy; salvare gli output reali e il checksum.
2. Analizzare `Outcome`, `CompletionSeconds`, eventi di fallimento, alloggi, cibo, ricercatori, scorte, capienze e attese: distinguere insufficienza di strategia, prerequisiti ed eventuali softlock strutturali.
3. Confrontare i rapporti RC83 con gli esiti naturali e decidere A/B/C, Magazzini, pagamento progressivo e default MintTen per **nuove** partite soltanto dopo evidenza; non affermare che la Zecca debba già costare 100–1000 Tagliapietre.
4. Verifica visiva manuale della GUI approvata a 1040×740 / 1240×850; non cambiata in RC83.

**Modificati:** nuovo file benchmark Application, file test, CLI e suo dispatch, gate PS1, ADR/documentazione. **Non modificati:** src Core, Infrastructure, Desktop, formati DB, tariffe, ristorazione, famiglia di edifici e crescita ×20.
