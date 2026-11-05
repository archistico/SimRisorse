# ADR 0061 — Edifici alimentari direttamente costruibili e capienze derivate

**5 novembre 2026 · Accettata per RC50 candidata · F1-D05d.3a**

## Contesto

RC49 conserva i conteggi di sei nuovi edifici nel checkpoint, ma non li rende acquistabili, non espone costi o prerequisiti e accetta capienze fornite esplicitamente a un planner. La GUI v14 usa ancora `food-store` che produce direttamente Cibo da Pesce. L'utente ha autorizzato l'abbandono della compatibilità con vecchi database di sviluppo.

## Decisione

1. Un catalogo v15 con sei identificativi canonici, uno per edificio base, semantica di **costruzione diretta**, costi crescenti del 15% per famiglia e prerequisiti validati come grafo. Prezzi, capienze e prerequisiti sono temporanei finché E4a/E4b non stabiliscono il bilanciamento.
2. I posti sono ottenuti dal numero di edifici. Lo staff è modificabile solo dalla sessione, non può superare posti né popolazione disponibile; industria, scuola e militari condividono lo stesso budget.
3. Le capacità degli inventari animal/transit/depot sono derivate dai sei conteggi; il salvataggio **non** persiste una seconda copia di esse. Una scorta già eccedente non viene mai troncata durante una costruzione.
4. Il vecchio `food-store` è **escluso** dal nuovo catalogo acquistabile. Prima dell'attivazione della GUI v15, il clock e il vecchio catalogo devono essere sostituiti/riconciliati: non si devono usare entrambe le ricette.
5. Nessuna migrazione automatica fra vecchi database è requisito di RC50. Rimane lo schema SQLite 5 di RC49, perché i campi necessari ci sono già.

## Conseguenze

- RC50 può essere verificata con una sessione di simulazione isolata, test di costruzione/staffing/tick e round-trip SQLite, senza regressioni intenzionali della GUI v14.
- RC50 non è giocabile tramite `run-game.cmd`. D05d.3b deve fare un clock unico e rimuovere la doppia ownership del Cibo; D05d.4 porta le azioni nella GUI pixel art originale.
- Non modificare il bilanciamento della Zecca o la progressione quasi esponenziale prima di E4a/E4b.

[Dettagli](../design/F1_D05D3A_RC50_COSTRUZIONI_FILIERA.md) · [Test](../validation/F1_D05D3A_RC50_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
