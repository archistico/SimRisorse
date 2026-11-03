# G1-D35 / RC20 — Audit di non-regressione della tabella Risorse

**Data:** 03/11/2026. **Stato:** candidata da verificare su Windows. **Ultimo gate confermato:** RC16, 476 test. RC17–RC19 non sono state testate dall'utente.

## Confronto statico v13 / v14

La v14 riusa `ResourceRow` della GUI classica e conserva lo stesso tracciato XAML di **sette colonne** (`*,80,80,108,96,85,108`) con `ListBox.gutter-list` e margine di 20 px dalla scrollbar. Sono presenti quantità intere in tabella con valore esatto nel tooltip, capacità e relativo calcolo, produzione, consumo, saldo e tempo stimato di riempimento. Le righe vengono aggiornate **in place**, per non invalidare la selezione corrente.

**Risultato:** nessuna regressione deterministica dimostrata dalla sola lettura del codice. Non cambiare larghezze, scrollbar, colori, stili né la GUI v13 sulla base di ipotesi; servono evidenze eseguibili e prova visiva.

## Lavoro RC20

Aggiunto `tests/SimRisorse.Desktop.Tests/V14ResourcesAuditTests.cs`, senza modifiche ai sorgenti di produzione:

1. Due casi Avalonia headless a **1040×740 e 1240×850** verificano sette colonne, sei celle numeriche allineate a destra, intestazione/riga entro lo spazio riservato alla scrollbar, selezione ocra, valori visibili, tooltip della capacità e selezione preservata dopo i tick.
2. Un caso sul ViewModel verifica le **17 risorse** v14, l'ordine alfabetico italiano, l'identità stabile delle istanze `ResourceRow`, la notifica delle variazioni e la coerenza del dettaglio di capienza delle Matite.

La copertura nuova comprende **3 casi xUnit**; totale atteso **490** a partire dai 487 di RC19. Nessuno dei nuovi test è stato eseguito in questo ambiente privo dell'SDK .NET 10. La copertura automatica non dimostra da sola la leggibilità grafica: lo screenshot e la prova con scrollbar devono essere confermati su Windows.

## Decisione di rilascio

La tabella Risorse **rimane identica a quella precedente**. Un eventuale difetto grafico rilevato dai test/screenshot dovrà essere corretto in una RC dedicata partendo da misure concrete e conservando gli altri moduli. È particolarmente importante validare RC17, che modifica la protezione dell'autosalvataggio.

**Prossimo sviluppo:** G1-D36 audit Edifici (+1/+5/+10, personale, prerequisiti, tooltip, diagnosi e scroll); la fase economica E4 resta sospesa fino alla chiusura P0.

[Roadmap](../ROADMAP.md) · [Matrice di parità](../validation/G1_D29_AUDIT_GUI_V13_V14.md) · [Piano Windows RC20](../validation/G1_D35_RC20_TEST_PLAN.md) · [Handoff](../NEW_CHAT_HANDOFF.md).
