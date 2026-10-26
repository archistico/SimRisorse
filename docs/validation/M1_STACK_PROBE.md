# M1 — Verifica dello stack desktop

Data: 2026-10-26. Esito: **prova completata su Windows**. Evidenza riproducibile in [experiments/stack-probe](../../experiments/stack-probe/README.md), distinta dalla futura applicazione definitiva.

## Ambiente e perimetro

| Elemento | Verificato |
| --- | --- |
| Sistema operativo | Windows 10 x64, build 19045 |
| SDK | .NET 10.0.201 |
| Runtime installato | .NET 10.0.5 |
| Avalonia e headless | 11.3.18, pacchetti corrispondenti già disponibili localmente |
| Provider SQLite | Microsoft.Data.Sqlite 10.0.9 |
| Ripristino | Sorgente offline locale, lock file e cache dei pacchetti nel workspace |
| Pubblicazione | Release, framework-dependent, avvio nativo Windows riuscito |

La versione Avalonia provata non è quella più recente: serve a dimostrare la fattibilità con una catena locale completa. La scelta delle versioni del prodotto resta un'attività iniziale di M2. Linux e macOS sono possibilità del framework, **non piattaforme provate o distribuite in questa sessione**.

## Risultati

- Compilazione Release dei quattro progetti: **0 errori, 0 avvisi**.
- **23 test superati, 0 falliti, 0 ignorati** nell'esecuzione finale.
- Rendering Skia headless della finestra a 1180×790 e 1020×720, con ispezione delle immagini prodotte.
- Verifica dei binding e di comandi reali attraverso input mouse simulato.
- Verifica che i comandi principali rimangano entro l'area visibile alle due dimensioni.
- Pubblicazione e avvio nativo del pacchetto Windows: modalità smoke completata con codice di uscita 0.

### Comportamenti economici e temporali

Verificati valori a 1, 2 e 60 secondi, assenza di output senza input, blocco dei consumi quando l'output non trova spazio, ricette complete per i lavoratori servibili, acquisti falliti senza spesa e limiti delle assegnazioni.

Confrontati avanzamenti in blocco e in frazioni di secondo, anche su otto ore simulate. Verificati pausa, conservazione del tempo residuo al caricamento e visualizzazione di `32:00:00` senza ritorno a zero dopo 24 ore.

### Persistenza e reattività

Verificati round trip di risorse, tempo e assegnazioni su SQLite reale; ripresa in pausa; rifiuto di versioni, quantità o metadati incoerenti; assenza di creazione di un database vuoto al caricamento di un file mancante.

Una seconda scrittura fallita intenzionalmente tramite trigger lascia invariati sia i metadati sia lo stato del salvataggio precedente. Questa verifica riguarda il fallimento di una transazione: non è una prova di spegnimento improvviso del computer.

Il lavoro di persistenza è eseguito fuori dal dispatcher: un test con storage trattenuto da un segnale verifica che il tempo e lo stato di lettura della UI continuino ad aggiornarsi. Un caricamento fallito conserva la sessione corrente in pausa.

## Riproduzione

Dalla radice del repository, con i pacchetti disponibili nella cache indicata:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\scripts\validate-stack-probe.ps1 -OfflinePackageSource "$env:USERPROFILE\.nuget\packages"
```

Risultati locali sotto `.local-cache/stack-probe/`:

- `validation.json`: conteggi e risultato della prova nativa;
- `test-results/stack-probe.trx`: esiti dei test;
- `main-window.png` e `compact-window.png`: immagini della UI;
- `publish/`: pubblicazione dipendente dal runtime.

Gli artefatti sono ignorati da Git. Si conservano nel repository codice, lock file, script e questo report; gli output si rigenerano.

## Limiti e seguito

- Nessuna verifica su Linux/macOS o distribuzione self-contained.
- Nessun audit online delle dipendenze durante la validazione offline.
- Nessun catalogo completo, condizione di vittoria o classifica.
- Nessuna verifica del bilanciamento da 8 a 32 ore del gioco completo.
- La sospensione è riconosciuta nella prova con una soglia del timer; la gestione degli eventi reali di sospensione richiede lavoro nel desktop definitivo.
- UI, contenuti e API dell'esperimento non sono automaticamente API del prodotto.

La prova supporta l'adozione di C#/.NET, Avalonia/MVVM e SQLite per la prima implementazione desktop. Il prossimo passo è M2: selezionare versioni supportate, creare il Core definitivo e trasferire le regole del piccolo scenario attraverso casi d'uso e test, senza riferimenti ai progetti dell'esperimento.

## Riferimenti tecnici

- [Headless e acquisizione dei frame, Avalonia 11](https://v11.docs.avaloniaui.net/docs/concepts/headless/).
- [Integrazione headless xUnit](https://v11.docs.avaloniaui.net/docs/concepts/headless/headless-xunit/).
- [Transazioni Microsoft.Data.Sqlite](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/transactions).
- [ADR dello stack](../adr/0005-desktop-stack-candidate.md).
