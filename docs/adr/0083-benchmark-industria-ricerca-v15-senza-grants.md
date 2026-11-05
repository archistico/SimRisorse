# ADR 0083 — Benchmark avanzati naturali v15 con politiche comparabili

**Stato:** accettato per l'implementazione RC75 (non equivale a validazione .NET 10)

## Decisione

Mantenere i benchmark avanzati della Scuola, della ricerca, della Forgia e della Zecca separati dai runner v14 e da quelli alimentari RC73, con avvio naturale v15 e nessun grant. Raccogliere i materiali realmente addebitati da ciascun comando e i punti ricerca effettivamente spesi separatamente. Confrontare due ordini espliciti di sviluppo (`research-first`, `bread-first`) con lo stesso stato iniziale **e le stesse azioni necessarie**. La Scuola può essere costruita prima, ma nessuna delle due politiche può iscrivere ricercatori senza i 2 pezzi di Matite richiesti per studente dalla GUI v15: servono Pane/Casa grande/Miniera di carbone per fabbricarle. Etichettare sempre i run abbreviati come diagnostici.

Il confronto quantitativo dei tempi richiede che entrambe le politiche abbiano raggiunto la milestone nel rispettivo orizzonte pieno; gli altri casi non producono un delta temporale. I timeout e i blocchi di una politica non costituiscono prova di impossibilità del gioco.

## Ragioni

La RC74 poteva analizzare solo planks/stone dei primi tre scenari alimentari. Per decidere futuri costi di Forgia/Zecca serve registrare anche spese per altri materiali, Scuola e ricerca, senza confondere consumi continui con spese una tantum. Le politiche differiscono nell'ordine, non per risorse iniziali donate.

## Conseguenze e limiti

- Codice nuovo soltanto in Application e strumenti, senza modifiche a GUI, Core o SQLite.
- I tre scenari e le due politiche rimangono `not-run` nell'archivio; nessun dato empirico presentato prima di una run completa verificata.
- Il lavoro successivo E4a.2c dovrà quantificare anche i costi indiretti, la competizione sul Carbone, la capacità di magazzino e politiche di recupero.
- I costi quasi esponenziali e le squadre minime della Zecca restano E4b, da approvare dopo misure, non sono parte di questa ADR.

[Specifica RC75](../design/E4A_V15_RC75_INDUSTRIA_RICERCA.md) · [Test](../validation/E4A_V15_RC75_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
