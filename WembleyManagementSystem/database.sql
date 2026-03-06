CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    Password NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) CHECK (Role IN ('Admin','Business','Client')) NOT NULL
);

CREATE TABLE Events (
    EventId INT PRIMARY KEY IDENTITY(1,1),
    EventName NVARCHAR(100) NOT NULL,
    EventDate DATETIME NOT NULL,
    Location NVARCHAR(200),
    Description NVARCHAR(500),
    CreatedBy INT,
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId)
);