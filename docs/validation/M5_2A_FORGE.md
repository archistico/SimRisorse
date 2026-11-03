# M5.2a — Forgia e Spade: gate da verificare

**Preparazione 28/10/2026, pacchetto v38.** Implementazione effettuata, **nessuna nuova esecuzione .NET disponibile in questo ambiente**. Non confondere i casi predisposti con test superati. Il più recente gate con rapporto dettagliato resta 206/206; l'utente ha riferito il superamento dei test della v33 (208 casi attesi), ma ha anche segnalato errori successivi in `DesktopTests` senza il messaggio specifico.

## Contratto giocabile nuovo

| Voce | Valore |
| --- | --- |
| Regole | `civil-growth-8`, da `civil-growth-7` via conversione S1 |
| Catalogo | 18 edifici, 15 risorse |
| Forgia | Costruzione: 240 Tavole, 180 Pietra; prerequisito Fonderia di ferro |
| Personale | 1 addetto per Forgia |
| Ricetta | **1 Ferro + 1 Carbone → 2 Spade** al secondo per addetto |
| Carbone | Fonderia ferro priorità 90; Fonderia oro 91; Forgia 100 |
| Scorta | Spade base 1.000; ogni Magazzino aggiunge 500, anche alle Spade |
| Impiego | Solo stoccaggio: reclutamento con consumo di Spade rinviato a M5.3 |
| Ritorno | Nessuna produzione offline, lotti indivisibili e nessuna modifica allo snapshot SQLite v2 |

## Verifiche incluse nel codice

Nuovo `ForgeIndustryTests` nella suite Application:

1. Prezzo, ricetta, addetto, prerequisito, priorità e capacità; catalogo storico v7 rimasto immutato.
2. Acquisto reale della Forgia e primo lotto: il Ferro creato dalla fonderia nello stesso secondo **non è ancora disponibile** per la Forgia. Al secondo successivo vengono accreditate 2 Spade.
3. Lotti: a **998/1.000** la Forgia arriva esattamente a 1.000; a **999/1.000** il lotto da 2 è bloccato senza spendere Ferro/Carbone. Un Magazzino amplia la capacità a 1.500 e sblocca il lotto.
4. Contesa: con 2 Carbone e le due fonderie presidiate la Forgia si ferma; liberando un addetto della Fonderia di ferro, il Carbone rimasto può arrivare alla Forgia.
5. Un checkpoint che si dichiara v7 ma contiene Spade positive viene rifiutato, senza sostituire la sessione corrente.

Nella suite Infrastructure: salvataggio reale v7 → apertura in memoria v8 → nuovo salvataggio v8, con verifica del backup, dello stato, del tempo frazionario, degli addetti e dei materiali industriali.

Nella suite Desktop: icone Forgia/Spade, descrizione, costo e riquadro della capacità con lotto da 2. Nella teoria `WarehouseTests` aggiunto un caso di conversione v7. Il conteggio previsto cresce da **212 a 221 test xUnit**; il numero deve essere verificato dal runner, non assunto come esito.

Il benchmark `--benchmark-industry-scale` continua a usare `CreateGoldForBenchmarks()` e a emettere **v7** per conservare gli oracoli di M5.1c; gli altri comandi usano v8. L'esperimento separato ForgeMintProbe usa anch'esso il catalogo v7 per non duplicare Forgia/Spade e continua a simulare A/B senza essere inserito nel validatore principale.

## Comandi per il gate Windows

Da PowerShell nella radice del progetto:

```powershell
dotnet test tests/SimRisorse.Desktop.Tests/SimRisorse.Desktop.Tests.csproj -c Release --logger "console;verbosity=normal"
dotnet test tests/SimRisorse.Application.Tests/SimRisorse.Application.Tests.csproj -c Release
dotnet test tests/SimRisorse.Infrastructure.Tests/SimRisorse.Infrastructure.Tests.csproj -c Release
.\validate.cmd
```

Acquisire prima il **primo errore completo** di `DesktopTests` (nome del test, messaggio, file/riga, stack trace), senza attribuirlo automaticamente alle modifiche v36–v37 o v38. Poi confermare tutti i test, i benchmark v7 congelati, i layout 1040×740 e 1240×850 e lo smoke di apertura di un salvataggio v7.

**Condizione per M5.2b:** gate Windows positivo (incluse migrazione, rendering e benchmark storico), diagnosi degli errori Desktop chiarita. Solo allora introdurre Zecca e Monete con un ulteriore passaggio `civil-growth-8 → civil-growth-9`. I prezzi e la ricetta della Zecca sono già approvati nel [dossier](../design/M5_2_FORGE_MINT_PREFEASIBILITY.md).
