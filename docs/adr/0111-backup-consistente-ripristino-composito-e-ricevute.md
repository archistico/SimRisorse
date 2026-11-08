# ADR 0111 — Backup consistente e ripristino protetto del salvataggio composito

**Data:** 07/11/2026. **Stato:** sperimentale RC100, richiede validazione .NET 10/Windows.

## Contesto

RC95–RC99 mantengono già città v15, deposito materiali, revisione e ricevute **nello stesso contenitore SQLite sperimentale**. Il salvataggio giocabile ordinario usa tuttavia il formato generation 6 e non rappresenta i cantieri pendenti. Non è ancora autorizzato sostituirlo con il contenitore sperimentale. Il ponte RC99 può esportare soltanto quando non esiste un finanziamento aperto.

Il rischio più urgente è perdere un finanziamento pendente a causa di un backup fatto con una semplice copia byte-per-byte mentre SQLite sta scrivendo, oppure ripristinare un file senza verificare le ricevute.

## Decisione

1. Introdurre `E4bV15FundingCompositeRecovery` **solo in Infrastructure.Experimental**, con `BackupToNewFile(source, backup)` e `RestoreToNewFile(backup, destination)`. La sorgente è aperta in sola lettura e copiata tramite l'**online backup API SQLite**, ottenendo uno snapshot coerente senza copiare direttamente un file potenzialmente attivo.
2. Lo snapshot viene scritto in uno **staging nella cartella di destinazione**. Prima della pubblicazione si eseguono `integrity_check`, caricamento completo e riconciliazione dei fondi. Solo dopo si esegue `File.Move` **senza overwrite**. File originali e backup esistenti non vengono cancellati o sostituiti.
3. Il caricatore composito deve riconciliare la revisione e le ricevute: **un identificativo distinto per ciascuna revisione committata** da 1 a N. Ricevute mancanti, revisioni duplicate, buchi o testi vuoti determinano un rifiuto prima di accettare altri pagamenti. È un controllo strutturale, **non** una firma crittografica contro alterazioni intenzionali.
4. Il file ripristinato è un **nuovo ramo autonomo**, non una promozione automatica. Non usare contemporaneamente originale e clone come due autorità sulla stessa partita, né tentare di unire i rispettivi registri di pagamenti.
5. Non modificare l'ID o la versione del file, il normale SQLite generation 6, la GUI, i prezzi attivi o la curva A. Le nuove API non sono esposte al giocatore.

## Garanzie e limiti

La coerenza del backup si riferisce a un singolo istante SQLite e mantiene anche un cantiere ancora aperto. Una seconda operazione committata sulla sorgente **dopo** quel punto non appare nel backup. L'operatore deve scegliere quale file è autorevole prima di avviare una sessione recuperata. Restano necessari il protocollo di cambio autorità, il lock esclusivo e l'integrazione nel salvataggio giocabile prima dell'attivazione dei cantieri nella GUI.

## Evidenze e verifica

12 nuovi test Infrastructure in `Rc100CompositeRecoveryTests`: backup/restore vuoto, rate pendenti, replay dopo ripristino, Zecca completata, destinazioni occupate, origine=destinazione, formato nativo estraneo, percorsi mancanti, ricevute eliminate o revisioni alterate. Gate **1294 test cumulativi attesi**, ancora da eseguire. Ultima baseline confermata dall'utente: RC95, 1245 test.

**Comando unico Windows:** `./validate.cmd`. Niente file manifest o cartelle `.local-cache` negli archivi.
