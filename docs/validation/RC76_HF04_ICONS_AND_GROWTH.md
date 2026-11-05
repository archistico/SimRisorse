# RC76-HF04 — Icone, intestazione e crescita v15 ×20 (05/11/2026)

## Richiesta applicata

- Risorse: Bovini con testa di mucca bianca, corna e muso rosa; Pane con crosta più marrone.
- Edifici: Pescatore rappresentato da un pesce; Tagliapietre da un cubo grigio con facce illuminate e in ombra.
- Intestazione v15: Abitanti, Cibo, Legno lavorato, Pietra, in questo ordine. Il riepilogo non sostituisce più Pietra con Pane.
- Pulsanti v15: «Carica» e «Salva», senza suffisso nel testo. Comandi e destinazioni dei salvataggi restano quelli esistenti.
- Crescita v15 fino a **×20** rispetto al ritmo base, su richiesta esplicita dell'utente. È una modifica autorizzata del precedente tetto ×2.

Le icone restano matrici 8×8 disegnate dal renderer condiviso `PixelGlyph`. La nuova tinta del Pane non modifica il colore di grano e tetti.

## Regola di crescita

Il moltiplicatore v15 è:

```text
min(20, 1 + 0,15 × max(0, case_piccole − 1) + 0,30 × case_grandi)
```

I bonus per casa sono quelli precedenti; cambia il limite superiore. Si raggiunge ×20, per esempio, con 128 case piccole oppure 1 piccola e 64 grandi. Con l'intervallo canonico di 60 secondi si arriva a un abitante ogni **3 secondi**, se ci sono alloggi, Cibo e Pane richiesto. La partenza con una casa piena non genera nuovi abitanti.

Il forecast della sidebar e il clock v15 usano la stessa funzione di calcolo. `HousingGrowthRules.Rate` accetta un tetto esplicito; i chiamanti storici continuano a usare il limite precedente, mantenendo riproducibili i benchmark storici.

Il progresso resta espresso in ventesimi interi di secondo; non cambiano lo schema SQLite generation 6 o il formato dei checkpoint. Le partite v15 caricate applicano il nuovo limite ai tick successivi, conservando il progresso già salvato.

## Verifiche

`V15HousingGrowthTests`: **11 casi nuovi** (7 combinazioni abitative, crescita e ripresa con tick frazionari, 3 vincoli alimentari/abitativi). Confermati:

- bonus iniziali ×1,15 e ×1,30, crescita oltre ×2, avvicinamento e arresto a ×20;
- accordo fra forecast e clock;
- un arrivo ogni 3 secondi al massimo, con equivalenza fra avanzamento continuo e ripresa da checkpoint;
- pausa senza avanzamento e mancata crescita se mancano Cibo, Pane o posti nelle case.

Eseguito `./validate-f1.cmd`, incluso `validate.cmd`, su Windows/.NET SDK 10.0.201:

| Controllo | Esito |
| --- | --- |
| Build Release | 0 errori, 0 avvisi |
| Suite cumulativa | **885/885**, nessun fallito o saltato |
| Core / Application / Infrastructure / Desktop | 212 / 350 / 129 / 194 |
| Simulazioni e benchmark storici | Superati |
| Smoke nativo esistente | Superato |
| Audit alimentari v15 baseline/combinato | 120s ciascuno, superati |

Gate: `.local-cache/m4/f1-gates/85154b9cf1ec4f689c085200eed2116a/f1-gate.json`, concluso alle `2026-11-05T22:04:34.2906671Z`.

TRX e catture: `.local-cache/m4/runs/6bff832e9ef849228882bfe80177c125/`.

Anteprima delle quattro icone e delle finestre alle due risoluzioni, ispezionata da Codex: `.local-cache/captures-b11dc90e31c7456f923878cd8a4624e4/` (`icons.png`, `v15-1040-0.png`, `v15-1240-1.png` e ulteriori viste). Sono render Avalonia Headless/Skia; il confronto interattivo Windows previsto da F1 rimane distinto.

I risultati precedenti [HF03](RC76_HF03_VISUAL_REGRESSIONS.md) e [HF02](RC76_WINDOWS_VALIDATION.md) restano storici. Non sono state prodotte nuove misure economiche a orizzonte pieno.
