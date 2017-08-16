# EasyInv

**EasyInv** is a small program designed to make inventorying a little bit easier. You can feed it UPC barcodes and EasyInv will pull down the object's information from a database and format the results into a spreadsheet friendly style with options for exporting.

## Installation

1. You will first need to sign up for the [Digit-Eyes API](http://www.digit-eyes.com/api.html).
2. Download the EasyInv executable.
3. Open up a Command Prompt window and navigate to EasyInv.exe
4. Create a text file at this location with the extension '.api'.
5. On the first line of this file, you will need to insert your application key provided by Digit-Eyes.
6. On the next line, insert your authentication key which is also provided by Digit-Eyes and save/close the file.
7. It is reccomended you download a barcode scanning app that supports exporting to .csv for ease of use such as [this app for Android](https://play.google.com/store/apps/details?id=com.geekslab.qrbarcodescanner.pro&hl=en) or [this app for iOS](https://itunes.apple.com/us/app/free-qr-code-reader-barcode-scanner-for-iphone/id903799541?mt=8).

## Usage

Type 'easyinv' followed by a command. The commands are as follows:

Command | Value | Description
------------- | ------------- | -------------
-c/--csv | file path to '.csv' | The path to a CSV containing UPC codes in the first collumn. Requires path to a '.csv' file.
-u/--upc | 12 digit number | A single UPC code to scan. Can be used multiple times in one execution. Requires numerical UPC code.
-e/--export | file path to '.csv' | Export results to a csv. Requires a path to a '.csv' file (it will create a new one at the location with the given filename).
-h/--header | string | A header for the results.
--setup |  | Information on how to initalize EasyInv.
--help |  | Information on the commands for EasyInv.

### Examples: 

* easyinv -c C:\Users\John\barcodes.csv -e C:\Users\John\results.csv -h Results
* easyinv -u 056754365781 -u 018574659783 -e C:\Users\John\results.csv

## License

[MIT License](https://opensource.org/licenses/MIT)
