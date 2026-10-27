# ADR 0012 — Identità visiva retro 8-bit per il desktop

- **Data:** 2026-10-27
- **Stato:** Accettato (direzione artistica; prototipo verificato su Windows, tipografia monospace applicata)
- **Ambito:** Desktop e futuri asset grafici, non simulazione

## Contesto

SimRisorse ha già un desktop Avalonia gestionale, con schede ed elenchi e nessun posizionamento di edifici su una mappa. L'utente desidera una futura GUI pixel art e ha scelto esplicitamente fra tre proposte lo stile **retro 8-bit essenziale**.

Le schermate gestiscono più risorse, quantità precise, flussi al secondo, lavoratori e stati di blocco: leggibilità e densità informativa devono prevalere sulle decorazioni.

## Decisione

- Il desktop definitivo userà icone e pannelli in **pixel art retro 8-bit essenziale**; palette ridotta, pixel netti, controlli chiari e informazioni numeriche leggibili.
- Su successiva richiesta dell'utente, anche la tipografia è **monospace**: Consolas, con fallback DejaVu Sans Mono, Liberation Mono e Courier New. Un'unica risorsa del tema si applica a testo, cifre e controlli; le dimensioni esistenti sono verificate nei layout compatto e standard.
- Si mantiene **Avalonia/MVVM**, con Core e Application indipendenti dagli asset. Non si introduce un motore grafico diverso per questa sola esigenza.
- La gestione degli edifici resta **per elenchi e schede**; non si introduce collocamento spaziale su mappa.
- Gli screenshot originali rimangono riferimenti funzionali, non sorgenti da cui copiare artwork.
- Il rifacimento visivo attende il gate Windows di M4 `civil-growth-4`. Prima verrà costruita una schermata campione, poi verificato il comportamento a risoluzioni e DPI differenti.

In **v25** è iniziata un'implementazione incrementale: icone originali a griglia 8×8 nelle viste Edifici/Risorse e pannelli più netti. Il prototipo ha superato il gate tecnico Windows; la successiva richiesta di tipografia retro è applicata con un font monospace. Ulteriori dimensioni sprite, palette, animazioni e navigazione definitiva restano proposte nel [documento di direzione grafica](../PIXEL_ART_UI.md).

## Alternative valutate

- **Pixel art medievale 16-bit ricca:** più dettagli, maggiore produzione di asset; non scelta.
- **Diorama dell'isola come interfaccia primaria:** più spettacolare, ma in contrasto con la gestione tramite elenco se richiedesse collocazione spaziale; non scelta.
- **Retro 8-bit essenziale:** scelta dall'utente, con attenzione alla leggibilità.

## Conseguenze

Il prototipo v25 non modifica `civil-growth-5`, formato dei salvataggi o funzionalità di gameplay. I nuovi test Desktop aumentano i casi attesi a **177** e vanno eseguiti insieme alle verifiche di rendering, accessibilità tramite testo e regressione delle viste gestionali.
