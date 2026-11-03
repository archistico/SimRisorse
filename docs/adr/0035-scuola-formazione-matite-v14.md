# ADR 0035 — Scuola con progresso visibile e Matite nella v14

- **Data:** 2026-11-03
- **Stato:** decisione UX e requisito di due Matite per studente richiesti dall’utente; parametri di ricetta/capienza candidati da verificare
- **Ambito:** modalità sperimentale `civil-growth-14`, senza modifiche al catalogo ufficiale `civil-growth-13`
- **Integra:** [ADR 0034](0034-v14-stati-edifici-e-layout-azioni.md)

## Contesto

Il giocatore richiede una schermata Scuola analoga a Esercito, con progresso dell’addestramento, numero di studenti in coda, in formazione e ricercatori qualificati; deve inoltre essere possibile fabbricare Matite da Legno e Carbone e consumarne **almeno due a studente**. Rimuovere l’azione visibile «Esci senza salvare» dalle due modalità.

## Decisione

1. La scheda Scuola espone coda, formazione parallela per i posti disponibili, barra del **primo completamento**, durata residua, ricercatori qualificati/attivi, capacità e istruzioni. I progressi erano già persistiti e gestiti dal motore v14 (120 secondi per studente) e non vengono azzerati.
2. **La Scuola si costruisce solo dalla scheda Edifici.** Nella scheda Scuola restano i comandi `Iscrivi`, `Annulla`, `Assegna`, `Sospendi`, `Torna civile` e il comando per fabbricare Matite: nessun pulsante di costruzione.
3. In v14 compare la risorsa **Matite**, visibile nella tabella Risorse classica a sette colonne. Ricetta iniziale candidata del laboratorio Scuola: **1 Legno lavorato (`planks`) + 1 Carbone (`coal`) → 4 Matite**; un edificio Scuola è richiesto. La fabbricazione è esplicita, non un consumo o una produzione automatica a ogni tick.
4. L’iscrizione v14 consuma **10 Cibo + 2 Matite per studente**. Gli studenti occupano popolazione, i ricercatori qualificati non sono automaticamente assegnati e la ricerca si accumula liberamente.
5. Lo stoccaggio delle Matite nel solo ramo v14 deriva dal Municipio (+1.000), Magazzino (+500) e Scuola (+50); rimane uniforme per il Magazzino, senza introdurre eccezioni nel pannello Risorse. I coefficienti di produzione e capacità sono *candidati*, non approvati come bilanciamento E4.
6. Cancellare un’iscrizione rimborsa Cibo e **soltanto le Matite effettivamente pagate**, purché vi sia spazio. I vecchi salvataggi SQLite v3 non contengono tale informazione: studenti già iscritti prima della RC9 sono conservati come studenti *legacy*, con diritto al rimborso Cibo ma non a Matite che non avevano pagato. Un campo opzionale `pencil_paid_students` consente la lettura dei file precedenti senza alterare il formato v3.
7. La v13 resta dotata di catalogo a 16 risorse e salvataggi SQLite v2; la v14 mostra 17 risorse. Il pulsante «Esci senza salvare» viene tolto dalle due UI ma resta la chiusura normale della finestra e il metodo tecnico per i test.

## Verifica e rischi

- Test automatici per consumo, capienza, produzione atomica, avanzamento, rimborsi, salvataggi nuovi e precedenti, struttura UI e assenza del pulsante. Test di integrazione Windows e audit grafico ancora da eseguire.
- Le modifiche RC9 toccano il motore e il salvataggio *sperimentali*: **non** dichiarare invariato l'intero Core o lo schema sperimentale. Testare copy-on-write e backup, in particolare con salvataggi RC8.
- Resta da decidere in E4 se mantenere il laboratorio manuale o introdurre un edificio dedicato, e se cambiare coefficienti produttivi/capacità. Il requisito **2 Matite per studente** è invece confermato.

[Rapporto tecnico RC9](../design/G1_D25_E3B4_RC9_SCUOLA_MATITE.md) · [Piano RC9](../validation/G1_D25_RC9_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
