# ADR 0070 — La v15 diventa l'avvio normale, con filtro delle filiere

**Data:** 05/11/2026 · **Stato:** adottata nello sviluppo RC62, da validare su Windows · **Milestone:** F1-D05e.2b.1-fix.

## Problema

Nella RC61 gli edifici alimentari esistevano nel catalogo e nella GUI v15, ma `run-game.cmd` senza opzioni apriva ancora la v14. Il feedback dell'utente «non vedo ancora macelleria ecc.» descrive un problema concreto di accessibilità delle nuove funzioni, non una mancanza del catalogo.

## Decisione

1. L'avvio senza argomenti usa v15 e lo slot separato `civil-growth-v15.sqlite3` (SQLite gen. 6). `--v15` rimane valido; `--v14`/`--experimental-v14` mantengono disponibile la GUI storica con il medesimo DB personale.
2. Gli edifici alimentari devono essere rintracciabili direttamente tramite un filtro locale «Filiere alimentari (6)» / «Tutti gli edifici» all'interno della schermata Edifici. Il filtro è di sola presentazione e non muta il catalogo, i blocchi di ricerca, i lavoratori o la sessione.
3. Conservarne lo stile pixel art, la tabella Risorse a sette colonne e i tier direttamente costruibili. La nuova scelta modifica l'avvio, non converte automaticamente i vecchi salvataggi sperimentali; DB v15 gen. 5 rifiutati senza alterazione.
4. L'ADR 0065 descrive storicamente la scelta precedente della modalità **opt-in**; questa ADR sostituisce **solo** il comportamento predefinito, non le garanzie di isolamento dei salvataggi.

## Alternative

Lasciare v14 predefinita e chiedere sempre `--v15`: non soddisfa l'aspettativa di vedere le nuove filiere nel gioco corrente. Sostituire completamente la v14 o unirne i DB: rischio evitabile per i dati personali senza beneficio. Il filtro v15 è un piccolo intervento di sola UI, più semplice di un secondo catalogo.

## Conseguenze

I test Desktop che assumevano `DesktopOptions.Parse([]).UseV14` vanno aggiornati per verificare **la v14 esplicita**. La riconciliazione dei test SQLite 5→6 va eseguita nello stesso hotfix per sbloccare la suite. La rimozione dell'adattatore civico v14 dal checkpoint v15 resta F1-D05e.2b.2.
