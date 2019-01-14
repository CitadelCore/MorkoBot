#DROP DATABASE city27;

CREATE DATABASE city27;
USE city27;

CREATE TABLE multiverses (
	PRIMARY KEY(MultiverseId),
    MultiverseId BIGINT NOT NULL AUTO_INCREMENT,
    MultiverseName CHAR(32) NOT NULL,
    
    GuildId BIGINT NOT NULL
);

CREATE TABLE realities (
	PRIMARY KEY(RealityId),
    RealityId BIGINT NOT NULL AUTO_INCREMENT,
    RealityName CHAR(32) NOT NULL,
    
    FOREIGN KEY(MultiverseId) REFERENCES multiverses(MultiverseId),
    MultiverseId BIGINT NOT NULL
);

CREATE TABLE sessions (
	PRIMARY KEY(SessionId),
    SessionId BIGINT NOT NULL AUTO_INCREMENT,
    
    StartTime DATETIME,
    EndTime DATETIME,
    Active BIT,
    Paused BIT,
    
    FOREIGN KEY(MultiverseId) REFERENCES multiverses(MultiverseId),
    MultiverseId BIGINT NOT NULL
);

CREATE TABLE characters (
	PRIMARY KEY(CharacterId),
    CharacterId BIGINT NOT NULL AUTO_INCREMENT,
    OwnerId BIGINT NOT NULL,
    PlayerId BIGINT NOT NULL,
    
    Name CHAR(128) NOT NULL,
    Nickname CHAR(128) NOT NULL,
    Age INT NOT NULL,
    Gender TINYINT NOT NULL,
    
    Appearance TEXT(4096),
    Personality TEXT(4096),
    Backstory TEXT(4096),
    
    GuildId BIGINT NOT NULL
);

CREATE TABLE sessionparticipants (
	FOREIGN KEY(CharacterId) REFERENCES characters(CharacterId),
	CharacterId BIGINT NOT NULL,
    FOREIGN KEY(SessionId) REFERENCES sessions(SessionId),
    SessionId BIGINT NOT NULL
);