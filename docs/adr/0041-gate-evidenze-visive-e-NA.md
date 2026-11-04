# ADR 0041 — Evidenze P0 verificabili e gestione delle eccezioni NA

**Stato:** approvata come regola operativa degli strumenti di audit (G1-D44, RC30) il 04/11/2026. **Non equivale ad approvazione visiva del gioco.**

## Contesto

Il registro G1-D43 confronta 22 aspetti della GUI in due viewport. Verificare soltanto la presenza dei nomi PNG permette falsi positivi; inoltre 44 righe `NA` possono essere interpretate come complete.

## Decisione

1. Il report non conterà `PASS` una riga senza dati di test e **PNG realmente decodificabili** e coerenti per dimensioni con la viewport.
2. `NA` resta registrabile e motivabile, ma non verrà equiparato automaticamente a un risultato positivo. La non applicabilità della **v13** nelle pagine nuove non rende automaticamente non applicabile il test della **v14**.
3. `Verify` potrà certificare soltanto la **completezza formale** delle 44 prove, non la qualità della GUI, l'autenticità della cattura o il gate P0.
4. Le condizioni per promuovere la fase sono cumulativamente: suite .NET verde, prove e anomalie risolte, riesame umano e approvazione visiva esplicita.
5. `Init`, rapporti e test dello strumento non devono leggere o modificare salvataggi di gioco o dati personali, né sovrascrivere prove esistenti.

## Conseguenze

Vantaggio: meno falsi positivi nella matrice P0 e possibilità di eseguire una verifica automatica del registro con exit code affidabile. Costo: le eccezioni richiedono approvazione umana e i file PNG sono verificati tramite `System.Drawing` disponibile sul sistema Windows che esegue la verifica. Per trasparenza non vengono generati né usati screenshot sintetici per dichiarare la parità grafica.
