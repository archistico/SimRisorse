# Audit documentale G1-D19 — passaggio a nuova chat

**Data:** 3 novembre 2026. **Intervento:** esclusivamente documentazione, nessuna funzionalità aggiunta. La base è **v60-E3b4-RC3**.

## Problema individuato

- `docs/HANDOFF.md` e `docs/NEW_CHAT_HANDOFF.md` conservavano in cima come «stato attivo» una RC3 **precedente** (clock) anziché la RC3 **GUI** corrente.
- `docs/README.md` si presentava ancora come E3b3-RC2 o E3b2-RC3, nonostante le milestone successive.
- `docs/AUTHORITY_REGISTRY.md` conteneva una vecchia attribuzione di `civil-growth-14` al navale, superata dalla candidata nuova economia.
- `CHANGELOG.md` conteneva append successivi con stati «da testare» poi diventati verdi: mancava un sommario autorevole.
- La roadmap era troppo sintetica per gestire la precedenza della **parità GUI** rispetto al nuovo sviluppo.

## Azioni eseguite

1. Consolidati indici, README e stato; distinto **RC1 427 test confermati**, **RC2 respinta visivamente**, **RC3 439 test attesi e audit non confermato**.
2. Prodotta roadmap per gate da P0 a P6, con attività, rischi, dipendenze e criteri di accettazione.
3. Prodotto handoff completo e documento autonomo per iniziare una nuova chat; indicati file chiave, vincoli, comandi e rischi.
4. Creato registro decisioni sintetico, separando richieste confermate dai numeri di bilanciamento solo candidati.
5. Archiviate le versioni precedenti dei documenti operativi in `docs/history/operational/`, preservandone contenuto e tracciabilità.
6. Preparata matrice di parità v13/v14 con **22 gate non ancora convalidati manualmente**.

## Verifiche e limiti

- Controllo di integrità ZIP, verifiche dei percorsi Markdown locali e confronto hash del codice con RC3: risultati registrati nell'output del packaging.
- Nessuna compilazione .NET eseguita per questo riordino (non necessaria per la sola documentazione, ma **i 439 test RC3 restano pendenti**).
- Nessuna affermazione di conformità visiva v14: occorre una prova Windows reale con comparazione della GUI.

## Gerarchia documentale

Per riprendere usare nell'ordine `docs/HANDOFF.md`, `docs/CURRENT_STATE.md`, `docs/AUTHORITY_REGISTRY.md`, `docs/ROADMAP.md` e `docs/validation/G1_D19_MATRICE_PARITA_GUI.md`. Gli storici sono utili per decisioni e ricostruzioni, **non** per indicare il checkpoint corrente.
