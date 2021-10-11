INSERT INTO Users VALUES ('kuku@kuku.com','kuku','kaka','1234');
GO

--scaffold-dbcontext "Server=localhost\sqlexpress;Database=CarpoolDB;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models –force

--<binding protocol="http" bindingInformation="*:13939:localhost" />
--<binding protocol="https" bindingInformation="*:44319:localhost" />
--<binding protocol="http" bindingInformation="*:13939:127.0.0.1" />
--<binding protocol="https" bindingInformation="*:44319:127.0.0.1" />

--CREATE
Use master
Create Database CarpoolDB
Go

Use CarpoolDB
Go

Create Table Users (
ID int Identity primary key,
Email nvarchar(100) not null,
UserName nvarchar(30) not null,
FirstName nvarchar(30) not null,
LastName nvarchar(30) not null,
UserPswd nvarchar(30) not null,
BirthDate datetime not null,
PhoneNum nvarchar(30) not null,
Photo nvarchar not null,
City nvarchar(30) not null,
Neighborhood nvarchar(30) not null,
Street nvarchar(30) not null,
HouseNum nvarchar(30) not null,

CONSTRAINT UC_Email UNIQUE(Email),
CONSTRAINT UC_UserName UNIQUE(UserName)
)
Go

CREATE TABLE Adults (
ID int Identity primary key REFERENCES Users(ID)
);
Go

CREATE TABLE Kids (
ID int Identity primary key REFERENCES Users(ID)
);
Go

CREATE TABLE KidsOfAdults
(
AdultID int not null FOREIGN KEY REFERENCES Adults(ID),
KidID int not null FOREIGN KEY REFERENCES Kids(ID)
);
Go

Create Table Activities (
ID int Identity primary key,
ActivityName nvarchar(30) not null,
StartTime datetime not null,
EndTime datetime not null,
City nvarchar(30) not null,
Neighborhood nvarchar(30) not null,
Street nvarchar(30) not null,
HouseNum int not null,
Recurring bit not null,
EntryCode nvarchar(30) not null,
AdultID int not null FOREIGN KEY REFERENCES Adults(ID),
)
Go

CREATE TABLE KidsInActivities (
KidID int not null FOREIGN KEY REFERENCES Kids(ID),
ActivityID int not null FOREIGN KEY REFERENCES Activities(ID)
);
Go

Create Table CarpoolStatus (
StatusID int Identity primary key,
StatusName nvarchar(30) not null,
);
Go

Create Table Carpools (
ID int Identity primary key,
AdultID int not null FOREIGN KEY REFERENCES Adults(ID),
CarpoolTime datetime not null,
Seats int not null,
CarpoolStatusID int not null FOREIGN KEY REFERENCES CarpoolStatus(StatusID),
ActivityID int not null FOREIGN KEY REFERENCES Activities(ID),
);
Go

CREATE TABLE KidsInCarpools (
KidID int not null FOREIGN KEY REFERENCES Kids(ID),
CarpoolID int not null FOREIGN KEY REFERENCES Carpools(ID)
);
Go

Create Table RequestCarpoolStatus (
RequestID int Identity primary key,
RequestName nvarchar(30) not null,
);
Go

CREATE TABLE RequestToJoinCarpool (
KidID int not null FOREIGN KEY REFERENCES Kids(ID),
CarpoolID int not null FOREIGN KEY REFERENCES Carpools(ID),
RequestStatusID int not null FOREIGN KEY REFERENCES RequestCarpoolStatus(RequestID)
);
Go

--INSERT
INSERT INTO CarpoolStatus VALUES ('NotStarted')
INSERT INTO CarpoolStatus VALUES ('InProcess')
INSERT INTO CarpoolStatus VALUES ('Ended')
Go

INSERT INTO RequestCarpoolStatus VALUES ('Accepted')
INSERT INTO RequestCarpoolStatus VALUES ('Declined')
INSERT INTO RequestCarpoolStatus VALUES ('New')
Go
