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
  `MaxStorage` SMALLINT NOT NULL, /* 현재 유저의 창고 최대 칸 수 */
  `Love` SMALLINT NOT NULL,	/* 현재 유저가 동물을 쓰다듬을 수 있는 횟수 */
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
  `AttendanceCount` TINYINT NOT NULL DEFAULT '0', /* 누적 출석 수 */
  `LastAttendance` DATETIME NULL DEFAULT NULL, /* 최종 출석일 */
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
  `LastLoginAt` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP, /* 친구 목록에서 마지막 접속 일자를 표시하기 위한 최종 접속 시각 */
  `PassEndDate` DATETIME NULL DEFAULT NULL, /* 월간 구독제의 종료일자 */
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
  `Day` SMALLINT NOT NULL, /* 몇일차에 주는 보상인지 */
  `ItemCode` BIGINT NOT NULL, /* 보상이 골드라면 ItemCode는 0 */
  `Money` INT NOT NULL, /* 보상이 아이템이라면 Money는 0*/
  `Count` SMALLINT NOT NULL) /* 보상이 아이템일 경우, 아이템의 개수 */
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `master_db`.`farm_default`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `master_db`.`farm_default` (
  `DefaultLevel` SMALLINT NOT NULL DEFAULT '1', /* 최초 접속 시 기본 생성되는 레벨 값 */
  `DefaultMoney` BIGINT NOT NULL, /* 기본 재화 */
  `DefaultLove` SMALLINT NOT NULL, /* 기본 Love 수 */
  `DefaultStorage` SMALLINT NOT NULL) /* 기본 창고 크기 */
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `master_db`.`item_attribute`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `master_db`.`item_attribute` (
  `Code` BIGINT NOT NULL AUTO_INCREMENT, /* 아이템 고유 번호 */
  `TypeCode` SMALLINT NOT NULL, /* 아이템 타입 번호 */
  `Name` VARCHAR(30) NOT NULL, /* 아이템 이름 */
  `SellPrice` BIGINT NOT NULL, /* 상점에 팔 때 가격 */
  `BuyPrice` BIGINT NOT NULL, /* 상점에서 살 때 가격 */
  `Desc` VARCHAR(300) NOT NULL, /* 아이템 설명 */
  PRIMARY KEY (`Code`))
ENGINE = InnoDB
AUTO_INCREMENT = 9
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `master_db`.`item_default`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `master_db`.`item_default` (
  `Code` BIGINT NOT NULL, /* 최초 접속 시 기본 지급되는 아이템의 코드 */
  `Count` SMALLINT NOT NULL) /* 기본 지급되는 아이템 개수 */
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `master_db`.`item_type`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `master_db`.`item_type` (
  `TypeCode` SMALLINT NOT NULL AUTO_INCREMENT, /* 아이템 타입 식별 번호 */
  `Name` VARCHAR(10) NOT NULL, /* 아이템 타입 이름 */
  `Multiple` TINYINT NOT NULL, /* 창고 한 칸에 여러 개를 가질 수 있는지 */
  `Consumable` TINYINT NOT NULL, /* 소모품 여부 */
  `Equipable` TINYINT NOT NULL, /* 장비 여부 */
  PRIMARY KEY (`TypeCode`))
ENGINE = InnoDB
AUTO_INCREMENT = 5
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `master_db`.`num_values`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `master_db`.`num_values` (
  `key` VARCHAR(30) NOT NULL, /* Key값 */
  `value` INT NOT NULL, /* Key에 대응하는 Value값 */
  UNIQUE INDEX `key` (`key` ASC))
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


-- -----------------------------------------------------
-- Table `master_db`.`version`
-- -----------------------------------------------------
CREATE TABLE IF NOT EXISTS `master_db`.`version` (
  `AppVersion` VARCHAR(10) NOT NULL, /* 앱 버전 */
  `MasterDataVersion` VARCHAR(10) NOT NULL) /* 마스터 데이터 버전 */
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_0900_ai_ci;


SET SQL_MODE=@OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS;
SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS;
