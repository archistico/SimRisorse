# Piano di validazione RC67 — Contratto civico v15 nativo

**Ultimo checkpoint confermato dall'utente:** RC63, 771/771 test. RC64 +6, RC65 +7, RC66 +7 non ancora testati. RC67 +6 test Application: **797 cumulativi attesi**. Compilazione .NET 10 e xUnit non eseguiti in questo ambiente.

```powershell
.\validate.cmd
.\run-game.cmd         # GUI v15 predefinita, stessa impostazione v14
.\run-game.cmd --v14   # GUI storica e salvataggio personale isolato
# Stress combinato facoltativo
 dotnet run --project tools/SimRisorse.Simulation -c Release --no-restore -- --audit-v15 --seconds 120 --profile combined
```

## Nuovi test (6)

1. Proiezione `NativeCityCampaign`: sempre 15 famiglie e nessun `food-store` sintetico; round-trip del record senza perdite.
2. Conversione completa: quantità risorse, tempo/tick, crescita frazionaria, ricerca e resto del record conservati.
3. `FromInherited` respinge formati/regole v14 inattesi e valori null.
4. Il validatore nativo e il ponte storico approvano la stessa città legittima.
5. Il validatore nativo respinge una famiglia mancante e non muta l'input.
6. Capacità calcolate dal DTO v15 identiche a quelle della proiezione ereditata.

## Gate

- Tutti i 791 test precedenti più i nuovi 6, senza test ignorati.
- Nessuna modifica visiva o interattiva rispetto alla RC63; le sei filiere alimentari restano visibili e consultabili.
- Integrità dei salvataggi SQLite generation 6, senza cambi di schema.
- Prova facoltativa di audit combinato su 24 ore. Registrare test eseguiti, non confonderli con le verifiche statiche.
