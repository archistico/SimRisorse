# 0011 — Pane separato e razione degli abitanti avanzati

- Stato: **Accettato** dall'utente
- Data: 2026-10-27
- Ambito: quarto incremento M4, regole `civil-growth-4`
- Implementazione: presente nel codice, **build, test e render Windows ancora da eseguire**

## Contesto

In `civil-growth-3` il Forno produce lo stesso Cibo del Deposito alimentare. Un solo Pescatore + Deposito produce 1 Cibo/s e alimenta fino a 100 persone al consumo di 0,010/s. Il pane, quindi, non è indispensabile per un normale ampliamento abitativo; richiedere soltanto un Forno costruito per sbloccare altre case non ne richiederebbe il funzionamento.

La [valutazione quantitativa](../scenarios/ADVANCED_HOUSING_PROPOSAL.md), poi approvata dall'utente, confronta questo prerequisito nominale con una risorsa Pane distinta. Si sceglie la seconda soluzione per creare una decisione economica verificabile.

## Decisione

- Nuova `large-house` da 32 posti, costo 160 legno lavorato + 80 pietra, senza lavoratori, con prerequisito Forno costruito.
- Nuova risorsa `bread` / Pane, scorta iniziale zero e capacità iniziale 100; il Magazzino aggiunge 500 anche a questa risorsa.
- Il Forno consuma 1 Farina + 1 Acqua e produce 3 Pane/s, non più 3 Cibo/s. La filiera del pesce resta l'unica produzione di Cibo base in M4.
- Tutti gli abitanti consumano 0,010 Cibo/s; gli abitanti che superano i posti delle Case piccole richiedono inoltre 0,015 Pane/s. `abitanti_avanzati = max(0, popolazione − 8 × case_piccole)`.
- Il nuovo abitante può arrivare solo se la scorta, calcolata all'inizio del passo, copre anche la razione del nuovo residente nel secondo successivo. Il ritmo resta uno ogni 60 secondi validi.
- La mancanza di Pane blocca gli arrivi senza eliminare persone; il Cibo continua a essere consumato se sufficiente. Il Pane è prenotato prima delle ricette, come il Cibo; i consumi rimangono esatti e senza frazioni di una razione collettiva.
- I residenti di case piccole/grandi non sono stati persistiti separatamente: si derivano da popolazione e quantità di Case piccole. Costruire ulteriori Case piccole può ridurre la quota di residenti avanzati.

Le regole avanzate sono dati espliciti (`AdvancedHousingRules`) nel Core indipendente dalla UI; la variante Economy Foundation rimane senza alimentazione civile. UI e simulatore leggono il piano unico del Core.

## Versione dei contenuti e compatibilità

Lo scenario diventa `civil-growth-4`. È dichiarato un passo `-3`→`-4` che aggiunge Casa grande a zero e introduce Pane a zero, senza inventare quanta parte dello stock di Cibo precedente fosse stata prodotta dal Forno. Tutti i Cibi già salvati rimangono Cibo.

La conversione entra nella catena `-1`→`-2`→`-3`→`-4`, usa lo stesso scenario e non converte regole sconosciute. Vengono rifiutati checkpoint che si dichiarano `-3` ma contengono già Pane nonzero o Casa grande. La copia di sicurezza al primo salvataggio è quella prevista dall'[ADR 0010](0010-storage-capacity-and-rules-conversion.md).

Il formato snapshot/file resta v2: il vettore delle risorse è sparso, quindi una nuova risorsa assente in un vecchio checkpoint vale zero; la nuova Casa grande è una riga in `building_stock`. Devono comunque passare il round-trip SQLite e i test di invalidità prima della promozione della milestone.

## Alternative e trade-off

- Solo prerequisito Forno: poco costoso da implementare, ma senza un vero consumo di Pane.
- Richiedere un Forno in produzione nel momento della crescita: lega la crescita alla capacità libera del Cibo condiviso, con falsi blocchi quando il Cibo è pieno.
- Pane separato: richiede una nuova risorsa e nuovi test, ma rende chiara la relazione fra case grandi, consumi e blocchi.

La Casa grande costa meno materiali per posto, ma la filiera alimentare introduce un costo d'ingresso e impegna cinque lavoratori. Il pareggio *dei soli materiali da costruzione* arriva alla quarta Casa grande se basta una filiera Pane. Non è un pareggio garantito dei tempi o dei lavoratori.

## Gate

Prima di considerare completato questo incremento:

1. Build .NET 10 senza errori o warning; tutti i test della soluzione superati, compresi Core, Application, SQLite e Desktop.
2. Cronologie `bread` e `advanced` verificate, insieme agli invarianti di risorse nonnegative e entro la capacità a 8/32 ore.
3. Conversioni da `-1`, `-2`, `-3` con scorte e tempo conservati; copia di sicurezza e rifiuto di salvataggi incompatibili.
4. Render Avalonia alle due dimensioni previste, con Pane, fabbisogni, elenco edifici e controlli principali visibili.
5. Successivo benchmark di tre strategie (case piccole, case grandi, investimento accelerato), distinto dai test funzionali.

Questo ADR non decide il sistema militare, la demolizione, la vittoria, una classifica o il bilanciamento delle otto ore dello scenario completo.
