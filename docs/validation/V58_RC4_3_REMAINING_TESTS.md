# SimRisorse v58-RC4 — tre test ancora rossi dopo la RC3

**3 novembre 2026 — stato: RC4 predisposta, da validare su Windows.**

## Evidenza acquisita (RC3)

- `validate.cmd` raggiunge i test xUnit: **299 eseguiti, 296 superati, 3 falliti**, nessun test ignorato.
- Le suite Infrastructure sono superate. La compilazione C# è riuscita.
- Il test UI sul testo della ricetta Pane, fallito in RC2, risulta ora superato.
- I benchmark e gli smoke successivi alla suite non sono stati eseguiti perché `validate.cmd` si è interrotto dopo il risultato xUnit.

### Correzioni RC4

1. **Core** — `IndividualRecruitmentTests.LegacyConversionPreservesPeopleAndNormalizesPercentProgress`, riga 91: i due array erano visualizzati entrambi come `[15]`, ma l'overload di `Assert.Equal` ha riportato un'uguaglianza fallita. La RC4 usa `Assert.Single` e confronta direttamente il progresso scalare `15`, mantenendo i controlli sulla popolazione, sulla migrazione e sulla validazione. **Il convertitore di gioco non viene cambiato.**
2. **Application** — `WarehouseTests.UnknownOrInconsistentRulesAreNotConverted`, riga 104: il test sulla versione futura `civil-growth-14` restituisce `None`, inatteso rispetto al ramo `TryRestore` e al ruleset corrente `civil-growth-13`. Per non mascherare un'incompatibilità potenzialmente pericolosa, `GameRuntime.TryRestore` rifiuta esplicitamente, *prima* della migrazione, le versioni `civil-growth` non presenti nella catena dichiarata. Il test ora verifica anche versione corrente, versione fornita, risultato diagnostico e una versione ignota aggiuntiva (`civil-growth-999`). La compatibilità delle vecchie versioni note e del fork v41 non viene intenzionalmente modificata.
3. **Desktop** — `DesktopTests.HistoricalFoundationKeepsMilitaryNavigationHidden`, riga 237: la RC3 aveva tentato un `Binding` al VM sul contenitore `ListBoxItem`, ma il controllo risultava visibile dopo `Show()`. La RC4 imposta `IsVisible` come valore locale e lo riapplica durante `Opened`, al completamento `Loaded` del contenitore e dopo l'inizializzazione asincrona del VM. Il test verifica la voce *prima* e *dopo* `Show`; quello sulle partite militari verifica che resti visibile e navigabile.

### File C# modificati rispetto alla RC3

- `src/SimRisorse.Application/GameRuntime.cs`
- `src/SimRisorse.Desktop/Views/MainWindow.axaml.cs`
- `tests/SimRisorse.Core.Tests/IndividualRecruitmentTests.cs`
- `tests/SimRisorse.Application.Tests/WarehouseTests.cs`
- `tests/SimRisorse.Desktop.Tests/DesktopTests.cs`

**Invariati:** `SimRisorse.Core`, codice di persistenza SQLite, schema database, regole dei costi, economia, risorse e salvataggi.

## Procedura di verifica

1. **Estrarre RC4 in una nuova directory vuota** (`Riallineata-RC4`), non sovrascrivere una vecchia copia. I log precedenti provengono dalla stessa cartella `Riallineata` e un albero pulito evita dubbi su file compilati/copie rimaste.
2. Eseguire `\.\validate.cmd` dalla root e conservare il riepilogo completo. Attesi **299 test** finché non si introducono ulteriori casi.
3. Se un test fallisce, riportare **nome, stack trace, percorso, riga e risultati**. Per il test Application è ora incluso un messaggio diagnostico che distingue ruleset richiesto, ruleset corrente e risultato.
4. Se tutte le suite xUnit passano, raccogliere anche i risultati dei benchmark, delle sonde desktop e dei test di caricamento/salvataggio; la sola suite verde non chiude automaticamente R3.
5. Provare eventuali file SQLite personali **solo in copia**, verificando il backup iniziale.

## Limiti della verifica

L'archivio è stato confrontato con RC3 e validato staticamente; non è stato compilato né eseguito in questo ambiente, che non dispone del .NET SDK 10. Questa non è una conferma di 299 test superati. **R3 rimane aperto.**
