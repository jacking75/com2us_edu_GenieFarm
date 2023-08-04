-- MySQL dump 10.13  Distrib 8.0.33, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: farm_db
-- ------------------------------------------------------
-- Server version	8.0.33

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `auction_info`
--

DROP TABLE IF EXISTS `auction_info`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `auction_info` (
  `AuctionId` bigint NOT NULL AUTO_INCREMENT,
  `SellerId` bigint NOT NULL,
  `ItemName` varchar(45) NOT NULL,
  `ItemId` bigint NOT NULL,
  `ItemCode` bigint NOT NULL,
  `ItemCount` bigint NOT NULL,
  `TypeCode` smallint NOT NULL,
  `BidderId` bigint NOT NULL DEFAULT '0',
  `CurBidPrice` bigint NOT NULL,
  `BuyNowPrice` bigint NOT NULL,
  `ExpiredAt` datetime NOT NULL DEFAULT ((now() + interval 1 day)),
  `IsPurchased` tinyint NOT NULL DEFAULT '0',
  PRIMARY KEY (`AuctionId`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `auction_info`
--

LOCK TABLES `auction_info` WRITE;
/*!40000 ALTER TABLE `auction_info` DISABLE KEYS */;
INSERT INTO `auction_info` VALUES (1,1,'벼',1,1,100,1,63,11,20,'2023-08-04 11:54:28',1),(5,63,'벼',1,1,100,1,63,105,200,'2023-08-05 14:58:04',1),(6,63,'병아리 모이',15,7,30,1,0,100,200,'2023-08-05 15:08:15',1),(7,63,'기본 삽',17,5,1,3,0,100,200,'2023-08-05 15:08:38',1),(8,63,'벼',1,1,100,1,63,1001,2000,'2023-08-05 16:26:28',0);
/*!40000 ALTER TABLE `auction_info` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `farm_info`
--

DROP TABLE IF EXISTS `farm_info`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `farm_info` (
  `UserId` bigint NOT NULL,
  `FarmLevel` smallint NOT NULL DEFAULT '1',
  `FarmExp` bigint NOT NULL DEFAULT '0',
  `Money` bigint NOT NULL DEFAULT '0',
  `MaxStorage` smallint NOT NULL,
  `Love` smallint NOT NULL,
  PRIMARY KEY (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `farm_info`
--

LOCK TABLES `farm_info` WRITE;
/*!40000 ALTER TABLE `farm_info` DISABLE KEYS */;
INSERT INTO `farm_info` VALUES (63,1,0,2338,100,5);
/*!40000 ALTER TABLE `farm_info` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `mail_info`
--

DROP TABLE IF EXISTS `mail_info`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `mail_info` (
  `MailId` bigint NOT NULL AUTO_INCREMENT,
  `ReceiverId` bigint NOT NULL,
  `SenderId` bigint NOT NULL,
  `Title` varchar(100) NOT NULL,
  `Content` varchar(2000) NOT NULL,
  `ObtainedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ExpiredAt` datetime NOT NULL,
  `IsRead` tinyint NOT NULL DEFAULT '0',
  `IsDeleted` tinyint NOT NULL DEFAULT '0',
  `ItemCode` bigint NOT NULL,
  `ItemCount` smallint NOT NULL,
  `IsReceived` tinyint NOT NULL DEFAULT '0',
  `Money` bigint DEFAULT NULL,
  PRIMARY KEY (`MailId`)
) ENGINE=InnoDB AUTO_INCREMENT=99 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `mail_info`
--

LOCK TABLES `mail_info` WRITE;
/*!40000 ALTER TABLE `mail_info` DISABLE KEYS */;
/*!40000 ALTER TABLE `mail_info` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user_attendance`
--

DROP TABLE IF EXISTS `user_attendance`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_attendance` (
  `UserId` bigint NOT NULL,
  `AttendanceCount` tinyint NOT NULL DEFAULT '0',
  `LastAttendance` datetime DEFAULT NULL,
  PRIMARY KEY (`UserId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_attendance`
--

LOCK TABLES `user_attendance` WRITE;
/*!40000 ALTER TABLE `user_attendance` DISABLE KEYS */;
INSERT INTO `user_attendance` VALUES (63,0,NULL);
/*!40000 ALTER TABLE `user_attendance` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user_basicinfo`
--

DROP TABLE IF EXISTS `user_basicinfo`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_basicinfo` (
  `UserId` bigint NOT NULL AUTO_INCREMENT,
  `PlayerId` varchar(14) NOT NULL,
  `Nickname` varchar(10) NOT NULL,
  `LastLoginAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `PassEndDate` datetime DEFAULT NULL,
  PRIMARY KEY (`UserId`),
  UNIQUE KEY `Nickname_UNIQUE` (`Nickname`),
  UNIQUE KEY `AuthId_UNIQUE` (`PlayerId`)
) ENGINE=InnoDB AUTO_INCREMENT=64 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_basicinfo`
--

LOCK TABLES `user_basicinfo` WRITE;
/*!40000 ALTER TABLE `user_basicinfo` DISABLE KEYS */;
INSERT INTO `user_basicinfo` VALUES (63,'test06','genie','2023-08-04 10:30:30',NULL);
/*!40000 ALTER TABLE `user_basicinfo` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `user_item`
--

DROP TABLE IF EXISTS `user_item`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `user_item` (
  `ItemId` bigint NOT NULL AUTO_INCREMENT,
  `UserId` bigint NOT NULL,
  `ItemCode` bigint NOT NULL,
  `ItemCount` smallint NOT NULL DEFAULT '1',
  PRIMARY KEY (`ItemId`)
) ENGINE=InnoDB AUTO_INCREMENT=19 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `user_item`
--

LOCK TABLES `user_item` WRITE;
/*!40000 ALTER TABLE `user_item` DISABLE KEYS */;
INSERT INTO `user_item` VALUES (15,63,7,30),(16,63,4,1),(17,63,5,1),(18,63,6,1);
/*!40000 ALTER TABLE `user_item` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2023-08-04 17:40:09
-- MySQL dump 10.13  Distrib 8.0.33, for Win64 (x86_64)
--
-- Host: 127.0.0.1    Database: master_db
-- ------------------------------------------------------
-- Server version	8.0.33

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `attendance_reward`
--

DROP TABLE IF EXISTS `attendance_reward`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `attendance_reward` (
  `Day` smallint NOT NULL,
  `ItemCode` bigint NOT NULL,
  `Money` int NOT NULL,
  `Count` smallint NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `attendance_reward`
--

LOCK TABLES `attendance_reward` WRITE;
/*!40000 ALTER TABLE `attendance_reward` DISABLE KEYS */;
INSERT INTO `attendance_reward` VALUES (1,1,0,30),(2,2,0,30),(3,0,500,0),(4,0,1000,0),(5,3,0,30),(6,1,0,30),(7,2,0,30),(8,0,500,0),(9,0,1000,0),(10,3,0,30),(11,1,0,30),(12,2,0,30),(13,0,500,0),(14,0,1000,0),(15,3,0,30),(16,1,0,30),(17,2,0,30),(18,0,500,0),(19,0,1000,0),(20,3,0,30),(21,1,0,30),(22,2,0,30),(23,0,500,0),(24,0,1000,0),(25,3,0,30),(26,1,0,30),(27,2,0,30),(28,0,10000,0);
/*!40000 ALTER TABLE `attendance_reward` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `farm_default`
--

DROP TABLE IF EXISTS `farm_default`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `farm_default` (
  `DefaultLevel` smallint NOT NULL DEFAULT '1',
  `DefaultMoney` bigint NOT NULL,
  `DefaultLove` smallint NOT NULL,
  `DefaultStorage` smallint NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `farm_default`
--

LOCK TABLES `farm_default` WRITE;
/*!40000 ALTER TABLE `farm_default` DISABLE KEYS */;
INSERT INTO `farm_default` VALUES (1,3000,5,100);
/*!40000 ALTER TABLE `farm_default` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `item_attribute`
--

DROP TABLE IF EXISTS `item_attribute`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `item_attribute` (
  `Code` bigint NOT NULL AUTO_INCREMENT,
  `TypeCode` smallint NOT NULL,
  `Name` varchar(30) NOT NULL,
  `SellPrice` bigint NOT NULL,
  `BuyPrice` bigint NOT NULL,
  `Desc` varchar(300) NOT NULL,
  PRIMARY KEY (`Code`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `item_attribute`
--

LOCK TABLES `item_attribute` WRITE;
/*!40000 ALTER TABLE `item_attribute` DISABLE KEYS */;
INSERT INTO `item_attribute` VALUES (1,1,'벼',2,2,'싱싱한 벼이다.'),(2,1,'볏짚',1,1,'싱싱한 벼를 말려 만들었다.'),(3,1,'우유',25,30,'싱싱한 우유다.'),(4,2,'기본 물뿌리개',0,100,'평범한 물뿌리개이다.'),(5,3,'기본 삽',0,100,'평범한 삽이다.'),(6,4,'기본 모자',0,100,'햇빛을 막아주는 평범한 모자다.'),(7,1,'병아리 모이',1,1,'병아리가 좋아하는 먹이이다.'),(8,1,'닭 모이',2,2,'닭이 좋아하는 먹이이다.');
/*!40000 ALTER TABLE `item_attribute` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `item_default`
--

DROP TABLE IF EXISTS `item_default`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `item_default` (
  `Code` bigint NOT NULL,
  `Count` smallint NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `item_default`
--

LOCK TABLES `item_default` WRITE;
/*!40000 ALTER TABLE `item_default` DISABLE KEYS */;
INSERT INTO `item_default` VALUES (7,30),(4,1),(5,1),(6,1);
/*!40000 ALTER TABLE `item_default` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `item_type`
--

DROP TABLE IF EXISTS `item_type`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `item_type` (
  `TypeCode` smallint NOT NULL AUTO_INCREMENT,
  `Name` varchar(10) NOT NULL,
  `Multiple` tinyint NOT NULL,
  `Consumable` tinyint NOT NULL,
  `Equipable` tinyint NOT NULL,
  PRIMARY KEY (`TypeCode`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `item_type`
--

LOCK TABLES `item_type` WRITE;
/*!40000 ALTER TABLE `item_type` DISABLE KEYS */;
INSERT INTO `item_type` VALUES (1,'소모품',1,1,0),(2,'물뿌리개',0,0,1),(3,'삽',0,0,1),(4,'모자',0,0,1);
/*!40000 ALTER TABLE `item_type` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `num_values`
--

DROP TABLE IF EXISTS `num_values`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `num_values` (
  `key` varchar(30) NOT NULL,
  `value` int NOT NULL,
  UNIQUE KEY `key` (`key`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `num_values`
--

LOCK TABLES `num_values` WRITE;
/*!40000 ALTER TABLE `num_values` DISABLE KEYS */;
INSERT INTO `num_values` VALUES ('AttendReward_Expiry',7),('AttendReward_SenderId',0),('Auction_Item_Count_Per_Page',10),('Mail_Count_Per_Page',10),('Max_Attendance_Count',10),('OwnerId_In_Mailbox',0),('Redis_LockTime',5),('Redis_Token_Expiry_Hour',10);
/*!40000 ALTER TABLE `num_values` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `version`
--

DROP TABLE IF EXISTS `version`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `version` (
  `AppVersion` varchar(10) NOT NULL,
  `MasterDataVersion` varchar(10) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `version`
--

LOCK TABLES `version` WRITE;
/*!40000 ALTER TABLE `version` DISABLE KEYS */;
INSERT INTO `version` VALUES ('0.1','0.1');
/*!40000 ALTER TABLE `version` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2023-08-04 17:40:09
