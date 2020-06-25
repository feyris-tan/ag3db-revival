# The AG3DB rebuilder

The publicly available Torrent is not completeable anymore due to lack of seeders. 
However, the characters table and the character data in itself is complete.
These datasets can be used to guess the contents of the users table.

Also, this tool imports the character data and previews into the new SQL database. 
Appropriate Resources will be added to Azusarkus - as I do not want to go the FTP route.

## How to run

1. Pack the contents of the AG3DB Public Release into a tar file, like `tar -cvf ag3db.tar ~/Downloads/AG3DB/`
2. In src/main/resources create a file named sql.properties and fill it out like this:
```
url=jdbc:postgresql://127.0.0.1:5432/postgres
username=YOUR_USERNAME
password=YOUR_PASSWORD
```
3. Compile this program using `mvn compile`
4. Run moe.yo3explorer.ag3dbRebuild.presentation.Main and pass the path to the tar file as an argument.
 
