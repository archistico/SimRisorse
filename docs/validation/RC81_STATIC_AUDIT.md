# RC81 — Audit statico del pacchetto candidato

Data: 06/11/2026. **Questo documento NON certifica build, esecuzione xUnit né funzionamento della GUI.** L'ambiente non possiede .NET 10; il codice RC79, RC80 e RC81 rimane candidato. Ultima convalida effettiva dichiarata dall'utente: RC78, 926/926.

## Controlli svolti

1. Diff del sorgente completo contro lo ZIP RC80: nessun file del prodotto mancante; i tre file di configurazione `editorconfig`, `gitattributes`, `gitignore` dell'archivio precedente sono presenti con i nomi convenzionali `.editorconfig`, `.gitattributes`, `.gitignore` nella RC81. Sono stati controllati gli interventi limitati a Core, simulazione, sessione, persistenza, proiezione UI, test e documentazione.
2. Scansione lessicale euristica dei delimitatori in **16** file C# aggiunti/modificati, escludendo commenti/stringhe e i blocchi SQL C# con stringa raw: nessuna parentesi/graffetta sbilanciata osservata. È un controllo superficiale, **non un parser né un compilatore C#**.
3. Ispezione del modello: la Zecca richiede 10 persone per modulo; capacità per tier 10/30/100 per edificio. Il clock deriva il numero di squadre complete separatamente per tier; i lavoratori di squadre parziali non sono convertiti in ricette. Gli altri produttori rimangono nella modalità precedente.
4. SQL DDL generation 6 estratto dal sorgente e interpretato autonomamente da Python `sqlite3` 3.46.1: **16 tabelle** create, `PRAGMA user_version=6`, entrambi i valori di `rules_version` scrivibili e leggibili nel campo previsto. Questo **non dimostra** la riuscita del round-trip con `Microsoft.Data.Sqlite` o la compatibilità con file di salvataggio reali: gli xUnit relativi sono preparati ma non eseguiti.
5. Riferimenti locali dei tre nuovi documenti verificati: nessun link relativo inesistente. Test RC81 individuati: **6 casi Core**, **10 Application**, **4 Infrastructure**, in totale **20** nuovi casi. RC78 926 + RC79 17 + RC80 22 + RC81 20 = **985 attesi**, non eseguiti.
6. Archivio ZIP completo costruito con manifest SHA-256 di ogni file. Il test ZIP CRC e la corrispondenza dei digest sono eseguiti dopo la costruzione (risultati nel report di confezionamento esterno o verificabili riaprendo l'archivio).

## Rischi e gate non assolti

- Nessun `dotnet restore`, `dotnet build`, `dotnet test`, TRX o smoke desktop: non attribuire "test passati" a RC79–81.
- Mancano prove manuali Windows del layout canonico v14/v15 e dei comandi di gestione lavoratori nella GUI.
- `EnableMintCrews()` è volutamente **solo API**: non esiste un nuovo controllo GUI né un avvio di partita automaticamente `MintTen`.
- I benchmark naturali RC77 descrivono ancora l'economia storica, non il regime con dieci lavoratori. Curva A/B/C e pagamenti rateali RC79 non sono attivi in gioco.
- Le fixture con scorte/personale preparati confermano soltanto le aspettative dei test quando saranno eseguiti, non il raggiungimento naturale della Zecca.

**Gate previsto su Windows:** `./validate-f1.cmd` (985/985, nessun fallimento né skip), seguito da ispezione visiva canonica. [Piano funzionale](E4B_RC81_RUNTIME_TEST_PLAN.md) · [ADR 0089](../adr/0089-attivazione-opt-in-squadre-zecca-v15-versionata.md).
