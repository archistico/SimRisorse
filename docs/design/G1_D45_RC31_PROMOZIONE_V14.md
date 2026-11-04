# G1-D45 · RC31 — Promozione v14 a interfaccia predefinita

## Motivazione

L'utente ha confermato i test della RC30 e ha dichiarato di utilizzare la v14 come interfaccia abituale. Il precedente piano P0 prevedeva 44 screenshot, non ancora disponibili. L'approvazione del proprietario giustifica il passaggio alla v14 come modalità ordinaria **senza attribuire certificazioni grafiche formali non eseguite**.

## Diff applicativo

- `DesktopOptions`: apertura v14 predefinita; `--legacy-v13` esplicito, `--v14` e alias `--experimental-v14`; `--scenario` e `--smoke-test` restano legacy per compatibilità con gli script.
- I due profili mantengono database distinti; l'uso del percorso riservato della modalità opposta è rifiutato.
- `App.axaml.cs`: selezione della finestra secondo `UseV14`; smoke legacy invariato.
- Finestra e messaggi v14: tolti gli avvisi «anteprima/sperimentale», nessun layout modificato.
- `V14PreviewViewModel` e `V14PreviewWindow` mantengono per ora il nome interno delle classi, per evitare refactoring non necessario: il comportamento ora è quello della modalità ordinaria.

## Compatibilità dei salvataggi

Non si rinomina e non si trasferisce alcun file. La vecchia directory `experiments` nel percorso v14 è mantenuta deliberatamente per ritrovare le partite esistenti. Il database v13 non viene letto dalla sessione v14. La migrazione delle versioni rimane una fase distinta E4c.

## Limiti del lavoro

I test e la compilazione .NET 10 di RC31 devono essere eseguiti su Windows. Le 44 evidenze visive P0 non sono state create e rimangono un dossier opzionale per future regressioni. Nessun costo, schema SQLite, simulazione o ricetta modificata.

[ADR 0042](../adr/0042-promozione-v14-e-conservazione-slot.md) · [Piano di prova](../validation/G1_D45_RC31_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
