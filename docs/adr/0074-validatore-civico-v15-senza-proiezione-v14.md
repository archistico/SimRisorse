# ADR 0074 — Validazione civica v15 autonoma dalla proiezione v14

**Data:** 05/11/2026  
**Stato:** Accettato come scelta architetturale; codice RC66 candidato a test .NET 10.

## Problema

La RC65 esegue comandi civici nativi v15, ma ogni validazione passa da `V15LegacyProducerAdapter.ForV14Rules` e `V14CampaignSnapshots.Validate`, che richiede 16 famiglie produttive e considera ancora lo storico `food-store`. È un vincolo artificiale e rischia di applicare la capienza storica anziché la capacità fisica della città v15.

## Decisione

Introdurre `V15CityCampaignValidator` in Core/Food. Il checkpoint v15 con 15 famiglie viene validato direttamente senza uno slot `food-store` fittizio. Rimangono obbligatori: controllo header, famiglie/tier, unlock ricerca, edifici e Municipio, contatori progressivi, risorse ammesse, capienza cittadina fisica, popolazione, militari, Scuola, crescita e lavoratori complessivi. L'audit delle scorte di Deposito e intermedi resta in `V15FoodCampaignCheckpoints.Validate`.

La proiezione temporanea a 16 famiglie sopravvive esclusivamente per compatibilità esplicita con vecchi dati e diagnostica (`ToLegacyPreview`), non nei comandi nuovi né nel percorso ordinario di validazione. Uno slot storico **occupato** può entrare soltanto in uno stato importato in attesa di ritiro esplicito; SQLite generation 6 continua a rifiutarne il salvataggio. Non cancellare automaticamente edifici o lavoratori.

## Limiti espliciti

La proprietà `CityCampaign` è ancora tipizzata `V14CampaignSnapshot` come DTO di persistenza e il contatore `food-store` può ancora comparire nella cronologia. Inoltre il clock fornisce localmente un produttore disabilitato al planner di catalogo v13. Questi tre punti richiedono un refactoring separato. **Non dichiarare conclusa tutta F1-D05e.2b.2b.2.**

## Regole inderogabili

Nessun file Desktop/XAML, GUI v14, database personale v14, formula economica, produzione o ricetta modificata. SQLite v15 resta generation 6. Nessuna incompatibilità aggiuntiva introdotta da questa RC.

## Verifica

RC63 ultimo gate confermato: 771 test. RC64 +6, RC65 +7, RC66 +7 = **791 test attesi**, non eseguiti qui. `\.\validate.cmd` su Windows; controllo di parità GUI v14/v15, audit v15 combinato opzionale.

[Specifica](../design/F1_D05E2B2B2A_RC66_VALIDATORE_NATIVO.md) · [Piano test](../validation/F1_D05E2B2B2A_RC66_TEST_PLAN.md) · [Roadmap](../ROADMAP.md).
