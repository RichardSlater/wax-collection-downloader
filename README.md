## Usage

```
-c, --collection    Required. WAX name of the collection (case sensitive).

-s, --schema        Required. WAX name of the schema (case sensitive).

-a, --account       Required. WAX name of the account (case sensitive).

-o, --output        Required. CSV output file.

-e, --endpoint      (Default: atomic-wax.tacocrypto.io) Atomic Assets API Endpoint.

-p, --pageSize      (Default: 1000) Size of each API page.

-r, --retries       (Default: 10) Number of retries before giving up.

--help              Display this help screen.

--version           Display version information.
```

## Examples

The following will create a CSV file (that you can import into Excel or Google Sheets) for the specified user (`quadrellswar`) containing all of the `cards` from the `warsaken` collection.

```
wax-collection-download.exe download-collection-wax.exe --collection warsaken --schema cards --account quadrellswar -o c:\quad_warsaken_cards.csv
```