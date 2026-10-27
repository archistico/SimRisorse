# M4 — Risultati misurati e revisione dei render

Data: 2026-10-27. **Stato: verificato su Windows**, dopo la correzione della palette condivisa di `PixelGlyph`. Questo rapporto consolida il gate tecnico e la lettura dei quattro benchmark; la chiusura progettuale di M4 resta aperta.

## Evidenza e riproducibilità

Esecuzione di `.\validate.cmd` terminata con codice 0, SDK **10.0.201**, Avalonia **12.1.4**, Microsoft.Data.Sqlite **10.0.14**. Build Release con **zero avvisi e zero errori**.

| Suite | Superati | Falliti |
| --- | ---: | ---: |
| Core | 67 | 0 |
| Application | 46 | 0 |
| Infrastructure | 26 | 0 |
| Desktop | 38 | 0 |
| **Totale** | **177** | **0** |

I conteggi sono stati controllati nei quattro TRX. Completati i controlli di raggiungibilità dei cataloghi, le simulazioni di riferimento, il confronto abitativo storico/accelerato e i due confronti del Pane. Pubblicazione e prove native Windows superate per entrambi gli scenari: salvataggio, caricamento, ripresa della crescita, notifica di sospensione e chiusura.

Artefatti locali dell'esecuzione: `.local-cache/m4/runs/98d961210607483a8afe8d708039efab/`.

- `test-results/`: quattro TRX.
- `screenshots/`: nove PNG ispezionati.
- `m4-report.md`: rapporto prodotto da `scripts/report-m4.ps1`.
- `benchmark-inputs/`: copie dei quattro JSON di benchmark e di `validation.json` utilizzati per questa lettura.
- `benchmark-inputs.sha256.json`: impronte dei cinque input conservati.

Il lettore è stato eseguito anche sulle copie isolate: il testo coincide con il rapporto originale. Queste copie preservano i dati analizzati quando una successiva validazione sovrascrive i JSON nella cartella comune. Gli artefatti restano locali e non sono aggiunti al codice sorgente; le tabelle seguenti conservano le misure principali.

## Abitazioni: storico contro crescita accelerata

Tre strategie predeterminate, **28.800 secondi simulati ciascuna**. Confronto `civil-growth-4 → civil-growth-5`; tutti i secondi riconciliati.

| Strategia | 48 abitanti, s (v4 → v5) | 100 abitanti, s | 136 abitanti, s | Secondi risparmiati per 136 | Popolazione finale (v4 → v5) |
| --- | ---: | ---: | ---: | ---: | ---: |
| Solo case piccole | 2.461 → 1.754 | 5.581 → 3.326 | 7.741 → 4.406 | 3.335 | 136 → 136 |
| Case grandi, avvio standard | 2.680 → 2.084 | 5.800 → 3.781 | 7.960 → 4.861 | 3.099 | 144 → 144 |
| Case grandi, avvio anticipato | 2.561 → 1.965 | 5.681 → 3.662 | 7.841 → 4.742 | 3.099 | 144 → 144 |

**Risultato osservato:** il bonus anticipa tutte le soglie in tutti e tre i profili. Il profilo con sole case piccole raggiunge prima le soglie confrontate; le strategie con case grandi terminano con otto abitanti in più, grazie alla capienza. La popolazione finale uguale tra v4 e v5 non annulla il vantaggio sui tempi.

Questi profili non confrontano direttamente le quattro combinazioni a minimo costo per raggiungere 2× dell'[audit M4.13](../scenarios/HOUSING_INCENTIVE_AUDIT.md). Non dimostrano la strategia ottima né giustificano, da soli, cambiamenti a prezzi, razioni o bonus.

## Pane: confronto a 220 e 320 abitanti

Colonie sintetiche, regole storiche **`civil-growth-4`**, **1.800 secondi simulati per strategia**, tutti riconciliati. Cibo sufficiente e posti disponibili isolano il vincolo Pane.

| Avvio | Strategia | Popolazione finale | Secondi senza Pane | Pane finale | Addetti totali | Spesa aggiuntiva legno/pietra |
| ---: | --- | ---: | ---: | ---: | ---: | ---: |
| 220 | Una filiera | 247 | 180 | 6,295 | 14 | 0 / 0 |
| 220 | Potenziamento snello | 250 | 0 | 95,500 | 18 | 360 / 250 |
| 220 | Seconda completa | 250 | 0 | 95,500 | 19 | 420 / 280 |
| 220 | Seconda completa al minuto 15 | 249 | 53 | 94,165 | 19 | 420 / 280 |
| 320 | Una filiera | 338 | 662 | 5,170 | 16 | 0 / 0 |
| 320 | Potenziamento snello | 347 | 126 | 9,085 | 20 | 360 / 250 |
| 320 | Seconda completa | 350 | 0 | 92,500 | 21 | 420 / 280 |
| 320 | Seconda completa al minuto 15 | 344 | 315 | 92,905 | 21 | 420 / 280 |

A **220 abitanti**, snella e completa raggiungono 240 abitanti a 1.200 s e 250 a 1.800 s, con gli stessi Pane prodotto (**6.111**), consumato (**6.115,500**) e finale. In questo intervallo la snella risparmia **60 legno, 30 pietra e un lavoratore** rispetto alla seconda completa.

A **320 abitanti**, la snella raggiunge 340 abitanti a **1.270 s**, ma non 350 entro la durata del test; la completa raggiunge 340 a **1.200 s** e 350 a **1.800 s**. La spesa e il lavoratore aggiuntivi della completa eliminano le carenze nel profilo osservato. Il potenziamento tardivo recupera parte del ritardo rispetto alla sola filiera, senza recuperare la crescita persa prima dell'acquisto.

Il confronto conferma che la convenienza della snella dipende dalla domanda. I costi sono incrementali: non includono la costruzione della colonia iniziale. Le misure del Pane non descrivono la progressione accelerata v5 e non certificano le future otto/trentadue ore fino alla vittoria.

## Revisione dei nove render

Ispezionati Edifici e Risorse dei due scenari a **1040×740** e **1240×850**, più lo stato civile con Pane e residente avanzato a 1040×740.

- Icone visibili accanto ai nomi; quantità, flussi e segni dei saldi leggibili nelle viste osservate.
- Comandi di produzione, costruzione, personale e salvataggio contenuti nelle finestre, compresa la versione compatta. L'elenco edifici scorre nella propria area.
- Le nove risorse civili sono visibili nella tabella compatta. Nello stato avanzato restano visibili fabbisogni, diagnosi del Forno a deposito quasi pieno e comandi.
- I test Desktop già superati verificano anche prerequisiti sopra Costruisci, geometria invariata su hover e stati del pulsante. L'ispezione di un PNG statico non sostituisce queste prove interattive.

**Limite:** sono render headless. DPI e leggibilità su monitor reali non sono stati valutati in questa revisione; il giudizio estetico dell'utente sul prototipo D0.7 resta aperto.

## Punto di ripresa

Il gate tecnico è **verificato** e il lettore M4.12 è **eseguito**. Non sono state modificate meccaniche, GUI o salvataggi durante questa lettura.

Per chiudere formalmente M4 restano la valutazione del prototipo e la classificazione esplicita di [Carne](../scenarios/MEAT_DESIGN_REVIEW.md) e [demolizione](../scenarios/DEMOLITION_M4_REVIEW.md). La proposta operativa è rinviare queste due estensioni alla progettazione successiva, preservando la baseline misurata; il rinvio richiede ancora una scelta dell'utente.

La [scheda D0](../design/D0_DECISION_BOARD.md) conserva le decisioni necessarie prima di M5, inclusa la politica S1/S2 dei salvataggi. Nessuno studio industriale, militare o navale è approvato da questa verifica.
