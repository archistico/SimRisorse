# Ripresa corrente — RC5 / E3b/3e

La RC4 ha **408 test superati** secondo l'utente. La RC5 aggiunge la costruzione civile nella sessione v14 e 9 test, **417 attesi**; test Windows ancora da eseguire. La partita v13 e SQLite v2 non sono coinvolti.

1. Estrarre lo ZIP in una cartella nuova.
2. Eseguire `.\validate.cmd` in PowerShell.
3. Riportare il numero di test superati ed eventuali errori completi.

Prossima fase E3b/4: GUI sperimentale separata, poi E4. La ricerca è a saldo libero con spesa volontaria, mentre i parametri di prezzo restano candidati.

[Documento tecnico](../../design/G1_D15_E3B3E_REPLAY_CIVILI_V14.md) · [Piano test](../../validation/V60_E3B3_RC5_TEST_PLAN.md) · [Checkpoint](../../history/checkpoints/G1_D15_CHECKPOINT.md).

---

# Stato operativo — v60-E3b3-RC4 (E3b/3d)

**Ultima validazione Windows confermata:** v60-E3b3-RC3, 396 test passati. **RC4:** 12 test nuovi, obiettivo 408; compilazione e test .NET ancora da eseguire. Le nuove operazioni `BuildProducer`, `UpgradeProducer`, `AssignProducerWorkers` e `ReleaseProducerWorkers` appartengono alla sola sessione v14 isolata. Non c'è ancora interfaccia per questi comandi.

Eseguire dalla radice in cartella separata: `.\validate.cmd`. Conservare i salvataggi v13 originali, senza migrazioni distruttive. [Contratto](../../design/G1_D14_E3B3D_COSTRUZIONE_UPGRADE_TIER_V14.md) · [Test](../../validation/V60_E3B3_RC4_TEST_PLAN.md) · [Checkpoint](../../history/checkpoints/G1_D14_CHECKPOINT.md). Prossimo E3b/3e, poi E3b/4 ed E4.

---

# SimRisorse — guida rapida alla ripresa

**Candidato corrente:** `v60-E3b3-RC3` (3 novembre 2026); **test Windows RC3 da eseguire**.  
**Ultimo pacchetto completamente validato dall'utente:** `v60-E3b3-RC1` (387 test).  
**RC2:** 396 test, 389 superati e 7 falliti perché la fixture del clock prenotava 10 abitanti con popolazione 8.

## Correzione RC3

- Sistemata **solo** `V14CampaignClockTests.cs`: fixture iniziale con popolazione 10, capienza 16; test con 10 lavoratori nell'impianto avanzato con popolazione 13.
- Invariante "lavoratori + militari + studenti/ricercatori <= popolazione" mantenuta invariata.
- La nuova sessione `civil-growth-14`, i sistemi E1/E2/E3 e SQLite v3 restano isolati; **la partita v13/GUI/SQLite v2 è invariata**.

## Validazione Windows

Estrarre lo ZIP in cartella separata e lanciare:

```powershell
.\validate.cmd
```

**Obiettivo: 396 test passati**, seguiti dai benchmark; compilazione e test RC3 non eseguiti nell'ambiente di preparazione. Nessuna prova su database personali senza copie.

## Prossimi passi

Una volta confermato il gate RC3: E3b/3d (costruzione/upgrade tier nella sessione), E3b/3e (replay e salvataggi), E3b/4 (GUI Scuola/Ricerca), E4 (bilanciamento e attivazione). La regola approvata è accumulo libero/spesa volontaria dei punti ricerca; coefficienti economici e costi Scuola restano candidati.

[Rapporto RC3](../../validation/V60_E3B3_RC3_FIX_CLOCK_FIXTURE.md) · [D13](../../history/checkpoints/G1_D13_CHECKPOINT.md) · [Roadmap](../../ROADMAP.md) · [D12](../../design/G1_D12_E3B3C_TEMPO_UNICO_V14.md).
