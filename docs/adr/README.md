# Registro delle decisioni architetturali

Gli ADR documentano una decisione, le alternative considerate e le conseguenze. La data iniziale del registro è il 2026-10-26. Lo stato di un ADR non indica che la relativa funzionalità sia già implementata.

## Stati

- **Proposto**: scelta candidata, ancora da validare o confermare.
- **Accettato**: vincolo adottato per lo sviluppo successivo; l'implementazione può essere ancora assente.
- **Superato**: sostituito da un nuovo ADR, che deve essere collegato al precedente.
- **Respinto**: proposta valutata e non adottata, conservata con la motivazione.

Le preferenze del prodotto espresse dall'utente sono distinte dalle scelte tecniche di impostazione. Le regole di gioco ancora aperte sono in [Game design](../GAME_DESIGN.md), senza trasformarle implicitamente in decisioni attraverso un ADR.

## Indice

| ADR | Titolo | Stato | Implementazione |
| --- | --- | --- | --- |
| [0001](0001-modular-monolith-and-vertical-features.md) | Monolite modulare e funzionalità verticali | Accettato | Core, Application, Infrastructure, Desktop e Simulation implementati |
| [0002](0002-simulation-time-and-pause.md) | Tempo di simulazione, pausa e riproducibilità | Accettato | Core e adattatore monotono desktop verificati |
| [0003](0003-versioned-content-catalog.md) | Catalogo versionato e stato separato dalle definizioni | Accettato | Catalogo immutabile e validato in memoria, con analisi di raggiungibilità degli sblocchi; formato esterno futuro |
| [0004](0004-population-and-military-boundaries.md) | Popolazione e progressione militare come modelli espliciti | Accettato | Assegnazioni e crescita civile implementate; militari futuri |
| [0005](0005-desktop-stack-candidate.md) | C#/.NET, Avalonia e SQLite per il primo desktop | Accettato | Desktop definitivo e adattatore SQLite collegati in M3 |
| [0006](0006-economic-step-and-resource-precision.md) | Precisione delle risorse e prenotazioni nel passo economico | Accettato | Implementato e verificato in M2 |
| [0007](0007-desktop-persistence-and-lifecycle.md) | Desktop, salvataggi e ciclo di vita della partita | Accettato | Implementato e verificato in M3 |
| [0008](0008-avalonia-12.md) | Avalonia 12 per il desktop | Accettato | Avalonia 12.1.4, xUnit 3 headless e provider SQLite aggiornati e verificati |
| [0009](0009-civil-growth-and-save-generations.md) | Crescita civile e generazioni di salvataggio | Accettato | Primo scenario civile, snapshot/file v2 e compatibilità v1 verificati |
| [0010](0010-storage-capacity-and-rules-conversion.md) | Capacità di magazzino e conversione esplicita delle regole | Accettato | Magazzino, `civil-growth-2`, conversione da `civil-growth-1` e copia di sicurezza verificati |
| [0011](0011-advanced-housing-and-bread-rations.md) | Pane separato e razione degli abitanti avanzati | Accettato | `civil-growth-4` implementato, gate Windows e benchmark pendenti |
| [0012](0012-retro-8bit-desktop-visual-language.md) | Identità visiva retro 8-bit essenziale | Accettato | Progettazione soltanto; futura GUI pixel art non ancora implementata |
| [0013](0013-accelerated-housing-and-fractional-growth.md) | Case che accelerano gli arrivi, frazioni e compatibilità SQLite v2 | Accettato | `civil-growth-5` implementato; gate Windows v24 da eseguire |

## Come aggiungere una decisione

1. Copiare il [modello](TEMPLATE.md), assegnando il successivo numero a quattro cifre.
2. Descrivere il problema concreto e le alternative realistiche.
3. Specificare cosa viene deciso e cosa resta aperto.
4. Collegare i documenti e le milestone interessate.
5. Aggiornare questo indice; aggiornare roadmap e handoff se cambia il lavoro successivo.

Una decisione accettata non si riscrive per nascondere un cambio di direzione. Si crea un nuovo ADR e si marca il precedente come superato. Le correzioni editoriali sono consentite.
