# RC76-HF03 — Regressioni grafiche v15 (05/11/2026)

## Difetti riprodotti e corretti

Il confronto delle finestre v14/v15 renderizzate con Avalonia Headless/Skia alle risoluzioni 1040×740 e 1240×850 ha evidenziato questi difetti della v15:

- A 1040×740 il filtro delle filiere riduceva la colonna «STRUTTURA» a 18 pixel per un testo largo circa 59,4 pixel, sovrapponendo le intestazioni. Il filtro occupa ora una riga sopra le tre colonne, che mantengono lo spazio disponibile e i contatori allineati.
- Le quotazioni mostravano gli ID `planks` e `stone`, mentre le righe con icone usavano i nomi italiani. `FormatCost` usa ora la stessa risoluzione dei nomi delle risorse per edificio, batch, potenziamenti e capacità.
- Le descrizioni di Matite/Cibo nella Scuola e Spade nell'Esercito venivano misurate senza limite orizzontale dallo StackPanel. Una griglia icona/testo assegna ora al testo la larghezza effettiva della card e consente di andare a capo.

Le azioni restano nelle righe esistenti, i cinque pannelli mantengono il layout canonico. Nessuna modifica a prezzi, formule, produzione o SQLite.

## Regressioni automatiche

`V15VisualRegressionTests` aggiunge **5 casi**:

1. Intestazioni e filtro leggibili, senza sovrapposizioni, nei due stati del filtro a 1040×740.
2. Stesso controllo a 1240×850.
3. Materiali di Scuola/Esercito contenuti nelle card con ordine +10 a 1040×740.
4. Stesso controllo a 1240×850.
5. Nomi italiani nelle quotazioni singole e batch, coerenti con le righe con icone.

Prima della correzione i nuovi test hanno riprodotto il difetto delle intestazioni, i nomi tecnici e l'overflow dei materiali. Dopo le modifiche tutti e cinque passano.

`validate.ps1` richiede ora anche **8 catture v15** della stessa esecuzione: Edifici con filtro disattivato/attivo, Scuola ed Esercito alle due risoluzioni. Il gate F1 richiede **874 test** (869 precedenti + 5), con milestone aggiornata a HF03.

## Esito Windows

`./validate-f1.cmd` eseguito da Codex su Windows con .NET SDK 10.0.201, incluso `validate.cmd`:

| Controllo | Esito |
| --- | --- |
| Build Release | 0 errori, 0 avvisi |
| Suite cumulativa | **874/874**, 0 falliti, 0 saltati |
| Core / Application / Infrastructure / Desktop | 212 / 339 / 129 / 194 |
| Simulazioni e benchmark di validate.cmd | Superati |
| Smoke nativo esistente sui due scenari | Superato |
| Audit v15 baseline e combinato | 120s ciascuno, superati |
| Catture richieste | 25, comprese le 8 nuove v15 |
| Ispezione delle 8 nuove catture da parte di Codex | Eseguita alle due risoluzioni |
| Stress combinato 24h | Non eseguito |

Gate concluso alle `2026-11-05T21:50:24.8124093Z`: `.local-cache/m4/f1-gates/17620fd73dc54c2ea80bedb51617f20e/f1-gate.json`.

TRX e screenshot: `.local-cache/m4/runs/7eb012f2b47d415897b5d6296ca323bb/`.

Il primo tentativo del gate era stato fermato dal ripristino NuGet (`NU1900`, accesso all'indice per l'audit vulnerabilità). La successiva esecuzione con accesso autorizzato ha completato il ripristino e il gate, senza disabilitare l'audit.

## Limiti della verifica visiva

Le catture v15 sono render reali di Avalonia Headless/Skia su dati nuovi e isolati; sono state ispezionate direttamente. Non sono screenshot di una sessione interattiva Windows. Lo smoke nativo esistente usa la finestra storica `MainWindow`, non le finestre v14/v15. Restano da verificare nella GUI Windows confronto interattivo, hover/focus, tooltip, scorrimento e progressi di una campagna attiva, come richiesto dal piano RC63.

**F1-D05e resta formalmente aperta per tali controlli interattivi.** RC63 rimane il riferimento grafico approvato. Queste correzioni rimuovono regressioni osservate prima di passare a RC77/E4a.3.

La [validazione HF02](RC76_WINDOWS_VALIDATION.md) con 869 test e la diagnostica industriale da 120s restano evidenze storiche; non sono misure a orizzonte pieno.
