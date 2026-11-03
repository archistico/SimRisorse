# Roadmap — SimRisorse (3 novembre 2026)

La progressione viene misurata per milestone, con **test confermati** distinti da test soltanto scritti.

| Fase | Stato |
|---|---|
| v59 / `civil-growth-13` | Giocabile; regressioni preservate |
| E1 · Prezzi progressivi per famiglia | Implementata Core, validata tramite gate RC5 |
| E2 · Tre livelli e upgrade | Implementata Core, validata tramite gate RC5 |
| E3a · Scuola, ricercatori, punti e tecnologie | Implementata Core, validata tramite gate RC5 |
| E3b/1 · Snapshot v14 e migrazione in memoria | Validata |
| E3b/2 · SQLite v3 con backup e protezioni | Validata |
| E3b/3a-e · Sessione v14, orologio unico, costruzioni e replay | **RC5 validata (417 test)** |
| **E3b/4a · Finestra desktop v14 opt-in** | **RC1 validata, 427 test** |
| E3b/4b · Primo riallestimento classico RC2 | **Non approvato: regressioni risorse/edifici/esercito** |
| **E3b/4c · Preservazione GUI e funzioni v13, RC3** | **Candidata: 439 test attesi e audit grafico da eseguire** |
| E3b/4d · Parità definitiva Pane, usabilità e componenti condivisi | Da fare dopo il gate RC3 |
| E4 · Audit anti-blocco, simulazioni lunghe, salvataggi reali | Da fare |
| E4 · Bilanciamento, approvazione quantità e attivazione v14 | Decision gate; non anticipare |
| M5.4 · Cantiere e flotta | Dopo nuova economia |
| M5.5-M5.7 · Esplorazione, conquista, Castello | Da pianificare dopo M5.4 |
| M6/M7 | Successivamente |

## Invarianti

- `civil-growth-13` e il suo DB v2 non vengono mai scritti dal ramo v14.
- Municipio unico; Pane e altre risorse condividono il sistema di stoccaggio; i produttori conservano le rispettive capacità.
- Ogni persona può occupare un solo ruolo: lavoratore, recluta/soldato o studente/ricercatore.
- Punti ricerca: accumulo libero, acquisto volontario; le tecnologie non implementate non sono acquistabili.
- Parametri di costo/ricerca ancora sperimentali, non approvati dalla semplice conferma dei test.
- La GUI pixel art v14 è **opt-in e su DB separato**, ma adotta la **stessa composizione visiva** della GUI storica; i livelli produttivi sono righe edificio distinte, non ComboBox. La GUI normale v13 rimane invariata.

[Stato](../../CURRENT_STATE.md) · [guida](../../START_HERE.md) · [design D18](../../design/G1_D18_E3B4_RC3_RIPRISTINO_FUNZIONI_GUI.md) · [storia roadmap RC5](ROADMAP_RC5.md).
