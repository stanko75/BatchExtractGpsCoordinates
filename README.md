## Fast Extraction of GPS Coordinates and Saving to CSV, JSON, or a Database
### Usage:
BatchExtractGpsCoordinates.exe folder:C:\ csvFile:test.csv jsonFile:test.json connectionString:"Server=myServer;Database=myDb;User Id=myUser;Password=myPass;TrustServerCertificate=True;" tableName:GpsInfo

### Notes:
* The parameters csvFile, jsonFile, and connectionString are optional, but at least one of them must be provided.
* If connectionString is set but tableName is not, the default table name GpsInfo will be used.
* A log file named BatchExtractGpsCoordinates.log will be automatically created in the same folder as BatchExtractGpsCoordinates.exe
