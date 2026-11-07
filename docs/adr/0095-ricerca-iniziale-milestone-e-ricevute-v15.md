# ADR 0095 — Milestone Estrazione I e verifica delle ricevute reali

**Data:** 06/11/2026  
**Stato:** accettata per RC87 (implementazione candidata fino a test Windows).

## Problema

RC86.1 avvia una nuova v15 con 2 Matite, sufficienti alla prima iscrizione. Nel benchmark `V15-I01`, però, il goal `unlock-estrazione-1` richiedeva anche azioni successive: filiera del pane e Miniera di carbone, persino quando `Estrazione I` era già stata acquistata. Un run che non costruiva la Miniera poteva quindi restare `timeout` senza che questo provasse un blocco della ricerca.

## Decisione

1. `V15-I01 / research-first`: si ferma dopo i comandi necessari alla ricerca: fondazione alimentare e abitativa → Scuola → iscrizione → ricercatore → acquisto `Estrazione I`.
2. `V15-I01 / bread-first`: realizza deliberatamente la filiera del pane *prima* della Scuola; **non** richiede invece la Miniera di carbone dopo l'acquisto. Il costo delle due strategie è differente per scelta sperimentale, non per vantaggio gratuito.
3. `V15-I02` e `V15-I03` mantengono l'intera sequenza fino ai primi output industriali; `MintTen` resta opt-in.
4. Nuovo `V15ResearchBootstrapAudit`: usa le ricevute di `V15-I01` e l'inventario fisico reale della nuova campagna. Una milestone `pass` necessita di Scuola costruita, studente iscritto (2 Matite pagate), ricercatore assegnato, `Estrazione I` acquistata al costo effettivo (150 punti) e campione finale con ricerca sbloccata. Il risultato del benchmark non può autocertificarsi privo di queste evidenze.
5. Il run a 0 secondi è **diagnostico**, non costituisce prova di raggiungibilità; il confronto dei tempi è ammesso solo fra due milestone realmente osservate e non diagnostiche.
6. Nessuna modifica a motore, clock produttivo, prezzi, GUI, default di personale o salvataggi. Non riaccreditare Matite al caricamento.

## Precisazione sulle miniere

Nel catalogo corrente, `Estrazione I` è il prerequisito per i **tier Grande** delle miniere (e la progressione verso Avanzato). Il tier **Base** dipende dai prerequisiti edilizi, tra cui la Casa grande; il benchmark costruisce comunque la Miniera dopo l'acquisto della ricerca per mantenere l'ordine didattico scelto. Non modificare questi vincoli senza una decisione di game design separata. Un eventuale messaggio UI che suggerisca un requisito tecnologico anche sul tier Base va verificato rispetto al tier selezionato.

## Verifica

- xUnit: piano [RC87_RESEARCH_BOOTSTRAP_TEST_PLAN](../validation/RC87_RESEARCH_BOOTSTRAP_TEST_PLAN.md).
- Windows: `./validate-f1.cmd` (1085 test previsti), con comando CLI diagnostico a 0 secondi; i run integrali sono separati e facoltativi per non allungare inutilmente il gate.
- L'analisi deve rifiutare spese scorrette, eventi duplicati, milestone fabricate e partite con inventario iniziale alterato.
