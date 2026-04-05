-- Devia Backend schema for MySQL 8
-- Source: EF Core domain model (Users, Apps, Deployments, Subscriptions)

CREATE DATABASE IF NOT EXISTS `dev-ia_db`
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE `dev-ia_db`;

CREATE TABLE IF NOT EXISTS `Users` (
  `Id` CHAR(36) NOT NULL,
  `Name` VARCHAR(120) NOT NULL,
  `Email` VARCHAR(200) NOT NULL,
  `PasswordHash` LONGTEXT NOT NULL,
  `AuthRole` VARCHAR(16) NOT NULL,
  `ProfileRole` VARCHAR(120) NOT NULL,
  `Location` VARCHAR(120) NOT NULL,
  `Plan` VARCHAR(16) NOT NULL,
  `Status` VARCHAR(16) NOT NULL,
  `JoinedAtUtc` DATETIME(6) NOT NULL,
  `AvatarInitial` VARCHAR(2) NOT NULL,
  CONSTRAINT `PK_Users` PRIMARY KEY (`Id`),
  CONSTRAINT `IX_Users_Email` UNIQUE (`Email`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Apps` (
  `Id` CHAR(36) NOT NULL,
  `UserId` CHAR(36) NOT NULL,
  `Name` VARCHAR(160) NOT NULL,
  `Type` VARCHAR(160) NOT NULL,
  `Status` VARCHAR(32) NOT NULL,
  `CreatedAtUtc` DATETIME(6) NOT NULL,
  `LastUpdateUtc` DATETIME(6) NOT NULL,
  CONSTRAINT `PK_Apps` PRIMARY KEY (`Id`),
  CONSTRAINT `FK_Apps_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
  INDEX `IX_Apps_UserId` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Subscriptions` (
  `Id` CHAR(36) NOT NULL,
  `UserId` CHAR(36) NOT NULL,
  `Plan` VARCHAR(16) NOT NULL,
  `PriceMonthly` DECIMAL(18,2) NOT NULL,
  `StartedAtUtc` DATETIME(6) NOT NULL,
  `Status` VARCHAR(16) NOT NULL,
  CONSTRAINT `PK_Subscriptions` PRIMARY KEY (`Id`),
  CONSTRAINT `FK_Subscriptions_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
  INDEX `IX_Subscriptions_UserId` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS `Deployments` (
  `Id` CHAR(36) NOT NULL,
  `UserId` CHAR(36) NOT NULL,
  `AppId` CHAR(36) NOT NULL,
  `Environment` VARCHAR(16) NOT NULL,
  `Status` VARCHAR(16) NOT NULL,
  `TimestampUtc` DATETIME(6) NOT NULL,
  `DurationSeconds` INT NOT NULL,
  CONSTRAINT `PK_Deployments` PRIMARY KEY (`Id`),
  CONSTRAINT `FK_Deployments_Apps_AppId` FOREIGN KEY (`AppId`) REFERENCES `Apps` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_Deployments_Users_UserId` FOREIGN KEY (`UserId`) REFERENCES `Users` (`Id`) ON DELETE CASCADE,
  INDEX `IX_Deployments_AppId` (`AppId`),
  INDEX `IX_Deployments_UserId` (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
