# E4b / RC78 — tre curve analitiche

Questo strumento confronta proposte; non modifica il gioco e non simula i nuovi tempi di campagna. I coefficienti sono definiti in `E4bEconomicCurveStudy.Curves`; [metodo](../../docs/design/E4B_RC78_THREE_CURVES_STUDY.md), [risultati](../../docs/validation/E4B_RC78_CURVE_COMPARISON.md), [verifiche](../../docs/validation/E4B_RC78_TEST_PLAN.md).

```powershell
dotnet run --project tools/SimRisorse.Simulation -c Release -- --study-e4b-curves --output .local-cache/e4b-v15/NUOVA_CARTELLA
```

Opzione `--baseline`: JSON o directory contenente `e4a-v15-industry-results.json` con i sei run naturali completi I01/I02/I03 × due politiche. Per riprodurre le misure, usare il runner indicato in [E4a v15](../e4a-v15/README.md) senza `--max-seconds`. Senza baseline nessun tempo è importato; non si fabbricano dati sostitutivi.

Cinque output nella directory nuova: `e4b-curves.json`, `e4b-quotes.csv`, `e4b-investments.csv`, `e4b-curves.md`, `baseline-source.json`. Tutte le metriche complete sono nel JSON; i CSV rappresentano le quantità per risorsa. Il totale con Magazzini di un percorso bloccato è nullo e non ha righe CSV come se fosse un prezzo zero. Lo stato del blocco resta nelle righe senza Magazzini e nel JSON. I dati generati restano nella cache.

Il gate `validate-f1.cmd` esegue lo studio senza una baseline locale obbligatoria: tre curve, 81 preventivi e 18 investimenti. Le misure naturali importate e il replay hanno evidenze separate. Coefficienti e moduli tier restano da decidere prima della RC79+.
