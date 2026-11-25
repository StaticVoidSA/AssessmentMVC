-- Created DB through Visual Studio
USE Sizanani_Logistics;

-- Contractors(Many Vehicles), Vehicles(One Contractor) one to many
DROP TABLE IF EXISTS Vehicles;
DROP TABLE IF EXISTS Contractors;

CREATE TABLE Contractors(
  ContractorId INT IDENTITY(1, 1) PRIMARY KEY,
  ContractorName NVARCHAR(100),
  Email NVARCHAR(100),
  PhoneNumber NVARCHAR(20)
);

CREATE TABLE Vehicles(
  VehicleId INT IDENTITY(1, 1) PRIMARY KEY,
  Registration NVARCHAR(20),
  Type NVARCHAR(50),
  Model NVARCHAR(50),
  Weight INT NOT NULL,
  ContractorId INT NOT NULL,
  FOREIGN KEY(ContractorId) REFERENCES Contractors(ContractorId)
);