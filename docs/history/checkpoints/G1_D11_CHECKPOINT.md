# G1-D11 — Checkpoint ripresa E3b/3a-b

- **Base confermata:** v60-E3b2-RC3, tutti i test `validate.cmd` superati secondo l'utente (380 attesi).
- **Candidato nuovo:** v60-E3b3-RC1, sessione v14 isolata con `FromV13/Restore`, costruzione Scuola, iscrizione/annullamento, staffing ricercatori e acquisto manuale delle tecnologie.
- **Test nuovi:** 7 (sei Application, uno Infrastructure round-trip SQLite v3); **non eseguiti** in questo ambiente, target 387.
- **V13:** immutata, UI e vecchi salvataggi invariati.
- **Prossimo:** validazione Windows, E3b/3c clock/produzione/militari/ricerca integrati, E3b/3d tier, E3b/3e save/restore, UI, E4 anti-softlock.
- **Invarianti:** ricerca liberi/spendibile solo volontariamente; nessun doppio residente; nessuna spesa parziale; test e migrazioni su copie, storico documentale sotto `docs/history`.

[Rapporto dettagliato](../../design/G1_D11_E3B3_SESSION_ISOLATA.md) · [ADR 0027](../../adr/0027-e3b3-session-v14-isolata.md)
