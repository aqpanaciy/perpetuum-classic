FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

# Create a config directory
RUN mkdir -p /var/opt/mssql/config
WORKDIR /var/opt/mssql/config/

# Bundle config source
COPY *.sh *.sql /var/opt/mssql/config/

#Copy database archive
COPY perpetuumsa.bak.gz /var/opt/mssql/data/

# Grant permissions for to our scripts to be executable
RUN chmod +x /var/opt/mssql/config/entrypoint.sh
RUN chmod +x /var/opt/mssql/config/configure-db.sh
RUN gzip -d /var/opt/mssql/data/perpetuumsa.bak.gz

ENTRYPOINT ["./entrypoint.sh"]
