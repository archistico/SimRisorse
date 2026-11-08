# ADR 0116 — RC105: primo formato sperimentale unificato SQLite generation 7

**Stato:** candidato, non approvato per partite normali. **Data:** 07/11/2026.

## Contesto e scelta

Il composito RC95–RC104 conserva la città generation 6 come **BLOB di un secondo database**; cantieri e ricevute sono nello stesso file, ma l'autorità vive in un registro esterno. Il loader giocabile generation 6 rifiuta tabelle nuove. Serve quindi un formato distinto e verificabile prima di una vera migrazione.

La RC105 introduce **`E4bV15UnifiedSavePrototype`**, con `application_id=0x53465237`, `user_version=7` e:

- le **16 tabelle cittadine v15** già presenti in generation 6 (ad es. `game_save`, `resource_stock`, `progression_family`);
- `unified_campaign`: `campaign_id`, `revision`, `funding_json` (preventivo e deposito), `authority_mode` impostato e vincolato a `rc105-experimental-not-authoritative`;
- `unified_receipt`: ricevute originali e revisioni contigue 1..N, compresi pagamenti, tick e costruzioni.

Il prototipo **non contiene** il BLOB `native_v15_image`. È una copia di osservazione e non assume l'autorità della partita.

## Confini di sicurezza

1. Origine obbligatoria: composito registrato che supera `E4bV15UnifiedSavePreflight` con il **registro attivo**, identità e protezioni coerenti. Backup non promossi e file senza registro non sono convertibili.
2. Esportazione **solo verso un percorso nuovo**, con SQLite online backup temporaneo della sorgente, generazione delle tabelle cittadine, transazione per metadati/ricevute, round-trip completo, nuova verifica dell'autorità e `File.Move` senza overwrite. Sorgente e registro non vengono modificati.
3. Un cantiere ancora aperto e le rate pagate **vengono conservati**, non trasformati in nuove risorse o scartati.
4. Il lettore `Inspect` apre il file in sola lettura, controlla `PRAGMA integrity_check`, versione, schema, identità e ricevute, verifica la città tramite una **copia privata temporanea** compatibile generation 6 e riconcilia l'escrow con il checkpoint.
5. Il formato è **NON giocabile e NON autorevole**. Nessun writer, clock, comando di costruzione o promozione è esposto. Il normale `V15SqliteFoodSaveStore` deve rifiutarlo.

**Limite esplicito:** la verifica prima/dopo la copia non garantisce che il ramo resti immutato dopo la pubblicazione; per questo il file RC105 non acquisisce mai autorità. La verifica delle ricevute è strutturale, non una firma crittografica contro la manomissione intenzionale.

## Restano da fare

RC106+: introdurre gestione dell'autorità nello stesso salvataggio, write-path atomico di città/finanziamento/ricevuta, isolamento tra processi, recovery dopo crash e migrazione controllata; solo dopo connettere comandi e GUI. La curva economica A e la classifica online restano fuori ambito.

**Invariati:** generation 6 giocabile, GUI pixel art v14/v15, Municipio con 100 Carbone e 2 Matite iniziali, prezzi attivi, unico `validate.cmd`.
