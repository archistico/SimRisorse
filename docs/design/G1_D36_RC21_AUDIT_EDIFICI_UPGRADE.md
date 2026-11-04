# G1-D36 / RC21 — Audit Edifici: stato visivo dei potenziamenti

**Baseline verificata dall’utente:** RC20, 490 test attesi e dichiarati superati. **Candidata RC21:** 493 test attesi (+3 casi Desktop: 2 viewport e 1 ripristino/acquisto). Questa analisi è statica, non una certificazione estetica Windows.

## Difetto riproducibile dal codice — D36-01 (UI-10/UI-12/UI-18)

Nella RC20 i pulsanti `V14UpgradeNext` e `V14UpgradeAdvanced` erano collegati esclusivamente a `ICommand.CanExecute`, ma **non** avevano `IsEnabled` esplicito né lo stile `primary build` adottato dagli ordini Costruisci. Per un potenziamento tecnologicamente sbloccato ma non pagabile il comando era già inibito; mancava invece la protezione dello stato visivo coerente grigio/ocra, già adottata in RC8/RC12 per altri pulsanti. Nessuna prova che il comando eseguisse un acquisto improprio.

## Correzione mirata

- Nella sola GUI `V14PreviewWindow.axaml`, associare `V14UpgradeNext.IsEnabled` a `CanUpgradeNext` e `V14UpgradeAdvanced.IsEnabled` a `CanUpgradeDirectlyToAdvanced`.
- Applicare a entrambi `Classes="primary build"` e allungamento orizzontale coerente con lo stile degli ordini costruzione: **verde se acquistabile, grigio se no, hover ocra soltanto da abilitato**.
- Non modificare `ShowUpgradeNext`/`ShowUpgradeDirectlyToAdvanced`: i pulsanti già sbloccati restano visibili se mancano materiali; costi e carenze restano consultabili.
- Nessuna variazione a `V14CampaignSession`, prezzi progressivi, prerequisiti, unità di produzione, SQLite, XAML classico v13 o salvataggi.

## Test aggiunti

`tests/SimRisorse.Desktop.Tests/V14BuildingUpgradeAuditTests.cs`:

1. Viewport **1040×740**: entrambi gli upgrade sbloccati ma senza risorse mostrano lo stato visivamente disabilitato e il preventivo; il mouse non riattiva il colore né sposta il pulsante; il comando non muta la colonia.
2. Viewport **1240×850**: stesso controllo geometrico/visivo.
3. Disponibilità dinamica: si carica un checkpoint con **esattamente** il costo calcolato dal pricing Core; l'upgrade diventa verde, mantiene dimensione/posizione in hover ocra, realizza una sola conversione Base→Grande e torna disabilitato senza cambiare la riga selezionata.

## Audit ancora aperto

Le altre voci Edifici sono protette da test precedenti (righe Base/Grande/Avanzato, prerequisiti grigi ma selezionabili, costo lotti +5/+10, righe Costruisci/Lavoratore, diagnosi produzione). **Non** sono però certificate manualmente: serve confronto Windows a 1040×740 e 1240×850, inclusi scrolling del box, testo nelle etichette, cambio scheda e hover reale. Verificare anche la protezione del checkpoint RC17 su copia del salvataggio.

[Validazione RC21](../validation/G1_D36_RC21_TEST_PLAN.md) · [Matrice completa](../validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Roadmap](../ROADMAP.md).
