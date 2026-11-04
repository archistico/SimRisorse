# ADR 0053 — Filiera Ortofrutta con edifici separati

Data: 2026-11-04. Stato: direzione approvata, modello Core isolato in RC42; catalogo giocabile futuro.

## Decisione

Due filiere corte distinte, senza combobox: **Orto → Verdura (`vegetables`)** e **Frutteto → Frutta (`fruit`)**. Lavoratori e capacità separati, resa per lotto configurabile, eventuale uso di Acqua parametrico e priorità deterministica in caso di contesa della risorsa. Nessuna conversione automatica in Cibo: occorrono ricezione e lavorazione nel Deposito alimentare.

## Conseguenze

Il Core può testare la logica senza alterare i salvataggi e il bilanciamento correnti. F1-D04 integra il deposito; F1-D05 registra risorse, edifici, tier, GUI e migrazione SQLite. [Rapporto](../design/F1_D03_RC42_ORTOFRUTTA_E_BACKUP_V14.md).
