# Piano di validazione RC90.3 — Icone delle tecnologie e degli edifici sbloccati

**Candidata, 06/11/2026.** Partenza: RC90.2, con **1157 test attesi** ma nessuna conferma completa dell'utente. RC90.3 aggiunge **10 casi xUnit** (6 combinazioni di ricerca, 2 verifiche dei dati e 2 verifiche UI Avalonia Headless): **1167 test cumulativi attesi**.

## Controlli automatici

- `TechnologyPresentation.GlyphForTechnology`: sprite definiti per sei rami tecnologici; icone valide.
- Liste generate da `TechnologyTierEffects`: solo livelli reali del catalogo; alimento Grande/Avanzato include il Deposito corrispondente e il medesimo pittogramma. Nessun edificio fittizio `food-store` nella v15.
- Tecnologie non implementate: nessuna promessa di sblocco, ma riga e icona tecnologia visibili.
- Cambiare selezione nel ViewModel aggiorna il riepilogo e la lista iconografica v14/v15.
- Window headless: elementi `ListBox` e `ItemsControl` mostrano i `PixelGlyph` e le righe corrette; prerequisiti preservati. Adeguati i precedenti test RC90 che verificavano la descrizione testuale ormai rimossa.

## Controllo manuale Windows

1. ` .\validate-f1.cmd`: aspettarsi 1167 passati, zero falliti, zero ignorati. xUnit analyzer e compiler warnings sono errori.
2. Avviare v15, scheda Albero: ogni tecnologia ha icona vicino al nome. Selezionare Alimentazione I e II: «SBLOCCA EDIFICI» deve includere Deposito alimentare Grande/Avanzato con icona. Cambiare da Grande ad Avanzato e poi a Logistica I: la lista deve aggiornarsi e scomparire quando non ci sono sblocchi.
3. Selezionare Estrazione I/II, Industria I/II, Lavorazioni I/II: icone dei veri edifici e livelli corretti. Navigazione ha icona nave, ma non edifici promessi perché rinviata.
4. Ripetere nella vista v14; verificare 1040×740 e 1240×850, scroll del dettaglio, selezione degli elementi e nessuna modifica ai sette campi di Risorse, tooltip, costi di costruzione o pulsanti.
5. Verificare la persistenza della RC90.2 (vecchi salvataggi 6 righe, nuovi 8 righe) e il fixture corretto senza introdurre concessioni sulle capacità.

**Non validato localmente:** SDK .NET 10 non disponibile in questo ambiente; controlli XML, documentazione e integrità ZIP non sostituiscono xUnit.
