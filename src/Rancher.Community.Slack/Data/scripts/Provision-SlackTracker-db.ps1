$server=<db_server>
$port=<db_port>
$username=<username>
$password=<password>

# Create the database
$scripts = $(Get-Item ./*.sql) | Sort-Object -Property Name

# Execute each script
foreach( $script in $scripts) {
    Write-Host "Executing $script."
    sqlcmd -S "tcp:$server,$port" -U $username -P $password -i $script
}
