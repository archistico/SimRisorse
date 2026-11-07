# ADR 0096 — Audit della filiera industriale e ricevuta di produzione

**Data:** 06/11/2026  
**Stato:** accettata per la RC88 candidata (validazione Windows ancora necessaria).

## Problema

I benchmark `V15-I02` e `V15-I03` richiedono l'ordine delle azioni della ricerca e della filiera industriale, ma una semplice stringa `pass` o una milestone inserita in un JSON non bastano a dimostrare il primo output reale. La RC87 copre il primo acquisto di `Estrazione I`, non la filiera Spade/Monete. La mancanza di output alla scadenza non basta a dimostrare un ciclo di prerequisiti.

## Decisione

1. Il clock del benchmark allega a ogni **milestone produttiva** la quantità positiva ottenuta in quel tick tramite `FirstSecondProduction` (`ProducedMilliunits`). Non cambiare clock o catalogo della partita.
2. `V15IndustrialChainAudit` controlla **sorgente del salvataggio, inventario fisico iniziale, regime delle squadre, strategia, ordine dei comandi**, costi reali di iscrizione e materiali per costruzioni, punti ricerca pagati per `Estrazione I` (150) e `Industria I` (200), milestone del clock e stock finale delle Spade/Monete. Un `pass` non sostenuto da questi dati è rifiutato.
3. Analizza separatamente **2 strategie Forgia Legacy**, **2 strategie Zecca Legacy** e **3 strategie Zecca MintTen** (compresa `housing-buffer`); non sommare indebitamente le squadre fra i tier.
4. Confronti temporali ammessi **solo con due risultati verificati e non diagnostici**. A 0 secondi il gate registra sette diagnostiche, non tempi di completamento.
5. CLI `--audit-v15-industry-chain`: senza parametri esegue la diagnostica a 0 secondi; `--max-seconds` permette un orizzonte esplicito; `--input` analizza report già salvati senza far avanzare il gioco. Export JSON/CSV/Markdown in una cartella che non contenga già i file generati.
6. Ricerca e costruzioni Base hanno regole distinte: `Estrazione I` precede la Miniera **nel benchmark**, ma nel catalogo la Miniera Base resta legata ai prerequisiti edilizi; `Estrazione I` è richiesta dai livelli successivi. Non alterare il gameplay su questa base.
7. Non modificare il default `Legacy`, prezzi, GUI canonica, salvataggi, inventari o filiere. La curva A resta non approvata.

## Nota attuativa RC88.1

`ResourceBundle` rappresenta stock fisico come mappa sparsa, con risorse assenti interpretate come zero. Nella prima RC88 l'audit accedeva direttamente a `.Amounts["swords"]`/`["coins"]`, provocando `KeyNotFoundException` su un inventario iniziale valido. RC88.1 usa l'indicizzatore del bundle, senza modificare il contratto delle ricevute o i requisiti di approvazione.

## Limiti

Il report valida la **coerenza interna delle ricevute registrate**. Non certifica in modo crittografico che un file esterno non sia stato manipolato e non dimostra la raggiungibilità globale per tutte le strategie. Le esecuzioni naturali integrali possono terminare per timeout; vanno conservate senza etichettarle come softlock definitivi.

## Gate

RC86.1 validata: 1067 test. RC87 +18 e RC88 +18; **RC88 eseguita su Windows con 1103 test e 16 fallimenti** dovuti a una singola lettura errata di stock sparso. **RC88.1**: +2 casi di regressione, **1105 cumulativi attesi**, ancora da validare. Per verificare: `./validate-f1.cmd`; comando diagnostico RC88 nel gate; run naturali lunghi facoltativi separati. Dettagli in [piano di validazione RC88](../validation/RC88_INDUSTRIAL_CHAIN_TEST_PLAN.md).
