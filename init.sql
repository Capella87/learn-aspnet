-- PostgreSQL script to initialize the database schema for ASP.NET Core Application

CREATE DATABASE dbdb WITH
    LC_LOCALE= 'und-x-icu'
    LC_COLLATE = 'und-x-icu'
    TEMPLATE = template0;

CREATE USER myuser WITH ENCRYPTED PASSWORD 'mypassword';

-- Source: https://docs.digitalocean.com/support/how-do-i-fix-a-permission-denied-for-schema-public-error-in-postgresql/
-- Source: https://stackoverflow.com/questions/67276391/why-am-i-getting-a-permission-denied-error-for-schema-public-on-pgadmin-4
GRANT ALL PRIVILEGES ON DATABASE dbdb TO myuser;

\c dbdb postgres

GRANT ALL ON SCHEMA public TO myuser;
