# SimRisorse — v60-E3b3-RC5 (candidata)

Confermati 408 test sulla RC4. La RC5 aggiunge nella sola sessione v14 la costruzione di Case, Magazzini e Caserme con prezzi progressivi, e 9 nuovi test di replay tra sessione diretta e checkpoint SQLite v3 (**417 test attesi, da eseguire**). Partita v13, GUI e salvataggi storici non modificati.

Validare da Windows: `.\validate.cmd`. Dettagli: [guida](../../../docs/START_HERE.md) · [D15](../../../docs/design/G1_D15_E3B3E_REPLAY_CIVILI_V14.md) · [ADR 0030](../../../docs/adr/0030-e3b3e-edifici-civili-replay-v14.md).

---

# SimRisorse — stato v60-E3b3-RC4

**Candidato E3b/3d:** costruzione diretta, potenziamenti e assegnazione lavoratori per produttori Base/Grandi/Avanzati nella **sola sessione sperimentale v14**. Ultimo gate Windows validato: **RC3, 396 test superati**; RC4 aggiunge 12 test (obiettivo 408) ancora da eseguire. I prezzi giocabili della v13, UI Avalonia e salvataggi SQLite v2 sono invariati. Coefficienti numerici ancora sperimentali.

Avvio verifica Windows: `.\validate.cmd`. Dettagli: [guida](../../../docs/START_HERE.md), [roadmap](../../../docs/ROADMAP.md), [rapporto](../../../docs/design/G1_D14_E3B3D_COSTRUZIONE_UPGRADE_TIER_V14.md), [ADR 0029](../../../docs/adr/0029-e3b3d-comandi-tier-sessione-v14-isolati.md).

---

# SimRisorse — checkpoint v60-E3b3-RC3 (3 novembre 2026)

**Ultima validazione Windows interamente verde:** `v60-E3b3-RC1` — 387 test passati secondo l'utente.  
**RC2:** 396 test eseguiti, 389 superati, 7 falliti per fixture errata. **RC3:** fixture corretta, 396 test attesi, validazione Windows pendente.

- La campagna giocabile rimane `civil-growth-13` con SQLite v2, Avalonia e regole economiche/militari immutate.
- Il ramo isolato `civil-growth-14` include prezzi progressivi, edifici Base/Grandi/Avanzati, Scuola e ricerca, snapshot e store SQLite v3.
- **E3b/3c:** un unico `Advance(TimeSpan)` gestisce produzione, consumo, abitanti, reclutamento, studenti e punti ricerca, con pausa e commit atomico.
- **Non ancora pronti:** costruzione/upgrade tier nella sessione, GUI delle nuove meccaniche, validazione di bilanciamento/anti-blocco e attivazione della v14.
- Parametri economici e tecnologici **candidati**; è approvato l'accumulo libero di punti ricerca con spesa volontaria.

## Verifica Windows

Estrarre lo ZIP in una cartella separata ed eseguire dalla radice:

```powershell
.\validate.cmd
```

Non provare il nuovo store su database personali senza una copia. Nel container non è disponibile .NET 10 per compilare e provare la nuova suite.

Documenti: [guida](../../../docs/START_HERE.md), [roadmap](../../../docs/ROADMAP.md), [stato](../../../docs/CURRENT_STATE.md), [specifica G1-D12](../../../docs/design/G1_D12_E3B3C_TEMPO_UNICO_V14.md), [ADR 0028](../../../docs/adr/0028-e3b3c-orologio-unico-v14-isolato.md), [correzione RC3](../../../docs/validation/V60_E3B3_RC3_FIX_CLOCK_FIXTURE.md), [changelog](../../../CHANGELOG.md).

**Radice ordinata:** checkpoint precedenti sotto `docs/history/`, senza file G1_Dn o Vnn sparsi nella home.
