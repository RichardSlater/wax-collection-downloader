## Installation

The following are the basic steps:

1. [Install the .NET 8 runtime](https://learn.microsoft.com/en-gb/dotnet/core/install/) for your operating system (Windows, Linux or MacOS).
2. Download and unzip the [latest release](https://github.com/RichardSlater/wax-collection-downloader/releases).
3. Unzip to your computer.
4. Open a terminal and change directory to the location you unzipped the file.
5. Run the command:

```
download-collection-wax.exe --collection {{ collection name }} --schema {{ schema }} --account {{ account }} -o {{ output file }}
```

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
download-collection-wax.exe --collection warsaken --schema cards --account quadrellswar -o c:\quad_warsaken_cards.csv
```