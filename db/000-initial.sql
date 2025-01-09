/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

CREATE DATABASE IF NOT EXISTS `ttc_aalst` /*!40100 DEFAULT CHARACTER SET utf8 */;
USE `ttc_aalst`;

CREATE TABLE IF NOT EXISTS `club` (
  `ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `Naam` varchar(50) NOT NULL DEFAULT '',
  `CodeVTTL` varchar(10) DEFAULT NULL,
  `Actief` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `Douche` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `Website` varchar(255) DEFAULT NULL,
  `CodeSporta` varchar(10) DEFAULT NULL,
  PRIMARY KEY (`ID`)
) ENGINE=MyISAM AUTO_INCREMENT=85 DEFAULT CHARSET=latin1;

INSERT INTO `club` (`ID`, `Naam`, `CodeVTTL`, `Actief`, `Douche`, `Website`, `CodeSporta`) VALUES
	(1, 'Aalst', 'OVL134', 1, 0, NULL, '4055');

CREATE TABLE IF NOT EXISTS `clubcontact` (
  `ClubID` int(10) unsigned NOT NULL DEFAULT '0',
  `SpelerID` int(10) unsigned NOT NULL DEFAULT '0',
  `Omschrijving` varchar(100) NOT NULL DEFAULT '',
  `Sortering` tinyint(3) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`ClubID`,`SpelerID`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;


CREATE TABLE IF NOT EXISTS `clublokaal` (
  `ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `ClubID` int(10) unsigned NOT NULL DEFAULT '0',
  `Lokaal` varchar(250) DEFAULT NULL,
  `Adres` varchar(250) DEFAULT NULL,
  `Gemeente` varchar(250) DEFAULT NULL,
  `Hoofd` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `Postcode` int(10) unsigned NOT NULL DEFAULT '0',
  `Telefoon` varchar(50) NOT NULL DEFAULT '',
  PRIMARY KEY (`ID`),
  KEY `IX_ClubId` (`ClubID`) USING HASH
) ENGINE=MyISAM AUTO_INCREMENT=65 DEFAULT CHARSET=latin1;

/*!40000 ALTER TABLE `clublokaal` DISABLE KEYS */;
INSERT INTO `clublokaal` (`ID`, `ClubID`, `Lokaal`, `Adres`, `Gemeente`, `Hoofd`, `Postcode`, `Telefoon`) VALUES
	(1, 1, 'KTA Technigo Sporthal-De Voorstad', 'Cesar Haeltermanstraat 71', 'Aalst', 1, 9300, '');
/*!40000 ALTER TABLE `clublokaal` ENABLE KEYS */;

CREATE TABLE IF NOT EXISTS `matchcomment` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `PostedOn` datetime NOT NULL,
  `Text` longtext,
  `MatchId` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `Hidden` tinyint(1) NOT NULL,
  `ImageUrl` varchar(100) CHARACTER SET utf8 DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_MatchId` (`MatchId`) USING HASH,
  CONSTRAINT `FK_matchcomment_match_MatchId` FOREIGN KEY (`MatchId`) REFERENCES `matches` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1835 DEFAULT CHARSET=latin1;

CREATE TABLE IF NOT EXISTS `matches` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Date` datetime NOT NULL,
  `Week` int(11) NOT NULL,
  `FrenoyMatchId` varchar(20) CHARACTER SET utf8 DEFAULT NULL,
  `HomeTeamId` int(11) DEFAULT NULL,
  `HomeClubId` int(11) NOT NULL,
  `HomeTeamCode` varchar(2) CHARACTER SET utf8 DEFAULT NULL,
  `AwayTeamId` int(11) DEFAULT NULL,
  `AwayClubId` int(11) NOT NULL,
  `ReportPlayerId` int(11) NOT NULL,
  `Description` longtext,
  `HomeScore` int(11) DEFAULT NULL,
  `AwayScore` int(11) DEFAULT NULL,
  `WalkOver` tinyint(1) NOT NULL,
  `IsSyncedWithFrenoy` tinyint(1) NOT NULL,
  `FrenoyDivisionId` int(11) NOT NULL,
  `FrenoySeason` int(11) NOT NULL,
  `Competition` int(11) NOT NULL,
  `AwayTeamCode` varchar(2) CHARACTER SET utf8 DEFAULT NULL,
  `Block` varchar(200) CHARACTER SET utf8 DEFAULT NULL,
  `FormationComment` longtext,
  `ShouldBePlayed` tinyint(1) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_HomeTeamId` (`HomeTeamId`) USING HASH,
  KEY `IX_AwayTeamId` (`AwayTeamId`) USING HASH,
  KEY `Date` (`Date`),
  KEY `HomeClubId` (`HomeClubId`),
  KEY `AwayClubId` (`AwayClubId`),
  KEY `IX_Date` (`Date`) USING HASH,
  CONSTRAINT `FK_match_team_AwayTeamId` FOREIGN KEY (`AwayTeamId`) REFERENCES `team` (`Id`),
  CONSTRAINT `FK_match_team_HomeTeamId` FOREIGN KEY (`HomeTeamId`) REFERENCES `team` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=73695 DEFAULT CHARSET=latin1;


CREATE TABLE IF NOT EXISTS `matchgame` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `MatchId` int(11) NOT NULL,
  `MatchNumber` int(11) NOT NULL,
  `HomePlayerUniqueIndex` int(11) NOT NULL,
  `HomePlayerSets` int(11) NOT NULL,
  `AwayPlayerUniqueIndex` int(11) NOT NULL,
  `AwayPlayerSets` int(11) NOT NULL,
  `WalkOver` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_MatchId` (`MatchId`) USING HASH,
  CONSTRAINT `FK_matchgame_match_MatchId` FOREIGN KEY (`MatchId`) REFERENCES `matches` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=716145 DEFAULT CHARSET=latin1;

CREATE TABLE IF NOT EXISTS `matchplayer` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `MatchId` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `Won` int(11) DEFAULT NULL,
  `Home` tinyint(1) NOT NULL,
  `Position` int(11) NOT NULL,
  `Name` varchar(50) CHARACTER SET utf8 DEFAULT NULL,
  `Ranking` varchar(5) CHARACTER SET utf8 DEFAULT NULL,
  `UniqueIndex` int(11) NOT NULL,
  `Status` varchar(10) CHARACTER SET utf8 DEFAULT NULL,
  `StatusNote` varchar(300) CHARACTER SET utf8 DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_MatchId` (`MatchId`) USING HASH,
  KEY `IX_PlayerId` (`PlayerId`) USING HASH,
  CONSTRAINT `FK_matchplayer_match_MatchId` FOREIGN KEY (`MatchId`) REFERENCES `matches` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=456814 DEFAULT CHARSET=latin1;

CREATE TABLE IF NOT EXISTS `parameter` (
  `sleutel` varchar(20) NOT NULL DEFAULT '0',
  `value` varchar(255) NOT NULL DEFAULT '0',
  `omschrijving` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`sleutel`)
) ENGINE=MyISAM DEFAULT CHARSET=latin1;

/*!40000 ALTER TABLE `parameter` DISABLE KEYS */;
INSERT INTO `parameter` (`sleutel`, `value`, `omschrijving`) VALUES
	('email', 'info@ttc.be', 'Email club'),
	('year', '2019', 'Huidig speeljaar'),
	('endOfSeason', 'false', NULL),
	('frenoy_password', '', 'Paswoord voor synchronisatie met Frenoy'),
	('frenoy_login', '', 'Login voor synchronisatie met Frenoy'),
	('trainingDays', 'Vrije Training: ma. en woe. tussen 19u30 en 22u30', 'Trainings dagen/tijdstippen'),
	('competitionDays', 'Competitie: Maandag en vrijdag om 20u', 'Competitie dagen/tijdstippen'),
	('adultMembership', '€125 voor volwassenen (Competitie)', 'Lidgeld voor volwassenen'),
	('youthMembership', '€75 voor  jeugdspelers (-18 jarigen)', 'Lidgeld voor -18 jarigen'),
	('frenoyClubIdVttl', 'OVL134', 'Frenoy ClubId'),
	('SendGridApiKey', '', ''),
	('FromEmail', 'info@ttc.be', ''),
	('googleMapsUrl', 'https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d2513.815157055805!2d4.0235603157494!3d50.94563137954687!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x47c3bd3c04b709a9%3A0x867b1c17caea9b21!2sTTC+Aalst!5e0!3m2!1sen!2sbe!4v1546798590314', NULL),
	('location', 'Sportzaal Technigo ("De Voorstadt"), 1e verdieping,  Cesar Haeltermansstraat 71, 9300 Aalst', NULL),
	('clubBankNr', 'BE55 0016 5927 6744', NULL),
	('clubOrgNr', 'BE 0840.545.283', NULL),
	('compBalls', 'VICTAS - VP40+ 3 STARS BALLS ', NULL),
	('frenoyClubIdSporta', '4055', NULL),
	('additionalMembership', '', NULL),
	('recreationalMembers', '€75 voor recreanten', NULL);
/*!40000 ALTER TABLE `parameter` ENABLE KEYS */;

CREATE TABLE IF NOT EXISTS `playerpasswordresetentity` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Guid` char(36) CHARACTER SET utf8 COLLATE utf8_bin NOT NULL DEFAULT '',
  `ExpiresOn` datetime NOT NULL,
  `PlayerId` int(11) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=604 DEFAULT CHARSET=utf8;


CREATE TABLE IF NOT EXISTS `speler` (
  `ID` int(10) unsigned NOT NULL AUTO_INCREMENT,
  `LinkKaartVTTL` varchar(250) DEFAULT NULL,
  `KlassementVTTL` varchar(5) DEFAULT NULL,
  `KlassementSporta` varchar(5) DEFAULT NULL,
  `Stijl` varchar(50) DEFAULT NULL,
  `BesteSlag` varchar(200) DEFAULT NULL,
  `ComputerNummerVTTL` int(11) DEFAULT NULL,
  `Adres` varchar(250) DEFAULT NULL,
  `Gemeente` varchar(250) DEFAULT NULL,
  `GSM` varchar(20) DEFAULT NULL,
  `Email` varchar(250) DEFAULT NULL,
  `Paswoord` varchar(32) DEFAULT NULL,
  `ClubIdVTTL` int(10) unsigned DEFAULT NULL,
  `ClubIdSporta` int(10) unsigned DEFAULT NULL,
  `NaamKort` varchar(20) DEFAULT NULL,
  `VolgnummerVTTL` tinyint(3) unsigned DEFAULT NULL,
  `IndexVTTL` tinyint(3) unsigned DEFAULT NULL,
  `LidNummerSporta` int(11) DEFAULT NULL,
  `VolgnummerSporta` tinyint(3) unsigned DEFAULT NULL,
  `IndexSporta` tinyint(3) unsigned DEFAULT NULL,
  `Gestopt` int(4) unsigned DEFAULT NULL,
  `Toegang` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `LinkKaartSporta` varchar(20) DEFAULT NULL,
  `HasKey` tinyint(1) DEFAULT NULL,
  `FirstName` varchar(100) CHARACTER SET utf8 DEFAULT NULL,
  `LastName` varchar(100) CHARACTER SET utf8 DEFAULT NULL,
  `NextKlassementVttl` longtext,
  `NextKlassementSporta` longtext,
  PRIMARY KEY (`ID`)
) ENGINE=MyISAM AUTO_INCREMENT=725 DEFAULT CHARSET=latin1;


CREATE TABLE IF NOT EXISTS `team` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Competition` varchar(10) CHARACTER SET utf8 DEFAULT NULL,
  `Reeks` varchar(2) CHARACTER SET utf8 DEFAULT NULL,
  `ReeksType` varchar(10) CHARACTER SET utf8 DEFAULT NULL,
  `ReeksCode` varchar(2) CHARACTER SET utf8 DEFAULT NULL,
  `Year` int(11) NOT NULL,
  `LinkId` varchar(10) CHARACTER SET utf8 DEFAULT NULL,
  `FrenoyTeamId` varchar(10) CHARACTER SET utf8 DEFAULT NULL,
  `FrenoyDivisionId` int(11) NOT NULL,
  `TeamCode` varchar(2) CHARACTER SET utf8 DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=425 DEFAULT CHARSET=latin1;


CREATE TABLE IF NOT EXISTS `teamopponent` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `TeamId` int(11) NOT NULL,
  `ClubId` int(11) NOT NULL,
  `TeamCode` varchar(2) CHARACTER SET utf8 DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_TeamId` (`TeamId`) USING HASH,
  KEY `IX_ClubId` (`ClubId`) USING HASH,
  CONSTRAINT `FK_teamopponent_team_TeamId` FOREIGN KEY (`TeamId`) REFERENCES `team` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=4255 DEFAULT CHARSET=latin1;


CREATE TABLE IF NOT EXISTS `teamplayer` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `PlayerType` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `TeamId` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_SpelerId` (`PlayerId`) USING HASH,
  KEY `IX_TeamId` (`TeamId`) USING HASH,
  CONSTRAINT `FK_teamplayer_team_TeamId` FOREIGN KEY (`TeamId`) REFERENCES `team` (`Id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=7355 DEFAULT CHARSET=latin1;

/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IF(@OLD_FOREIGN_KEY_CHECKS IS NULL, 1, @OLD_FOREIGN_KEY_CHECKS) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;
