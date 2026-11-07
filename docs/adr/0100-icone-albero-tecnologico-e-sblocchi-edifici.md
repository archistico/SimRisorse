# ADR 0100 — Icone nell'Albero tecnologico e lista di edifici sbloccati

**Data:** 06/11/2026  
**Stato:** scelta di presentazione approvata dall'utente; implementazione RC90.3 candidata da validare.

## Contesto

Nel dettaglio RC90 il campo «Sblocca edifici» era testo con nomi separati da virgole. La UI edifici usa invece icone pixel art e nomi in righe, più leggibili. Le tecnologie in elenco erano prive di icona. La v14 approvata rimane il contratto grafico della v15.

## Decisione

1. In entrambe le viste v14 e v15, ogni riga dell'Albero mostra l'icona del ramo tecnologico nello stile `PixelGlyph` 8×8 già adottato (Lavorazioni: Segheria; Alimentazione: Pane; Estrazione: Miniera carbone; Industria: Forgia; Logistica: Magazzino; Navigazione: nuova nave originale 8×8).
2. Il dettaglio mostra una sezione **SBLOCCA EDIFICI** composta da righe con icona effettiva dell'edificio, nome leggibile e livello Grande o Avanzato. Evitare di ripetere lo stesso elenco nella stringa descrittiva.
3. Costruire la lista dai `TechnologyTierEffects` sul catalogo produttivo attuale. V15 omette la famiglia ritirata `food-store` e include esplicitamente `food-depot-large` in Alimentazione I e `food-depot-advanced` in Alimentazione II, usando l'icona `food-depot` già approvata.
4. Non mostrare sblocchi per tecnologie rinviate e non acquistabili; conservare il messaggio di indisponibilità e i prerequisiti. Non cambiare stato di sblocco, prezzi, motore, salvataggi o navigazione.

## Verifiche

Test del mapping degli sprite, compatibilità con tutte le tecnologie, dettagli Grande/Avanzato, rami rinviati, binding Avalonia nelle due finestre e refresh dopo il cambio di selezione. Gate RC90.3: **1167 test attesi**, non eseguiti localmente per assenza .NET 10. Baseline confermata RC89.1: 1135 test. Vedere [piano di test](../validation/RC90_3_TECHNOLOGY_ICON_ROWS.md).
