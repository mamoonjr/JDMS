-- JDMS - MySQL database setup
-- Run this in MySQL Workbench or: mysql -u root -p < JDMS_MySQL_Setup.sql

CREATE DATABASE IF NOT EXISTS jdms_db
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

-- Optional: dedicated application user (recommended for production)
-- CREATE USER IF NOT EXISTS 'jdms_user'@'localhost' IDENTIFIED BY 'YourStrongP@ssw0rd!';
-- GRANT ALL PRIVILEGES ON jdms_db.* TO 'jdms_user'@'localhost';
-- FLUSH PRIVILEGES;

-- Tables are created automatically by EF Core migrations on first app run, or run:
--   dotnet ef database update --project src/JDMS.Infrastructure --startup-project src/JDMS.Web

USE jdms_db;
