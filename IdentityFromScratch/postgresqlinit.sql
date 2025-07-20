-- For PostgreSQL 17.5

-- Create database
CREATE DATABASE identitytestdev WITH TEMPLATE   'template0'
                                     LC_LOCALE  'und-x-icu'
                                     LC_COLLATE 'und-x-icu';

-- Create user with password
CREATE USER itdadmin WITH ENCRYPTED PASSWORD 'myapppassword';

-- Grant all privileges on the database to the user
GRANT ALL PRIVILEGES ON DATABASE identitytestdev TO itdadmin;
CREATE DATABASE EXAMPLE_DB;
CREATE USER EXAMPLE_USER WITH ENCRYPTED PASSWORD 'Sup3rS3cret';
GRANT ALL PRIVILEGES ON DATABASE EXAMPLE_DB TO EXAMPLE_USER;
\c EXAMPLE_DB postgres
# You are now connected to database "EXAMPLE_DB" as user "postgres".
GRANT ALL ON SCHEMA public TO EXAMPLE_USER;
