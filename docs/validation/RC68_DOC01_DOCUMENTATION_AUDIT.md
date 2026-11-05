# RC68-DOC01 — Audit e riordino della documentazione

**Data:** 05/11/2026 · **Tipo di consegna:** esclusivamente documentazione e rigenerazione del manifest ZIP. **Nessuna nuova RC funzionale.**

## Perché

I documenti di accesso operativi contenevano molte sezioni successive «stato corrente RC48…RC68». Indicazioni vecchie (GUI v14 predefinita, SQLite gen. 5, F1 non attiva) erano state conservate accanto alle nuove senza una chiara separazione. Si rischiava di leggere come valide scelte superate e di perdere la GUI confermata dall'utente nella RC63.

## Interventi

- `README.md`: una pagina iniziale breve, con comandi e link attivi; cronologia rimandata a `CHANGELOG.md`.
- `docs/START_HERE.md`, `docs/README.md`: comandi e indice unificati, eliminata la successione di guide rapide di versioni passate.
- `docs/CURRENT_STATE.md`: uno stato reale (RC68 candidata, 806 test attesi; RC63 ultima validata con 771), dipendenze residue dichiarate.
- `docs/ROADMAP.md`: sequenza completa G0 → RC69–72 → E4a → E4b → M5.4–M7; task, dipendenze, rischi, prove e criteri di uscita.
- `docs/NEW_CHAT_HANDOFF.md`: singolo handoff attivo, informazioni sufficienti per riprendere senza chiedere nuovamente le scelte GUI o di gameplay.
- `docs/HANDOFF.md`: ora rimando breve per conservare i collegamenti storici.
- `docs/AUTHORITY_REGISTRY.md`, `docs/CATALOG_CURRENT.md`, `docs/GAME_DESIGN.md`: separate decisioni confermate, proposte, codice corrente e contenuto storico.
- `docs/SAVE_FORMAT.md`: contratto v15 in apertura e specifica storica v14 chiaramente separata.
- `docs/DEVELOPMENT.md` e `docs/ARCHITECTURE.md`: note introduttive storiche sostituite da riferimenti allo stato vigente, mantenendo il contenuto tecnico utile.

## Cosa non è stato eliminato

- **ADR 0001–0076:** cronologia delle decisioni, anche quando una decisione successiva sostituisce quella iniziale.
- Dossier `docs/design/`, piani/prove `docs/validation/`, report `docs/history/`, `CHANGELOG.md` completo: tracciabilità delle implementazioni e dei fallimenti precedenti.
- Vecchi documenti `*_ARCHIVE.md`: **conservati, ma esclusi dall'indice operativo** perché potrebbero contenere dettagli non ripetuti altrove. Una futura eliminazione richiede un audit dei contenuti e dei rimandi, non soltanto il criterio del nome.
- Tutti i sorgenti, test, strumenti, asset grafici, schemi e salvataggi di esempio eventualmente presenti nel repository (nessun file di questa natura è stato modificato o rimosso).

## Regole di qualità e stato della verifica

- Riesaminare i link Markdown locali dopo l'editing, verificare XML/XAML e JSON, e rigenerare `MANIFEST_RC68_DOC01_SHA256.txt` per tutti i file inclusi; verificare SHA-256 del nuovo ZIP e assenza di un vecchio manifest.
- Confrontare **hash per file** dei sorgenti C#, progetti, XAML, test, CSV, asset e script rispetto alla RC68: devono coincidere integralmente.
- `validate.cmd` e i test .NET 10 **non sono stati eseguiti da questa consegna documentale**. Ultima prova dell'utente: **RC63 771/771**, RC68 806 previsti.

## Come riordinare in futuro senza creare nuovi duplicati

Aggiornare **in posto** il solo documento operativo per argomento; aggiungere la storia al `CHANGELOG.md`, l'eventuale decisione ad un ADR e il piano di test al dossier dell'incremento. Non anteporre una nuova sezione «Stato corrente RCxx» a tutti i documenti ad ogni RC. Evitare di duplicare interamente l'handoff dentro la roadmap.

## Risultati del controllo statico

- **File XML/XAML e progetti analizzati:** 27; parsing XML superato.
- **JSON analizzati:** 39; parsing superato.
- **Collegamenti Markdown locali:** 1.835 validi, **0** destinazioni mancanti (controllo sui percorsi, non su tutti gli anchor remoti).
- **Sorgenti applicativi e test:** confronto SHA-256 rispetto allo ZIP RC68: **nessuna differenza**.
- **Gate dotnet/xUnit:** **non eseguito**. I controlli statici non garantiscono che gli 806 test RC68 passino.

La rigenerazione del manifest e l'integrità del nuovo archivio sono verificate separatamente durante il confezionamento.
