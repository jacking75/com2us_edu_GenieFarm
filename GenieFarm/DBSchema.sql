-- MySQL Workbench Forward Engineering

SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0;
SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0;
SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION';

-- -----------------------------------------------------
-- Schema mydb
-- -----------------------------------------------------
-- -----------------------------------------------------
-- Schema farm_db
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema farm_db
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `farm_db` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci ;
-- -----------------------------------------------------
-- Schema master_db
-- -----------------------------------------------------

-- -----------------------------------------------------
-- Schema master_db
-- -----------------------------------------------------
CREATE SCHEMA IF NOT EXISTS `master_db` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci ;
USE `farm_db` ;

-- -----------------------------------------------------
-- Table `farm_db`.`farm_info`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `farm_db`.`farm_info` (
  `UserId` BIGINT NOT NULL,
  `FarmLevel` SMALLINT NOT NULL DEFAULT '1',
  `FarmExp` BIGINT NOT NULL DEFAULT '0',
  `Money` BIGINT NOT NULL DEFAULT '0',
  `MaxStorage` SMALLINT NOT NULL, /* ���� ������ â�� �ִ� ĭ �� */
  `Love` SMALLINT NOT NULL,	/* ���� ������ ������ ���ٵ��� �� �ִ� Ƚ�� */
  PRIMARY KEY (`UserId`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `farm_db`.`farm_item`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `farm_db`.`farm_item` (
  `ItemId` BIGINT NOT NULL AUTO_INCREMENT,
  `OwnerId` BIGINT NOT NULL,
  `ItemCode` BIGINT NOT NULL,
  `ItemCount` SMALLINT NOT NULL DEFAULT '1',
  PRIMARY KEY (`ItemId`))
ENGINE = InnoDB
AUTO_INCREMENT = 15
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `farm_db`.`mail_info`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `farm_db`.`mail_info` (
  `MailId` BIGINT NOT NULL AUTO_INCREMENT,
  `ReceiverId` BIGINT NOT NULL,
  `SenderId` BIGINT NOT NULL,
  `Title` VARCHAR(100) NOT NULL,
  `Content` VARCHAR(2000) NOT NULL,
  `ObtainedAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ExpiredAt` DATETIME NOT NULL,
  `IsRead` TINYINT NOT NULL DEFAULT '0',
  `IsDeleted` TINYINT NOT NULL DEFAULT '0',
  `ItemId` BIGINT NOT NULL,
  `IsReceived` TINYINT NOT NULL DEFAULT '0',
  `Money` BIGINT NULL DEFAULT NULL,
  PRIMARY KEY (`MailId`))
ENGINE = InnoDB
AUTO_INCREMENT = 99
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `farm_db`.`user_attendance`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `farm_db`.`user_attendance` (
  `UserId` BIGINT NOT NULL,
  `AttendanceCount` TINYINT NOT NULL DEFAULT '0', /* ���� �⼮ �� */
  `LastAttendance` DATETIME NULL DEFAULT NULL, /* ���� �⼮�� */
  PRIMARY KEY (`UserId`))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `farm_db`.`user_basicinfo`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `farm_db`.`user_basicinfo` (
  `UserId` BIGINT NOT NULL AUTO_INCREMENT,
  `PlayerId` VARCHAR(14) NOT NULL,
  `Nickname` VARCHAR(10) NOT NULL,
  `LastLoginAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, /* ģ�� ��Ͽ��� ������ ���� ���ڸ� ǥ���ϱ� ���� ���� ���� �ð� */
  `PassEndDate` DATETIME NULL DEFAULT NULL, /* ���� �������� �������� */
  PRIMARY KEY (`UserId`),
  UNIQUE INDEX `Nickname_UNIQUE` (`Nickname` ASC) VISIBLE,
  UNIQUE INDEX `AuthId_UNIQUE` (`PlayerId` ASC) VISIBLE)
ENGINE = InnoDB
AUTO_INCREMENT = 63
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;

USE `master_db` ;

-- -----------------------------------------------------
-- Table `master_db`.`attendance_reward`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `master_db`.`attendance_reward` (
  `Day` SMALLINT NOT NULL, /* �������� �ִ� �������� */
  `ItemCode` BIGINT NOT NULL, /* ������ ����� ItemCode�� 0 */
  `Money` INT NOT NULL, /* ������ �������̶�� Money�� 0*/
  `Count` SMALLINT NOT NULL) /* ������ �������� ���, �������� ���� */
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `master_db`.`farm_default`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `master_db`.`farm_default` (
  `DefaultLevel` SMALLINT NOT NULL DEFAULT '1', /* ���� ���� �� �⺻ �����Ǵ� ���� �� */
  `DefaultMoney` BIGINT NOT NULL, /* �⺻ ��ȭ */
  `DefaultLove` SMALLINT NOT NULL, /* �⺻ Love �� */
  `DefaultStorage` SMALLINT NOT NULL) /* �⺻ â�� ũ�� */
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `master_db`.`item_attribute`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `master_db`.`item_attribute` (
  `Code` BIGINT NOT NULL AUTO_INCREMENT, /* ������ ���� ��ȣ */
  `TypeCode` SMALLINT NOT NULL, /* ������ Ÿ�� ��ȣ */
  `Name` VARCHAR(30) NOT NULL, /* ������ �̸� */
  `SellPrice` BIGINT NOT NULL, /* ������ �� �� ���� */
  `BuyPrice` BIGINT NOT NULL, /* �������� �� �� ���� */
  `Desc` VARCHAR(300) NOT NULL, /* ������ ���� */
  PRIMARY KEY (`Code`))
ENGINE = InnoDB
AUTO_INCREMENT = 9
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `master_db`.`item_default`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `master_db`.`item_default` (
  `Code` BIGINT NOT NULL, /* ���� ���� �� �⺻ ���޵Ǵ� �������� �ڵ� */
  `Count` SMALLINT NOT NULL) /* �⺻ ���޵Ǵ� ������ ���� */
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `master_db`.`item_type`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `master_db`.`item_type` (
  `TypeCode` SMALLINT NOT NULL AUTO_INCREMENT, /* ������ Ÿ�� �ĺ� ��ȣ */
  `Name` VARCHAR(10) NOT NULL, /* ������ Ÿ�� �̸� */
  `Multiple` TINYINT NOT NULL, /* â�� �� ĭ�� ���� ���� ���� �� �ִ��� */
  `Consumable` TINYINT NOT NULL, /* �Ҹ�ǰ ���� */
  `Equipable` TINYINT NOT NULL, /* ��� ���� */
  PRIMARY KEY (`TypeCode`))
ENGINE = InnoDB
AUTO_INCREMENT = 5
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `master_db`.`num_values`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `master_db`.`num_values` (
  `key` VARCHAR(30) NOT NULL, /* Key�� */
  `value` INT NOT NULL, /* Key�� �����ϴ� Value�� */
  UNIQUE INDEX `key` (`key` ASC))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `master_db`.`version`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `master_db`.`version` (
  `AppVersion` VARCHAR(10) NOT NULL, /* �� ���� */
  `MasterDataVersion` VARCHAR(10) NOT NULL) /* ������ ������ ���� */
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
