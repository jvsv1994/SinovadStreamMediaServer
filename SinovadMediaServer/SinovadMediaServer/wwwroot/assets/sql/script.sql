
/*DROP TABLE IN ORDER */

ALTER TABLE MenuOption_Role
DROP CONSTRAINT FK_MenuOption_Role_MenuOption_ID;

ALTER TABLE MenuOption_Role
DROP CONSTRAINT FK_MenuOption_Role_Role_ID;

DROP TABLE IF EXISTS MenuOption_Role;
DROP TABLE IF EXISTS MenuOption;
DROP TABLE IF EXISTS Account;
DROP TABLE IF EXISTS Role;
/*Role*/

CREATE TABLE Role (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  Title varchar(255) NOT NULL
);

SET IDENTITY_INSERT Role ON

INSERT INTO Role(ID,Title) VALUES (1,'Administrador'),(2,'End User');

Select * FROM Role;

/*Account*/

CREATE TABLE Account (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  FullName varchar(255) NOT NULL,
  Username varchar(255) NOT NULL,
  Password varchar(255) NOT NULL,
  Role_ID int  NOT NULL,
  CONSTRAINT FK_Role_ID
    FOREIGN KEY (Role_ID)
    REFERENCES Role (ID)
);

ALTER TABLE Account
Add FirstName varchar(1000) NULL;

ALTER TABLE Account
Add LastName varchar(1000) NULL;

ALTER TABLE Account
Add Email varchar(1000) NULL;

ALTER TABLE Account
Add Active BIT NOT NULL DEFAULT 0;

ALTER TABLE Account
Add ConfirmationEmailToken varchar(1000) NULL;

ALTER TABLE Account
Add ConfirmationEmailDate DATETIME NULL;

ALTER TABLE Account
Add CreationDate DATETIME  NOT NULL DEFAULT (GETDATE());

ALTER TABLE Account
Add LastUpdatingDate DATETIME  NOT NULL DEFAULT (GETDATE());

SET IDENTITY_INSERT Account OFF

INSERT INTO Account(FullName,Username,Password,Role_ID) VALUES
('JORGE VICTOR SOLIS VEGA','jvsv1994@gmail.com','1234',1);

Select * FROM Account;

/*RoleAccount*/
CREATE TABLE RoleAccount (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  Role_ID int  NOT NULL,
  Account_ID int  NOT NULL,
  CONSTRAINT FK_RoleAccount_Role_ID
    FOREIGN KEY (Role_ID)
    REFERENCES Role (ID),
  CONSTRAINT FK_RoleAccount_Account_ID
    FOREIGN KEY (Account_ID)
    REFERENCES Role (ID)
);


SET IDENTITY_INSERT RoleAccount ON

INSERT INTO RoleAccount(ID,Role_ID,Account_ID) VALUES (1,1,1);



/*MenuOption*/

DELETE FROM MenuOption;

CREATE TABLE MenuOption (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  Caption varchar(255) NOT NULL,
	Method varchar(255) NOT NULL,
  ParentOption_ID int NOT NULL
);

SET IDENTITY_INSERT MenuOption ON

INSERT INTO MenuOption(ID,Caption,Method,ParentOption_ID) VALUES
(1,'Login','ShowLogin',0),
(2,'Crear Cuenta','ShowCreateAccountForm',0),
(3,'Inicio','ShowInitialApp',0),
(4,'Animes','ShowAnimes',0),
(5,'Series','ShowSeries',0),
(6,'Peliculas','ShowMovies',0),
(7,'Mantenimiento','ShowManagementOptions',0),
(8,'Animes','ShowManagementAnimes',7),
(9,'Series','ShowManagementSeries',7),
(10,'Peliculas','ShowManagementMovies',7),
(11,'Genres','ShowManagementGenres',7),
(12,'TV','ShowTVChannels',0),
(13,'TV','ShowManagementTVChannels',7);
Select * FROM MenuOption;

/*MenuOption_Role*/

CREATE TABLE MenuOption_Role (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  Role_ID int  NOT NULL,
  MenuOption_ID int  NOT NULL,
  CONSTRAINT FK_MenuOption_Role_MenuOption_ID
    FOREIGN KEY (MenuOption_ID)
    REFERENCES MenuOption (ID),
  CONSTRAINT FK_MenuOption_Role_Role_ID
    FOREIGN KEY (Role_ID)
    REFERENCES Role (ID)
);

DELETE FROM MenuOption_Role;

SET IDENTITY_INSERT MenuOption_Role ON

INSERT INTO MenuOption_Role(ID,Role_ID,MenuOption_ID) VALUES
(1,1,3),
(2,1,4),
(3,1,5),
(4,1,6),
(5,1,7),
(6,1,8),
(7,1,9),
(8,1,10),
(9,1,11),
(10,2,1),
(11,2,2),
(12,2,3),
(13,2,4),
(14,2,5),
(15,2,6),
(16,3,3),
(17,3,4),
(18,3,5),
(19,3,6),
(20,1,12),
(21,2,12),
(22,3,12),
(23,1,13);
Select * FROM MenuOption_Role;

/*Genre*/

DELETE FROM Genre;

DROP TABLE IF EXISTS Genre;

CREATE TABLE Genre (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  Name varchar(255) NOT NULL
);

Select * FROM Genre;

ALTER TABLE Genre
Add TMDbID int NULL;

/*TvSerie*/

CREATE TABLE TvSerie (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  Name varchar(255) NOT NULL,
  TMDbID int NULL,
  OriginalLanguage varchar(1000)  NULL,
  OriginalName varchar(1000)  NULL,
  Overview varchar(1000)  NULL,
  Popularity float NULL,
  PosterPath varchar(1000) NULL,
  BackdropPath varchar(1000)  NULL,
  FirstAirDate DATETIME  NULL,
  LastAirDate DATETIME  NULL
);

ALTER TABLE TvSerie
Add Directors varchar(1000) NULL;

ALTER TABLE TvSerie
Add Actors varchar(1000) NULL;

/*TvSerieGenre*/
CREATE TABLE TvSerieGenre (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  TvSerie_ID int  NOT NULL,
  Genre_ID int  NOT NULL,
  CONSTRAINT FK_TvSerieGenre_TvSerie_ID
    FOREIGN KEY (TvSerie_ID)
    REFERENCES TvSerie (ID),
  CONSTRAINT FK_TvSerieGenre_Genre_ID
    FOREIGN KEY (Genre_ID)
    REFERENCES Genre (ID)
);

Select * FROM TvSerieGenre;

/*Season*/

DELETE FROM Season;

DROP TABLE IF EXISTS Season;

CREATE TABLE Season (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  SeasonNumber int NULL,
  Name varchar(1000) NULL,
  Summary varchar(1000) NULL,
  TvSerie_ID int NOT NULL,
  CONSTRAINT FK_Season_TvSerie_ID
  FOREIGN KEY (TvSerie_ID)
  REFERENCES TvSerie (ID)
);

Select * FROM Season;

/*Episode*/

DELETE FROM Episode;

ALTER TABLE Episode
DROP CONSTRAINT FK_Episode_Season_ID;

DROP TABLE IF EXISTS Episode;

CREATE TABLE Episode (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  EpisodeNumber int NULL,
  Title varchar(1000) NULL,
  Summary varchar(1000) NULL,
  Season_ID int NOT NULL,
  ImageUrl varchar(1000) NULL,
  CONSTRAINT FK_Episode_Season_ID
  FOREIGN KEY (Season_ID)
  REFERENCES Season (ID)
);

Select * FROM Episode;

/*Item*/

DELETE FROM FilmaffinityData;

DROP TABLE IF EXISTS FilmaffinityData;

CREATE TABLE FilmaffinityData (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  FilmaffinityID varchar(255) NOT NULL,
  Title varchar(8000) NULL,
  Lang varchar(8000) NULL,
  Link varchar(8000) NULL,
  Rating float NULL,
  Votes float NULL,
  Year int NULL,
  Country varchar(8000) NULL,
  Duration varchar(8000) NULL,
  Excerpt varchar(8000) NULL,
  Actors varchar(8000) NULL,
  Director varchar(8000) NULL,
  Genres varchar(8000) NULL,
  Images varchar(8000) NULL,
  Trailer varchar(8000) NULL
);


/*Account Server*/

CREATE TABLE AccountServer (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  IpAddress varchar(1000) NULL,
  Account_ID int NOT NULL,
  CONSTRAINT FK_AccountServer_Account_ID
    FOREIGN KEY (Account_ID)
    REFERENCES Account (ID)
);

ALTER TABLE AccountServer
Add State_Catalog_ID int NOT NULL Default 3,
State_Catalog_Detail_ID int NOT NULL Default 2,
HostUrl varchar(1000) NULL;


/*Account Storage*/

CREATE TABLE AccountStorage (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  AccountServer_ID int NOT NULL,
  PhisicalPath varchar(1000) NULL,
  AccountStorageType_ID int NOT NULL,
  CONSTRAINT FK_AccountStorage_AccountServer_ID
    FOREIGN KEY (AccountServer_ID)
    REFERENCES AccountServer (ID)
);


/*Movie*/

CREATE TABLE Movie (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  Title varchar(255) NOT NULL
  Adult BIT NULL DEFAULT 0,
  TMDbID int NULL,
  OriginalLanguage varchar(1000)  NULL,
  OriginalTitle varchar(1000)  NULL,
  Overview varchar(1000)  NULL,
  Popularity float NULL,
  PosterPath varchar(1000) NULL,
  BackdropPath varchar(1000)  NULL,
  ReleaseDate DATETIME  NULL
);

ALTER TABLE Movie
Add Directors varchar(1000) NULL;

ALTER TABLE Movie
Add Actors varchar(1000) NULL;

ALTER TABLE Movie
Add IMDBID varchar(1000) NULL;



/*MovieGenre*/
CREATE TABLE MovieGenre (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  Movie_ID int  NOT NULL,
  Genre_ID int  NOT NULL,
  CONSTRAINT FK_MovieGenre_Movie_ID
    FOREIGN KEY (Movie_ID)
    REFERENCES Movie (ID),
  CONSTRAINT FK_MovieGenre_Genre_ID
    FOREIGN KEY (Genre_ID)
    REFERENCES Genre (ID)
);

Select * FROM MovieGenre;


/*Profile*/

CREATE TABLE Profile (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  Account_ID int NOT NULL,
  FullName varchar(1000)  NULL,
  CONSTRAINT FK_Profile_Account_ID
    FOREIGN KEY (Account_ID)
    REFERENCES Account (ID)
);

ALTER TABLE Profile
Add Main BIT NOT NULL DEFAULT 0;

ALTER TABLE Profile
Add PINCode int NULL;

ALTER TABLE Profile
Add AvatarPath varchar(1000) NULL;

/*Video*/

CREATE TABLE Video (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  Title varchar(1000) NULL,
  PhysicalPath varchar(1000) NULL,
  AccountStorage_ID int NULL,
  Movie_ID int NULL,
  Episode_ID int NULL,
  TvSerie_ID int NULL,
  EpisodeNumber int NULL,
  SeasonNumber int NULL,
  CreationDate DATETIME  NOT NULL DEFAULT (GETDATE())
);

ALTER TABLE Video
Add Subtitle varchar(1000) NULL;

/*VideoProfile*/

CREATE TABLE VideoProfile (
    Video_ID int NOT null,
    Profile_ID int NOT null,
    DurationTime float NOT NULL ,
    CurrentTime float NOT NULL ,
    CreationDate DATETIME  NOT NULL DEFAULT (GETDATE()),
    LastUpdatingDate DATETIME  NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT PK_VideoProfile PRIMARY KEY (Video_ID, Profile_ID),
    CONSTRAINT FK_VideoProfileVideo FOREIGN KEY (Video_ID) REFERENCES Video(ID),
    CONSTRAINT FK_VideoProfileProfile FOREIGN KEY (Profile_ID) REFERENCES Profile(ID)
)

/*Catalog*/

CREATE TABLE Catalog (
  ID int NOT NULL PRIMARY KEY,
  Name varchar(1000) NOT NULL
);

/*CatalogDetail*/

CREATE TABLE CatalogDetail (
  Catalog_ID int NOT NULL,
  ID int NOT NULL,
  Name varchar(1000) NOT NULL,
  TextValue varchar(1000) NULL,
  NumberValue int NULL,
  PRIMARY KEY (Catalog_ID,ID),
  CONSTRAINT FK_CatalogDetail_Catalog_ID
    FOREIGN KEY (Catalog_ID)
    REFERENCES Catalog (ID)
);

INSERT INTO Catalog(ID,Name) VALUES (1,'Transmission method');
INSERT INTO Catalog(ID,Name) VALUES (2,'Preset');
INSERT INTO Catalog(ID,Name) VALUES (3,'Server state');

/* INSERT INTO Catalog(ID,Name) VALUES (3,'Container Position');
INSERT INTO Catalog(ID,Name) VALUES (4,'Screen Size');
INSERT INTO Catalog(ID,Name) VALUES (5,'Platform'); */

INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (1,1,'None');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (1,2,'MPEG-DASH');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (1,3,'HLS');

INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (2,1,'ultrafast');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (2,2,'superfast');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (2,3,'veryfast');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (2,4,'faster');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (2,5,'fast');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (2,6,'medium');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (2,7,'slow');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (2,8,'slower');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (2,9,'veryslow');

INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (3,1,'Started');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (3,2,'Stopped');

/*
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (3,1,'Left');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (3,2,'Center');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (3,3,'Right');

INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (4,1,'Small');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (4,2,'Medium');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (4,3,'Large');

INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (5,1,'Desktop');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (5,2,'Web');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (5,3,'Mobile');
INSERT INTO CatalogDetail(Catalog_ID,ID,Name) VALUES (5,4,'TV');
 */
/*TranscodeSettings*/

CREATE TABLE TranscodeSettings (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  AccountServer_ID int NOT NULL,
  Transmission_Method_Catalog_ID int NOT NULL,
  Transmission_Method_Catalog_Detail_ID int NOT NULL,
  Preset_Catalog_ID int NOT NULL,
  Preset_Catalog_Detail_ID int NOT NULL,
  DirectoryPhysicalPath varchar(1000) NULL,
  CONSTRAINT FK_TranscodeSettings_AccountServer_ID
    FOREIGN KEY (AccountServer_ID)
    REFERENCES AccountServer (ID)
);

ALTER TABLE TranscodeSettings
Add ConstantRateFactor int NOT NULL DEFAULT 0;


/*TranscodeVideoProcess*/

CREATE TABLE TranscodeVideoProcess (
  ID int NOT NULL PRIMARY KEY IDENTITY(1,1),
  GUID varchar(1000) NOT NULL,
  TranscodeAudioVideoProcessID int NOT NULL,
  TranscodeSubtitlesProcessID int NULL,
  WorkingDirectoryPath varchar(1000) NOT NULL,
  CreationDate DATETIME  NOT NULL DEFAULT (GETDATE()),
  AccountServer_ID int NOT NULL
);

