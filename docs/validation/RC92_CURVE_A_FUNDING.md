# RC92 — Test plan: finanziamento progressivo Curve A (sandbox)

## Perimetro di accettazione

- Eseguire `./validate-f1.cmd` da Windows/.NET 10. Attesi **1203 test**: 1167 baseline RC90.3 verificata dall'utente, 21 RC91.3 ancora senza gate verde, **15** nuovi casi RC92. Nessun ignorato, errore o test fallito.
- Il gate richiede tre file nuovi in `.local-cache/m4/f1-gates/<run>/rc92-curve-a-funding/`: `rc92-curve-a-funding.json`, `.csv`, `.md`, con **6 acquisti riconciliati** e proprietà `Mode=curve-A/opt-in/in-memory-only/not-playable`.
- Provare anche il comando `dotnet run --project tools/SimRisorse.Simulation -c Release -- --study-e4b-curve-a-funding --output .local-cache/m4/rc92-manuale`. La directory non deve contenere report già esistenti.

## Matrice coperta da xUnit

1. Due acquisti successivi Base, Grande e Avanzato; il secondo utilizza il nuovo contatore, i preventivi restano coerenti con lo studio RC78 e la somma dei versamenti corrisponde esattamente al costo.
2. Stock sparso, capacità per risorsa, preventivi sopra capacità, rate limitate ai depositi e rifornimento ipotetico esplicitamente distinguibile dalla produzione reale.
3. Aperture duplicate, tentativo di finalizzare prima del pagamento, versamento zero/sovrabbondante o con materiale non richiesto, rifornimento sopra capienza: nessun cambio di stato in caso di rifiuto.
4. Rimborso totale e conservazione dei contatori; annullamento impossibile se il materiale restituito supererebbe la capacità: credito conservato.
5. Contatore della famiglia condiviso tra i tier, acquisizioni precedenti, input/tier invalidi, documenti riproducibili e nessun effetto sulla campagna reale.

## Limiti della prova

**I materiali consegnati nel sandbox sono ipotetici:** nessun benchmark di avanzamento naturale, prerequisiti reali, clock produttivo, ciclo demografico o SQLite è validato dall'esito `Reconciled=true`. Le curve non sono abilitate in partita né approvate. La RC91.3 necessita ancora della validazione visiva completa su Windows.
