#!/bin/bash

# Start the script to create the DB and user
    /var/opt/mssql/config/configure-db.sh &

# Start SQL Server
    /opt/mssql/bin/sqlservr
