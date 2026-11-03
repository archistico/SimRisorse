# ADR 0027 — Sessione v14 isolata, transizioni atomiche Scuola e ricerca

**Stato: sperimentale (decisione tecnica di implementazione, non ratifica dei parametri gameplay)**  
**Data:** 3 novembre 2026

## Contesto

E1/E2/E3 ed E3b/1-2 forniscono regole pure, snapshot v14 e SQLite v3. Manca un aggregato che le applichi senza confondere il ruleset attuale `civil-growth-13` con quello candidato `civil-growth-14`.

## Decisione tecnica

Creare `V14CampaignSession`, distinta da `GameSession`, con stato autorevole esclusivamente `V14CampaignSnapshot`. Costruzione/ripristino validano il formato; comandi Scuola e ricerca sono transizioni con nuovo snapshot validato prima del commit; errore significa nessuna mutazione. Le regole proposte della Scuola sono oggetto `V14SchoolConstructionRules` iniettato. Gli acquisti tecnologia non avvengono durante l'accumulo dei punti, ma soltanto su comando.

## Vincoli

- `civil-growth-13`, SQLite v2, Desktop e vecchi benchmark restano byte per byte invariati.
- Lo store v3 resta separato e scrive soltanto su percorso esplicito distinto dalla sorgente v13.
- Nessun costo/timer proposto diventa definitivo per il solo fatto di essere esercitato nei test.
- E3b/3a-b **non** pretende di simulare il tempo: senza ciclo integrato non si può dichiarare la campagna v14 giocabile.

## Verifiche

Sette test nuovi scritti; il test Windows e le prove di replay richiedono il gate successivo. Vedere [dettaglio G1-D11](../design/G1_D11_E3B3_SESSION_ISOLATA.md).
