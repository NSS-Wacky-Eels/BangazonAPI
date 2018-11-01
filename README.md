# BangazonAPI

## What is it?
BangazonAPI is a way to interact with the Bangazon Database. It is essential for Bangazon to operate, as it keeps track of customer's orders, their products, and Bangazon's employees.

## How do I use it?
### What you will need:
1. Basic knowledge of git bash
2. Visual Studio 2017
3. SQL Service Management Studio
4. Postman

### Steps to use BangazonAPI:
1. Clone this repo
2. Inside of the repo is a `SQLQueryBangazon.sql`, you will need to open this in SQL Service Management Studio, and execute the SQL.
3. In your git bash: `start Bangazon.sln`, which should open Visual Studio.
4. Open the `SQL Server Object Explorer` inside of Visual Studio.
5. Connect your SQL Server in order to view the database that was created when the `SQLQueryBangazon.sql` was executed.
6. Create an `appsettings.json` in the solution explorer.
7. Copy and paste all of the information in `appsettingsTEMP.json` into `appsettings.json`.
8. Add your connection string into `appsettings.json`.
9. Execute the`Bangazon.sln` in Visual Studio.

### Steps to test BangazonAPI:
1. Open Postman

**GET Functionality**

Paste `http://localhost:5000/api/` into Postman's URL

You can `Get` these different objects by pasting them (one at a time) after the `api/` in the URL:

`customers`, `product`, `orders`, `producttype`, `paymenttypes`, `department`, `trainingprogram`, `computer`.
