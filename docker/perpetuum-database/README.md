# Overview

* Build a docker image based on mcr.microsoft.com/mssql/server
```
sudo docker build -t perpetuum-database .
```

Spin up a new container using `docker run`
```
sudo docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=StrongPassword" -p 1433:1433 -d perpetuum-database
```

Note: MSSQL passwords must be at least 8 characters long, contain upper case, lower case and digits.  
