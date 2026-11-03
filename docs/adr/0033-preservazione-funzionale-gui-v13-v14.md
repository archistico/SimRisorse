# ADR 0033 — Le funzioni della GUI v13 sono una baseline vincolante per v14

- **Data:** 2026-11-03
- **Stato:** Approvato come requisito di compatibilità UI; implementazione RC3 candidata alla verifica Windows
- **Sostituisce le affermazioni non verificate di parità presenti nel D17/RC2.**

## Problema

La GUI sperimentale v14 è stata ricostruita indipendentemente e ha perso funzioni già presenti nel gioco: la scheda Risorse è passata da sette a tre colonne, l'Esercito è diventato un riepilogo in sola lettura, mancano dettagli di crescita e diagnosi, e veniva mostrato «Costruisci» anche per edifici non sbloccati. Questa è una regressione non autorizzata.

## Decisione

1. **Non sostituire/semplificare alcuna sezione esistente** durante l'introduzione di Scuola, tecnologie e livelli. La UI v13 è il riferimento visivo e funzionale.
2. La scheda Risorse conserva in ordine: Risorsa, Quantità arrotondata, Capacità con tooltip del contributo, Produzione/s, Consumo/s, Saldo/s, Si riempie tra. Riusare `ResourceRow` e le ricette della stessa simulazione, non un DTO sintetico da tre colonne.
3. Le sezioni militare, popolazione, evidenze e azioni di partita devono conservare tutte le azioni applicabili al ruleset; in caso di differenza concreta documentarla, non nasconderla.
4. Edifici di livelli diversi sono **righe distinte**, senza ComboBox nei dettagli. «Costruisci» visibile solo con prerequisiti soddisfatti; disabilitato in assenza di materiali. Se bloccato, mostrare costi e causa precisa. Potenziamenti espliciti, mai acquisti mascherati.
5. Nessuna modifica a `MainWindow`, al salvataggio v13 o al ruleset `civil-growth-13`. Fino a validazione, `--experimental-v14` e SQLite v3 restano separati.
6. L'approvazione dei test non approva costi +15%, parametri scuola o bilanciamento.

## Verifica

Test di regressione per sette colonne, celle/tooltip, blocchi e pulsanti, coda militare, progressione e salvataggio. Un audit visivo manuale Windows resta obbligatorio: test headless e XML non garantiscono correttezza dei pixel, ingombri e hover.

## Debito tecnico dichiarato

La presentazione v14 continua a usare un XAML separato, pur riutilizzando il DTO `ResourceRow` e copiando letteralmente i layout delle schede storiche. Dopo il gate RC3 valutare l'estrazione di componenti riutilizzabili senza rompere i test della vecchia `MainWindow`; non considerare «parità completa» finché non saranno verificati anche gli scenari Pane avanzati, notifiche e disposizione grafica su Windows.
