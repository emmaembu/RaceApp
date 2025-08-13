IF DB_ID('PawRaceDb') IS NULL
BEGIN
    CREATE DATABASE PawRaceDb;
END

IF OBJECT_ID('dbo.Dog', 'U') IS NULL
BEGIN
	CREATE TABLE Dog
	(
		Id INT IDENTITY(1,1) PRIMARY KEY,
		Name	NVARCHAR(100) NOT NULL,
		Breed	NVARCHAR(100) NOT NULL,
		Age		INT
	)
END

INSERT INTO dbo.Dog
(
	Name, 
	Breed,
	Age
)
VALUES
	('Max','Greyhound', 1),
	('Otto','Whippet', 2),
	('Sally','Saluki', 3),
	('Alex','Borzoi', 4),
	('Snape','Afghan Hound', 5),
	('Das Pas','Vizsla', 6),
	('Alpha','Greyhound', 1),
	('Beta','Whippet', 2),
	('Speed','Saluki', 3),
	('Space','Borzoi', 4),
	('Curlly','Afghan Hound', 5),
	('Winner','Vizsla', 6)

IF OBJECT_ID('dbo.Race', 'U') IS NULL
BEGIN
	CREATE TABLE Race
	(
		Id INT IDENTITY(1,1) PRIMARY KEY,
		RaceStatusId	INT NOT NULL DEFAULT 0,
		ScheduledAt		DATETIME,
		StartedAt		DATETIME,
		FinishedAt		DATETIME
	)
END

IF OBJECT_ID('dbo.RaceDog', 'U') IS NULL
BEGIN
	CREATE TABLE RaceDog
	(
		RaceId	INT NOT NULL,
		DogId	INT NOT NULL, 
		StartPosition INT NOT NULL,
		Odds DECIMAL NOT NULL,
		IsAvailable BIT DEFAULT 0,
		IsWinner BIT DEFAULT 0,
		CONSTRAINT PK_RaceDog PRIMARY KEY (RaceId, DogId),
		CONSTRAINT FK_RaceDog_Race FOREIGN KEY (RaceId) REFERENCES Race(Id) ON DELETE CASCADE,
		CONSTRAINT FK_RaceDog_Dog FOREIGN KEY (DogId) REFERENCES Dog(Id) ON DELETE CASCADE
	);
END

IF OBJECT_ID('dbo.User', 'U') IS NULL
BEGIN
	CREATE TABLE [User]
	(
		Id INT PRIMARY KEY IDENTITY(1,1),
		GUIDId [uniqueidentifier] NOT NULL UNIQUE,
		NickName NVARCHAR(50) NOT NULL,
		IpAddress NVARCHAR(45) NOT NULL,
		CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
	);
END

IF OBJECT_ID('dbo.Ticket', 'U') IS NULL
BEGIN
	CREATE TABLE Ticket
	(
		Id INT IDENTITY(1,1) PRIMARY KEY,
		UserId [uniqueidentifier] NOT NULL,
		TotalAmount DECIMAL(18,2) NOT NULL,
		PotentialWinning DECIMAL(18,2) NOT NULL,
		CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
		StatusId TINYINT NOT NULL DEFAULT 0,
		FOREIGN KEY (UserId) REFERENCES [User](GUIDId)
	);
END

IF OBJECT_ID('dbo.RaceDog', 'U') IS NULL
BEGIN
	CREATE TABLE TicketDetails
	(
		ID INT IDENTITY PRIMARY KEY,
		TicketId INT NOT NULL,
		RaceId INT NOT NULL,
		DogId INT NOT NULL,
		CONSTRAINT FK_TicketDetails_TicketId FOREIGN KEY (TicketId) REFERENCES Ticket(Id),
		CONSTRAINT FK_TicketDetails_RaceId FOREIGN KEY (RaceId) REFERENCES Race(Id),
		CONSTRAINT FK_TicketDetails_DogId FOREIGN KEY(DogId) REFERENCES Dog(Id)
	);
END
