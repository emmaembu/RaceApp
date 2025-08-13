Paw Race 

This is backend API for managing races and betting , built with .NET.

Project Structure

PawRace.DataAccess - data access layer
PawRace.Models - DTOs, enums, mapping profile
Offer/ OfferService - offer management service 
Ticket/TicketService - ticket management service
Wallet/WalletService - in-memory wallet
PawRaceAPI - ASP.NET Core Web API Project
RaceOfferMaintainer/MaintainService

How to run

Create database : run the script PawRace_DB_Script.sql.
Open PawRaceAPI/ PawRace.sln. Currently is set as Multiple Startup Projects : PawRaceAPI and RaceMaintainer.

Configuration

Settings are in appsettings.json( connection string, max bet amounts)

Technologies used : .NET 8, Entity Framework Core, SignalR for real-time communication (only initialization)

Features

Race and ticket management.
In memory wallet for testing.
Background services for ticket processing and race managment.

Notes: API supports basic ticket statuses only (Pending, Rejected, Success, Won, Lost) and one winner. Wallet is currently in memory, should use database in future.