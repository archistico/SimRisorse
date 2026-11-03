# G1-D15 — E3b/3e: edifici civili e replay SQLite v14

**Data:** 03/11/2026. **Gate precedente confermato:** E3b3-RC4, `validate.cmd` con 408/408 test secondo l'utente. **Questa consegna:** RC5 candidata, non compilata nel container.

## Obiettivo implementato

La sessione `V14CampaignSession`, già distinta da `GameSession`, dispone di `BuildCivicBuilding(buildingId, quantity)` per edifici di catalogo non produttivi: `small-house`, `large-house`, `warehouse`, `barracks`. Restano comandi separati per Scuola, produttori ed eventuali futuri tipi; il Municipio è fisso e non acquistabile.

### Contratto del comando

- `quantity` positiva; ID del catalogo v13 conosciuto, non fisso, senza ricetta e non appartenente alle 16 famiglie produttive.
- I prerequisiti devono essere **già posseduti** prima della transazione; non si considerano acquisti impliciti.
- Il prezzo per ogni nuova struttura utilizza il **contatore storico della medesima famiglia**, con `ProgressiveConstructionPricing.QuoteNewBuildings` e coefficiente candidato comune iniettabile da `V14TierConstructionRules` (default 1,15).
- Si controllano prima i limiti di conteggio, alloggi e spazio condiviso, poi le risorse; su fallimento saldo, edifici e contatori restano identici (stessa istanza snapshot).
- Una Casa aggiunge **capienza** per gli abitanti ma non genera popolazione istantanea; il Magazzino contribuisce +500 unità per risorsa nello stesso inventario condiviso, senza magazzini separati.
- Le Caserme usano le regole militari v13 esistenti: il comando non crea soldati, non anticipa addestramento e non assegna residenti.
- I contatori storici del municipio non sono presenti perché è un edificio scenario-fixed. Gli upgrade dei produttori **non incrementano** il contatore.
- L'integrazione resta isolata dalla partita `civil-growth-13`, dai salvataggi v2 e dalla GUI corrente.

### Esempio, ancora sperimentale

Casa piccola: costo storico di catalogo 80 Legno lavorato + 40 Pietra; una Casa piccola già posseduta alla migrazione. Con +15% candidato, le prossime due costano **92/46** e **106/53**. Un ordine di due Case vale **198/99**, uguale alla somma degli ordini individuali riusciti. Nessun parametro numerico nuovo viene ratificato con questo passo.

## Replay deterministico con checkpoint

Lo scenario di prova usa un dataset artificiale valido, senza file privati: riserve di materiali e un saldo di ricerca precedentemente accumulato. Esegue le medesime operazioni su due sessioni:

1. Costruzione Casa, Magazzino, Pescatore e Deposito alimentare; costruzione Scuola; iscrizione studente.
2. Avanzamento 120 secondi, completamento formazione, assegnazione di un ricercatore, avanzamento di altri 9 secondi; accumulo del saldo di ricerca senza acquisto automatico.
3. Sessione A prosegue direttamente; sessione B viene salvata in SQLite v3, ricaricata inizialmente in pausa e poi ripresa.
4. Acquisto volontario di Lavorazioni I, costruzione Segheria grande, assegnazione lavoratori, upgrade Segheria base→grande e seconda nuova Casa.
5. Sessione A avanza 45 secondi, B 3×15 secondi. Verifica **uguaglianza completa dei campi persistiti**: regole, tempo, risorse, popolazione, altri edifici, famiglie, contatori, avanzamento crescita, militari, Scuola, saldo ricerca e tecnologie.
6. Secondo salvataggio v3, rilettura ed equivalenza. Test separato: progressione del prezzo della Casa dopo riavvio, con errori innocui in mezzo.

Questa milestone **non implementa un registro persistente degli eventi** (event sourcing): «replay» significa rigiocare una sequenza identica di comandi in test e confrontarne gli stati, anche dopo un checkpoint. Gli errori vengono verificati come transazioni senza commit.

## Limiti e gate

- I prezzi 1,15 e 2,6/6,5, costi Scuola e soglie di ricerca rimangono candidati.
- Il formato v14/SQLite v3 non cambia; nessun database personale viene scritto.
- Il test Windows `validate.cmd` sulla RC5 è obbligatorio prima di dichiarare verde questo incremento.
- **Prossimo:** E3b/4 UI sperimentale di Scuola, Ricerca, livelli, costruzione e potenziamenti; poi E4 bilanciamento/anti-softlock/attivazione con rollback.
