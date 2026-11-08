# ADR 0115 — RC104: preflight per il salvataggio giocabile unico

**Stato:** sperimentale, candidato a validazione. **Data:** 07/11/2026.

## Contesto

Il salvataggio normale v15 generation 6 contiene la città, ma non preventivi, versamenti e ricevute. Il database composito RC95–RC103 contiene città e finanziamento nello stesso file SQLite, mentre il registro di autorità è separato. Il caricatore giocabile rifiuta i compositi e i database con tabelle estranee. Una semplice aggiunta delle tabelle al file generation 6 non è una migrazione valida: `V15SqliteFoodSaveStore` controlla l'elenco esatto delle tabelle e `Save()` riscrive il file.

## Decisione RC104

Introdurre `E4bV15UnifiedSavePreflight` (Infrastructure.Experimental), esclusivamente in **lettura**, per valutare una futura migrazione:

1. Controllare tipo, versione, `PRAGMA integrity_check`, checkpoint nativo, deposito e sequenza delle ricevute.
2. Verificare `campaign_id` e, nei file registrati, l'associazione al **registro attivo**, non soltanto il numero di revisione. Un backup non promosso non può diventare per errore la fonte autorevole.
3. Segnalare l'assenza dei trigger obbligatori di fencing e il registro mancante, estraneo o di un'altra campagna.
4. Distinguere **ammissibilità preliminare alla migrazione futura** da **esportabilità verso generation 6**. Un preventivo aperto, anche senza versamenti, non è esportabile: mancherebbe la sua persistenza. I versamenti restano contabilizzati.
5. Non creare, modificare, promuovere, migrare o sovrascrivere alcun salvataggio. L'indicazione di ammissibilità non autorizza a costruire né certifica un formato nuovo già disponibile.

I file compositi non registrati e i vecchi senza identità possono ancora essere letti ed eventualmente esportati col ponte RC99 se non hanno cantieri pendenti, ma **non** sono considerati pronti per una migrazione autorevole.

## Passi successivi non compresi in RC104

- Progettare una **nuova generazione del formato giocabile** con tabelle proprie per identità, cantiere, rate, ricevute e revisione; migrazione esplicita con backup, file temporaneo, validazione, sostituzione atomica e recupero.
- Unificare la gestione dell'autorità **nel file giocabile** prima di utilizzare in partita i prezzi Curve A. La protezione cooperativa attuale non garantisce multi-host o difesa contro manomissioni intenzionali.
- Verificare bilanciamento naturale e sostenibilità della Zecca con squadre da 10 e magazzini reali.

**Invariati:** GUI v14/v15 approvata, salvataggi personali generation 6, prezzi correnti, Municipio con 100 Carbone e 2 Matite, unico `validate.cmd`, nessun manifest SHA-256.
