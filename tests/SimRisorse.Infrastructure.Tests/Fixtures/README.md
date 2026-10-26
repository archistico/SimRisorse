# Checkpoint storico M3

`economy-foundation-v1.sqlite3` proviene dal ciclo nativo privato di verifica M3, eseguito con Avalonia 11.3.18 e Microsoft.Data.Sqlite 10.0.9. Non contiene dati personali e non proviene dal salvataggio del giocatore.

Formato SQLite e snapshot v1, scenario `economy-foundation`, regole `economy-foundation-1`: 8 abitanti, un secondo trascorso, due taglialegna con due lavoratori, una segheria e un tagliapietra con un lavoratore ciascuno. Scorte: 4 tronchi, 100 legno lavorato e 71 pietra.

SHA-256: `10e6258ebaf9df0b2219f24b9fd529bea055781184c4c3c1ac6555ec1fe048b9`.

Il test di compatibilità apre questa fixture in sola lettura, confronta lo stato completo e salva una copia in un database temporaneo con il nuovo provider. Conservare il file originale quando cambiano le dipendenze; non rigenerarlo con la versione corrente per far passare il test.
