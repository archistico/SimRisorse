# ADR 0094 — Avvio ricerca con Matite del Municipio, costo unico e documentazione autorevole

**Data:** 06/11/2026. **Stato:** implementato in RC86, in attesa di validazione .NET/visiva.

## Problema

Il primo studente consuma 2 Matite. Il laboratorio produce Matite con Carbone; la Miniera di carbone richiede `Estrazione I`, acquistabile solo dopo aver generato punti ricerca da uno studente. Le nuove partite con zero Matite avevano un ciclo di prerequisiti. Nel dettaglio edificio il costo veniva mostrato due volte. Documenti operativi storici e sezioni di release replicate rendevano ambiguo quale stato fosse vigente.

## Decisioni

1. La **factory delle nuove campagne v15** assegna una sola volta 2 Matite all'inventario del Municipio. Sono consumabili come le altre risorse e rientrano nella capienza comune per Matite già prevista. Non sono rese infinite né ricaricate su Save/Load. Le importazioni di checkpoint preesistenti rimangono fedeli all'inventario originario.
2. Il benchmark naturale v15 usa la **stessa factory** del gioco; percorso educativo prima di `Estrazione I`, miniera di Carbone solo dopo lo sblocco. Non si tenta il crafting di Matite prima del Carbone. Report e tempi anteriori a questa modifica sono storici (scorta iniziale diversa).
3. Rimuovere nelle viste v14/v15 il TextBlock testuale ridondante, conservando titolo e `ItemsControl` con risorse e icone; costi dei lotti +5/+10 non toccati.
4. Un unico documento operativo per ciascuna famiglia: `ROADMAP.md`, `CURRENT_STATE.md`, `AUTHORITY_REGISTRY.md`, `NEW_CHAT_HANDOFF.md`. `docs/README.md` è l'indice unico. Eliminare `*_ARCHIVE.md` e vecchi handoff/avvii duplicati; conservare storia tecnica verificabile nel `CHANGELOG.md`, ADR e report specifici.

## Confini

Nessuna modifica ai prezzi di costruzione, ai coefficienti A/B/C, al modello MintTen, alle ricette, allo schema SQLite o al DB v14. La nuova dotazione non è retroattiva; eventuali partite già avviate con zero Matite conservano i loro dati. Se necessario servirà una decisione esplicita su un intervento di recupero per tali partite.

## Verifica

Test unitari di inventario iniziale e assenza di regrant sulle migrazioni, parità Desktop/benchmark, ordine `Estrazione I`→Carbone, test visuali e Save/Load; controllo di documentazione e ZIP. Verifiche eseguibili su Windows: `./validate-f1.cmd`. Niente risultati .NET dichiarati senza esecuzione.
