using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.SqlSettigns;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.SqlSettigns
{
	public class SqlTriggerAndTableManagerRepository : ISqlTriggerAndTableManagerService
	{
				public async Task CreateTableAndTrigger(string connectionString)
		{

			string directoryPath = "Configuration";
			string fileName = "appsettings.json";
			string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

			// Klasör yoksa oluştur
			if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
			{
				Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
			}

			// Dosya varsa oku
			string jsonData = "";
			if (File.Exists(filePath))
			{
				jsonData = File.ReadAllText(filePath);
			}

			// JSON verisini kontrol et ve gerekirse düzenle
			JObject jsonObject;

			jsonObject = JObject.Parse(jsonData);


			// Yeni veriyi JSON nesnesine ekle veya güncelle
			var scheman = jsonObject["DatabaseSchema"].ToString();

			string createTableSql = @"
    CREATE TABLE " + scheman + @".Change_Notice_LogTable (
     TransferID varchar(MAX),
      idA2A2 varchar(50),
      idA3masterReference varchar(MAX),
      statestate varchar(MAX),
      name varchar(MAX),
      WTPartNumber varchar(MAX),
      updateStampA2 datetime,
      ProcessTimestamp datetime,
 Version varchar(MAX),
 VersionID varchar(MAX),
ReviseDate datetime
    )";

			#region AlterneLinkLog için kullanılacak sql komutu

			string createTableSql2 = @"
                CREATE TABLE " + scheman + @".WTPartAlternateLink_LOG (
AnaParcaTransferID varchar(MAX),
AnaParcaID varchar(200),
AnaParcaNumber varchar(MAX),
AnaParcaName varchar(MAX),
TransferID varchar(MAX),
                 ID varchar(200),
                 ObjectType varchar(MAX),
                 Name varchar(MAX),
                 Number varchar(MAX),
                 updateStampA2 datetime,
            	  modifyStampA2 datetime,
                 ProcessTimestamp datetime,
                 state varchar(MAX)
                )";




			string createTableSql3 = @"
CREATE TABLE " + scheman + @".WTPartAlternateLink_ControlLog (
TransferID varchar(MAX),
  AdministrativeLockIsNull tinyint,
  TypeAdministrativeLock varchar(MAX),
  ClassNameKeyDomainRef varchar(MAX),
  IdA3DomainRef bigint,
  InheritedDomain tinyint,
  ReplacementType varchar(MAX),
  ClassNameKeyRoleAObjectRef varchar(MAX),
  IdA3A5 bigint,
  ClassNameKeyRoleBObjectRef varchar(MAX),
  IdA3B5 bigint,
  SecurityLabels varchar(MAX),
  CreateStampA2 datetime,
  MarkForDeleteA2 bigint,
  ModifyStampA2 datetime,
  ClassNameA2A2 varchar(MAX),
  IdA2A2 bigint,
  UpdateCountA2 int,
  UpdateStampA2 datetime
)
";



			string createTableSql4 = @"
    CREATE TABLE " + scheman + @".Ent_EPMDocState (
	[Ent_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[EPMDocID] [bigint] NULL,
	[StateDegeri] [nvarchar](200) NULL,
	[idA3masterReference] [bigint] NULL,
	[CadName] [nvarchar](200) NULL,
	[name] [nvarchar](200) NULL,
	[docNumber] [nvarchar](200) NULL,
	CONSTRAINT [PK_Ent_EPMDocState] PRIMARY KEY CLUSTERED ([Ent_ID] ASC)
);
";

			string Ent_EPMDocState_ERROR = @"
    CREATE TABLE " + scheman + @".Ent_EPMDocState_ERROR (
	[Ent_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[EPMDocID] [bigint] NULL,
	[StateDegeri] [nvarchar](200) NULL,
	[idA3masterReference] [bigint] NULL,
	[CadName] [nvarchar](200) NULL,
	[name] [nvarchar](200) NULL,
	[docNumber] [nvarchar](200) NULL,
	CONSTRAINT [PK_Ent_EPMDocState_ERROR] PRIMARY KEY CLUSTERED ([Ent_ID] ASC)
);
";


			string createTableTrigger4 = @$"
CREATE TRIGGER {scheman}.[EPMDokumanState]
ON {scheman}.[EPMDocument] 
AFTER UPDATE
AS 
BEGIN

    DECLARE @EPMDocumentID BIGINT,
			@idA3masterReference BIGINT,
		    @StateDegeri NVARCHAR(200),
			@CadName NVARCHAR(200),
			@name NVARCHAR(200),
			@docNumber NVARCHAR(200);

    SELECT @EPMDocumentID = idA2A2, @StateDegeri = statestate, @idA3masterReference = idA3masterReference FROM inserted;

	SELECT @CadName = CADName, @name = name, @docNumber = documentNumber FROM {scheman}.EPMDocumentMaster WHERE idA2A2 = @idA3masterReference;

    IF @StateDegeri = 'RELEASED'
    BEGIN
        IF EXISTS (SELECT 1 FROM {scheman}.EPMReferenceLink WHERE idA3A5 = @EPMDocumentID AND referenceType = 'DRAWING')
        BEGIN
            IF EXISTS (SELECT 1 FROM {scheman}.Ent_EPMDocState WHERE idA3masterReference = @idA3masterReference)
            BEGIN
                UPDATE {scheman}.Ent_EPMDocState
                SET StateDegeri = @StateDegeri,
                    EPMDocID = @EPMDocumentID,
                    CADName = @CadName,
                    name = @name,
                    docNumber = @docNumber
                WHERE idA3masterReference = @idA3masterReference;
            END
            ELSE
            BEGIN
                INSERT INTO {scheman}.Ent_EPMDocState (EPMDocID, StateDegeri, idA3masterReference, CADName, name, docNumber)
                VALUES (@EPMDocumentID, @StateDegeri, @idA3masterReference, @CadName, @name, @docNumber);
            END
        END
    END

END
";


			string createTableSql5 = @"
    CREATE TABLE " + scheman + @".Ent_EPMDocState_CANCELLED (
	[Ent_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[EPMDocID] [bigint] NULL,
	[StateDegeri] [nvarchar](200) NULL,
	[idA3masterReference] [bigint] NULL,
	[CadName] [nvarchar](200) NULL,
	[name] [nvarchar](200) NULL,
	[docNumber] [nvarchar](200) NULL,
	CONSTRAINT [PK_Ent_EPMDocState_CANCELLED] PRIMARY KEY CLUSTERED ([Ent_ID] ASC)
);
";

			string Ent_EPMDocState_CANCELLED_ERROR = @"
    CREATE TABLE " + scheman + @".Ent_EPMDocState_CANCELLED_ERROR (
	[Ent_ID] [bigint] IDENTITY(1,1) NOT NULL,
	[EPMDocID] [bigint] NULL,
	[StateDegeri] [nvarchar](200) NULL,
	[idA3masterReference] [bigint] NULL,
	[CadName] [nvarchar](200) NULL,
	[name] [nvarchar](200) NULL,
	[docNumber] [nvarchar](200) NULL,
	CONSTRAINT [PK_Ent_EPMDocState_CANCELLED_ERROR] PRIMARY KEY CLUSTERED ([Ent_ID] ASC)
);
";


			string createTableTrigger5 = @$"
CREATE TRIGGER {scheman}.[EPMDokumanState_CANCELLED]
ON {scheman}.[EPMDocument] 
AFTER UPDATE
AS 
BEGIN

    DECLARE @EPMDocumentID BIGINT,
			@idA3masterReference BIGINT,
		    @StateDegeri NVARCHAR(200),
			@CadName NVARCHAR(200),
			@name NVARCHAR(200),
			@docNumber NVARCHAR(200);

    SELECT @EPMDocumentID = idA2A2, @StateDegeri = statestate, @idA3masterReference = idA3masterReference FROM inserted;

	SELECT @CadName = CADName, @name = name, @docNumber = documentNumber FROM {scheman}.EPMDocumentMaster WHERE idA2A2 = @idA3masterReference;

    IF @StateDegeri = 'CANCELLED'
    BEGIN
        IF EXISTS (SELECT 1 FROM {scheman}.EPMReferenceLink WHERE idA3A5 = @EPMDocumentID AND referenceType = 'DRAWING')
        BEGIN
            IF EXISTS (SELECT 1 FROM {scheman}.Ent_EPMDocState_CANCELLED WHERE idA3masterReference = @idA3masterReference)
            BEGIN
                UPDATE {scheman}.Ent_EPMDocState_CANCELLED
                SET StateDegeri = @StateDegeri,
                    EPMDocID = @EPMDocumentID,
                    CADName = @CadName,
                    name = @name,
                    docNumber = @docNumber
                WHERE idA3masterReference = @idA3masterReference;
            END
            ELSE
            BEGIN
                INSERT INTO {scheman}.Ent_EPMDocState_CANCELLED (EPMDocID, StateDegeri, idA3masterReference, CADName, name, docNumber)
                VALUES (@EPMDocumentID, @StateDegeri, @idA3masterReference, @CadName, @name, @docNumber);
            END
        END
    END

END
";




			string Des_PartDocumentBagla = @$"
   CREATE TABLE {scheman}.Des_PartDocumentBagla (
	[PartDocID] [int] IDENTITY(1,1) NOT NULL,
	[WTDocumentTypeID] [int] NULL,
	[WTDocumentTypeName] [nvarchar](MAX) NULL,
	[Mesaj] [nvarchar](150) NULL,
	[EklemeDurumu] [tinyint] NULL,
	CONSTRAINT [PK_Des_PartDocumentBagla_1] PRIMARY KEY CLUSTERED ([PartDocID] ASC)
);
";

			string Des_PartDocumentBaglalOG = @$"
   CREATE TABLE {scheman}.Des_PartDokumanBaglaLog (
	[S_ID] [int] IDENTITY(1,1) NOT NULL,
	[Tarih] [datetime] NULL,
	[Part_ID] [bigint] NULL,
	[Part_Number] [nvarchar](50) NULL,
	[PartMasterID] [bigint] NULL,
	[DocType_ID] [bigint] NULL,
	[DocMaster_ID] [bigint] NULL,
	[DocNumber] [nvarchar](50) NULL,
	[Eklenti] [nvarchar](50) NULL,
	[GuncelDeger] [nvarchar](50) NULL,
	[EklemeDurumu] [tinyint] NULL,
	[Kontrol] [tinyint] NULL,
	[Kullanim] [tinyint] NULL,
	CONSTRAINT [PK_Des_PartDokumanBaglaLog] PRIMARY KEY CLUSTERED ([S_ID] ASC)
);
";


			string Part_DocumentTrigger = @$"
CREATE TRIGGER [{scheman}].[Part_Document]
ON [{scheman}].[WTPartReferenceLink]
AFTER INSERT
AS
BEGIN
    DECLARE @PartMasterID BIGINT,
			@PartID BIGINT,
			@Eklenti NVARCHAR(50),
			@EklemeDurumu TINYINT,
            @DocMaster_IDGecici BIGINT,
			@DocType_ID BIGINT,
			@DocMaster_ID BIGINT,
			@DocID BIGINT,
            @SonVersiyonHarf NVARCHAR(1),
            @SonVersiyonNo INT,
            @SonVersiyonPartID BIGINT,
            @idA3A5 BIGINT,
            @idA3B5 BIGINT,
            @idA2A2 BIGINT,
            @ToplamVeriSayisi INT,
            @Part_Number NVARCHAR(40),
			@EklentiSayisi NVARCHAR(40);

    -- En büyük DocMaster_ID değerini ve PartID'yi al
    SELECT TOP 1 @DocMaster_IDGecici = idA3B5, @PartID = idA3A5
    FROM INSERTED
    ORDER BY idA3B5 DESC;

	SELECT @DocType_ID=idA2typeDefinitionReference, @DocID=idA2A2 FROM [{scheman}].WTDocument WHERE idA3masterReference = @DocMaster_IDGecici
		   	 
	SELECT @Eklenti = WTDocumentTypeName, @EklemeDurumu = EklemeDurumu FROM [{scheman}].Des_PartDocumentBagla WHERE WTDocumentTypeID = @DocType_ID


    -- PartID'den PartMasterID'yi al
    SELECT @PartMasterID = idA3masterReference
    FROM [{scheman}].WTPart
    WHERE idA2A2 = @PartID;

    -- Son versiyon bilgilerini al (PartID kullanılarak)
    SELECT TOP 1 
        @SonVersiyonHarf = WT_FBI_COMPUTE_3_1, 
        @SonVersiyonNo = WT_FBI_COMPUTE_3_2,
        @SonVersiyonPartID = idA2A2
    FROM [{scheman}].WTPart
    WHERE idA2A2 = @PartID; -- @idA2A2 yerine @PartID kullanıldı

    -- PartNumber bilgisini al
    SELECT @Part_Number = WTPartNumber
    FROM [{scheman}].WTPartMaster
    WHERE idA2A2 = @PartMasterID;

    -- Eski versiyonlarda olup olmadığını kontrol et (idA3masterReference kullanarak)
    IF NOT EXISTS (
        SELECT 1
        FROM [{scheman}].[WTPartReferenceLink]
        WHERE idA3B5 = @DocMaster_IDGecici
        AND idA3A5 IN (
            SELECT idA2A2
            FROM [{scheman}].WTPart
            WHERE idA3masterReference = @PartMasterID
            AND NOT (WT_FBI_COMPUTE_3_1 = @SonVersiyonHarf AND WT_FBI_COMPUTE_3_2 = @SonVersiyonNo)
        )
    )
    BEGIN
        -- Log kaydını ekle 
  

        -- DocMaster_IDGecici değeri ile eşleşen tek bir kayıt olup olmadığını kontrol et
        IF (SELECT COUNT(*) FROM [{scheman}].[WTPartReferenceLink] WHERE idA3B5 = @DocMaster_IDGecici) = 1
        BEGIN
            -- Tek bir kayıt varsa, bilgileri al
            SELECT @idA3A5 = idA3A5, @idA3B5 = idA3B5, @idA2A2 = idA2A2 
            FROM [{scheman}].[WTPartReferenceLink] 
            WHERE idA3B5 = @DocMaster_IDGecici;

            -- Toplam veri sayısını al 
            SELECT @ToplamVeriSayisi = COUNT(*) 
            FROM [{scheman}].[WTPartReferenceLink]
            WHERE idA3A5 IN (SELECT idA2A2 FROM [{scheman}].WTPart WHERE idA3masterReference = @PartMasterID); 

			INSERT INTO [{scheman}].[Des_PartDokumanBaglaLog] (Part_ID,Part_Number,PartMasterID, DocMaster_ID,Eklenti)
			VALUES (@PartID,@Part_Number,@PartMasterID, @idA3B5,@Eklenti); 

			 --SELECT @EklentiSayisi = COUNT(*)
    --        FROM [{scheman}].[Des_PartDokumanBaglaLog]
    --        WHERE Part_Number = @Part_Number AND Eklenti = @Eklenti;

	 SELECT @EklentiSayisi = COUNT(*)
            FROM [{scheman}].[Des_PartDokumanBaglaLog]
            WHERE PartMasterID = @PartMasterID AND Eklenti = @Eklenti;

			IF @EklemeDurumu = 0
			BEGIN
            -- WTDocumentMaster tablosunu güncelle
            UPDATE [{scheman}].WTDocumentMaster
            SET WTDocumentNumber = @Part_Number + '_' + @Eklenti + '_' + CONVERT(nvarchar, @EklentiSayisi) 
            WHERE idA2A2 = @idA3B5; 
			END
			ELSE IF @EklemeDurumu = 1
			BEGIN
            -- WTDocumentMaster tablosunu güncelle
            UPDATE [{scheman}].WTDocumentMaster
            SET WTDocumentNumber =  @Eklenti + '_' + @Part_Number + '_' + CONVERT(nvarchar, @EklentiSayisi) 
            WHERE idA2A2 = @idA3B5; 
			END
	

        END
    END
END;
";




			//EPM

			string Des_CadDocumentBaglaLog = @"
CREATE TABLE " + scheman + @".Des_CadDocumentBaglaLog (
    [CadDocID] [int] IDENTITY(1,1) NOT NULL,
    [referenceType] [nvarchar](200) NULL,
    [classmakefyroleAObjectRef] [nvarchar](200) NULL,
    [idA3A5] [bigint] NULL,
    [classmakefyroleBObjectRef] [nvarchar](200) NULL,
    [idA3B5] [bigint] NULL,
    [idA2A2] [bigint] NULL,
    [branchIdA2typeDefinitionRefe] [bigint] NULL,
    [idA2typeDefinitionReference] [bigint] NULL,
    [idA3masterReference] [bigint] NULL,
    [mainDocumentNumber] [nvarchar](200) NULL,
    [mainName] [nvarchar](200) NULL,
    [mainIdA2A2] [bigint] NULL,
    [newDocumentNumber] [nvarchar](200) NULL,
    [newName] [nvarchar](200) NULL,
    [newIdA2A2] [bigint] NULL,
    CONSTRAINT [PK_Des_CadDocumentBaglaLog] PRIMARY KEY CLUSTERED ([CadDocID] ASC)
);
";

			string Des_CadDocumentBagla = @"
CREATE TABLE " + scheman + @".Des_CadDocumentBagla (
    [CadDocID] [int] IDENTITY(1,1) NOT NULL,
    [contentName] [nvarchar](200) NULL,
    [EklemeDurumu] [tinyint] NULL,
    [contentType] [tinyint] NULL,
    CONSTRAINT [PK_Des_CadDocumentBagla] PRIMARY KEY CLUSTERED ([CadDocID] ASC)
);
";

			string Des_EPMDocAttachmentsLog = $@"
CREATE TABLE [{scheman}].[Des_EPMDocAttachmentsLog](
	[empAttachID] [int] IDENTITY(1,1) NOT NULL,
	[serverName] [nvarchar](200) NULL,
	[databaseName] [nvarchar](200) NULL,
	[databaseSchemaName] [nvarchar](200) NULL,
	[databaseUsername] [nvarchar](200) NULL,
	[databasePassword] [nvarchar](200) NULL,
	[plmServerAdress] [nvarchar](100) NULL,
	[plmUserName] [nvarchar](200) NULL,
	[plmPassword] [nvarchar](200) NULL,
	[epmFileName] [nvarchar](200) NULL,
	[epmDocNumber] [nvarchar](200) NULL,
	[epmFileContent] [nvarchar](max) NULL,
CONSTRAINT [PK_Des_EPMDocAttachmentsLog] PRIMARY KEY CLUSTERED ([empAttachID] ASC)
);
";

			string EPM_DocumentTrigger = @$"
CREATE TRIGGER [{scheman}].[EPM_Document]
ON [{scheman}].[EPMReferenceLink]
AFTER INSERT
AS
BEGIN
    DECLARE @asStoredChildName NVARCHAR(200);
    DECLARE @depType INT;
    DECLARE @hasIBAValues TINYINT;
    DECLARE @referenceType NVARCHAR(200);
    DECLARE @required TINYINT;
    DECLARE @classmakefyroleAObjectRef NVARCHAR(200);
    DECLARE @idA3A5 BIGINT;
    DECLARE @classmakefyroleBObjectRef NVARCHAR(200);
    DECLARE @idA3B5 BIGINT;
    DECLARE @createStampA2 DATETIME2(7);
    DECLARE @markForDeleteA2 BIGINT;
    DECLARE @modifyStampA2 DATETIME2(7);
    DECLARE @classnameA2A2 NVARCHAR(200);
    DECLARE @idA2A2 BIGINT;
    DECLARE @updateCountA2 INT;
    DECLARE @updateStampA2 DATETIME2(7);
    DECLARE @branchIdA2typeDefinitionRefe BIGINT;
    DECLARE @idA2typeDefinitionReference BIGINT;
    DECLARE @uniqueLinkID BIGINT;
    DECLARE @uniqueNDId NVARCHAR(200);
    DECLARE @documentNumber NVARCHAR(200); 
    DECLARE @newDocumentNumber NVARCHAR(200);
    DECLARE @idA3masterReference BIGINT;
    DECLARE @name NVARCHAR(200);
    DECLARE @newName NVARCHAR(200);
    DECLARE @contentName NVARCHAR(200);
    DECLARE @contentType TINYINT;
    DECLARE @eklemeDurumu TINYINT;
    DECLARE @toplamSayi INT;
    DECLARE @newDocumentNumber2 NVARCHAR(200);

    SELECT 
        @asStoredChildName = i.asStoredChildName,
        @depType = i.depType,
        @hasIBAValues = i.hasIBAValues,
        @referenceType = i.referenceType,
        @required = i.required,
        @classmakefyroleAObjectRef = i.classnamekeyroleAObjectRef,
        @idA3A5 = i.idA3A5,
        @classmakefyroleBObjectRef = i.classnamekeyroleBObjectRef,
        @idA3B5 = i.idA3B5,
        @createStampA2 = i.createStampA2,
        @markForDeleteA2 = i.markForDeleteA2,
        @modifyStampA2 = i.modifyStampA2,
        @classnameA2A2 = i.classnameA2A2,
        @idA2A2 = i.idA2A2,
        @updateCountA2 = i.updateCountA2,
        @updateStampA2 = i.updateStampA2,
        @branchIdA2typeDefinitionRefe = i.branchIdA2typeDefinitionRefe,
        @idA2typeDefinitionReference = i.idA2typeDefinitionReference,
        @uniqueLinkID = i.uniqueLinkID,
        @uniqueNDId = i.uniqueNDId
    FROM inserted i;

    DECLARE @empResult INT = 0; 
	DECLARE @reviseControl INT = 0; 
    DECLARE @idA3masterReferenceControl NVARCHAR(200);
	    DECLARE @authoringApplicationControl NVARCHAR(MAX);
    SELECT 
        @idA3masterReferenceControl = idA3masterReference
    FROM 
        [{scheman}].EPMDocument
    WHERE 
        idA2A2 = @idA3A5;

    IF EXISTS (SELECT 1 FROM [{scheman}].EPMDocument WHERE idA3masterReference = @idA3masterReferenceControl AND statecheckoutInfo = 'wrk')
    BEGIN
        SET @empResult = 1;
    END
    ELSE
    BEGIN
        SET @empResult = ISNULL(@empResult, 2); 
    END

--Revise Control
	IF EXISTS (SELECT 1 FROM [{scheman}].EPMDocument WHERE idA2A2 = @idA3A5 AND versionIdA2versionInfo <> 'A')
BEGIN
    SET @reviseControl = 1;
END
ELSE
BEGIN
    SET @reviseControl = ISNULL(@reviseControl, 2); 
END;
--Revise Control

    IF @referenceType = 'DRAWING' AND @empResult = 0 AND @reviseControl = 0
    BEGIN
        SELECT 
            @documentNumber = documentNumber, 
            @name = name,
			@authoringApplicationControl = authoringApplication
        FROM 
            [{scheman}].EPMDocumentMaster
        WHERE 
            idA2A2 = @idA3B5;

        DECLARE @extensionPosition INT;
        SET @extensionPosition = LEN(@documentNumber) - CHARINDEX('.', REVERSE(@documentNumber)) + 1;




        -- Des_CadDocumentBagla tablosundan verileri çekiyoruz
        SELECT 
            @contentName = contentName,
            @contentType = contentType,
            @eklemeDurumu = EklemeDurumu
        FROM 
            [{scheman}].Des_CadDocumentBagla
        WHERE 
            contentType = 1;

        -- Toplam sayısını hesaplıyoruz
		SELECT 
    @toplamSayi = COUNT(*)
FROM 
    {scheman}.EPMReferenceLink x
JOIN 
    {scheman}.EPMDocument y ON x.idA3A5 = y.idA2A2
WHERE 
    x.idA3B5 = @idA3B5
    AND y.versionIdA2versionInfo = 'A';

        IF @contentType = 1
        BEGIN
            IF @extensionPosition > 0
            BEGIN
                IF @toplamSayi > 0
                BEGIN
                    SET @newDocumentNumber = 
                        SUBSTRING(@documentNumber, 1, @extensionPosition - 1) + 
                        @contentName +                                          
                        CAST(@toplamSayi AS NVARCHAR(50)) +                     
                        SUBSTRING(@documentNumber, @extensionPosition, LEN(@documentNumber));

                    SET @newDocumentNumber2 = 
                        SUBSTRING(@documentNumber, 1, @extensionPosition - 1) + 
                        @contentName +
                        CAST(@toplamSayi AS NVARCHAR(50)) + 
                        SUBSTRING(@documentNumber, @extensionPosition, LEN(@documentNumber));
                END
                ELSE
                BEGIN
                    SET @newDocumentNumber = 
                        SUBSTRING(@documentNumber, 1, @extensionPosition - 1) + 
                        @contentName +                                          
                        SUBSTRING(@documentNumber, @extensionPosition, LEN(@documentNumber));

                    SET @newDocumentNumber2 = 
                        SUBSTRING(@documentNumber, 1, @extensionPosition - 1) + 
                        @contentName + 
                        SUBSTRING(@documentNumber, @extensionPosition, LEN(@documentNumber));
                END
            END
            ELSE
            BEGIN
                IF @toplamSayi > 0
                BEGIN
                    SET @newDocumentNumber = @documentNumber  + @contentName + '_'  + CAST(@toplamSayi + 1 AS NVARCHAR(50)) + '.prt';
                    SET @newDocumentNumber2 = @documentNumber  + @contentName + '_'  + CAST(@toplamSayi + 1 AS NVARCHAR(50)) + '.prt';
                END
                ELSE
                BEGIN
                    SET @newDocumentNumber = @documentNumber  + @contentName + '.prt';
                    SET @newDocumentNumber2 = @documentNumber  + @contentName + '.prt';
                END
            END
        END
        ELSE
        BEGIN
            SET @newDocumentNumber = @documentNumber;
            SET @newDocumentNumber2 = @documentNumber;
        END

        SET @newName = @name + @contentName;
        IF @toplamSayi > 0
        BEGIN
            SET @newName = @newName + CAST(@toplamSayi + 1 AS NVARCHAR(50));
        END

        SELECT 
            @idA3masterReference = idA3masterReference
        FROM 
            [{scheman}].EPMDocument
        WHERE 
            idA2A2 = @idA3A5;



		IF @authoringApplicationControl = 'UG'
		BEGIN
	
		UPDATE 
    [{scheman}].EPMDocumentMaster
SET 
    documentNumber = @newDocumentNumber, 
    name = @name,
    CADName = CASE 
                WHEN RIGHT(@newDocumentNumber2, 4) = '.PRT' 
                THEN @newDocumentNumber2 
                ELSE @newDocumentNumber2 + '.PRT' 
              END
WHERE 
    idA2A2 = @idA3masterReference;
	END
	ELSE
	BEGIN

	        UPDATE 
            [{scheman}].EPMDocumentMaster
        SET 
            documentNumber = @newDocumentNumber, 
            name = @name,
            CADName = REPLACE(CADName,
            SUBSTRING(CADName, 1, CHARINDEX('.', CADName) - 1),
            SUBSTRING(@newDocumentNumber2, 1, CHARINDEX('.', @newDocumentNumber2) - 1)        
            )
        WHERE 
            idA2A2 = @idA3masterReference;
	END
	


        -- Log ekleme işlemini yapıyoruz
        INSERT INTO [{scheman}].Des_CadDocumentBaglaLog 
        (referenceType, classmakefyroleAObjectRef, idA3A5, classmakefyroleBObjectRef, idA3B5, idA2A2, 
         branchIdA2typeDefinitionRefe, idA2typeDefinitionReference, idA3masterReference, mainDocumentNumber, mainName, mainIdA2A2, 
         newDocumentNumber, newName, newIdA2A2)      
        VALUES 
        (
            @referenceType,
            @classmakefyroleAObjectRef,
            @idA3A5,
            @classmakefyroleBObjectRef,
            @idA3B5,
            @idA2A2,
            @branchIdA2typeDefinitionRefe,
            @idA2typeDefinitionReference,
            @idA3masterReference,
            @documentNumber,
            @name,
            @idA3B5,
            @newDocumentNumber,
            @newName,
            @idA3masterReference
        );
    END


END
";

			//EPM
			//KULLANICI AYARLARI

			string Des_Kullanici = @$"
   CREATE TABLE {scheman}.Des_Kullanici (
	[kullaniciID] [int] IDENTITY(1,1) NOT NULL,
	[kullanici] [nvarchar](50) NULL,
	[sifre] [nvarchar](max) NULL,
	[admin] [tinyint] NULL,
	[superAdmin] [tinyint] NULL,
    CONSTRAINT [PK_Des_Kullanici] PRIMARY KEY CLUSTERED ([kullaniciID] ASC)
);
";

			string Des_KulYetki = @$"
   CREATE TABLE {scheman}.Des_KulYetki (
    [yetkiID] [int] IDENTITY(1,1) NOT NULL,
    [kullaniciID] [int] NULL,
    [m1] [tinyint] NULL,
    [m2] [tinyint] NULL,
    [m3] [tinyint] NULL,
    [m4] [tinyint] NULL,
    [m5] [tinyint] NULL,
    [m6] [tinyint] NULL,
    [m7] [tinyint] NULL,
    [m8] [tinyint] NULL,
    [m9] [tinyint] NULL,
    [m10] [tinyint] NULL,
    [m11] [tinyint] NULL,
    [m12] [tinyint] NULL,
    [m13] [tinyint] NULL,
    CONSTRAINT [PK_Des_KulYetki] PRIMARY KEY CLUSTERED ([yetkiID] ASC)
);

-- Süper Admin kullanıcısını ekle 
INSERT INTO {scheman}.Des_Kullanici (kullanici, sifre, admin, superAdmin)
VALUES ('desSuperAdmin', 'AQAAAAIAAYagAAAAEAsIwLrCwmnHFq0fs4+7elwHSWLwOp373WDRrn++RDUUdYCHQ9iXuF7bM5uaAs6Vig==', 1, 1);

-- Süper Admin'in ID'sini al
DECLARE @superAdminID INT = (SELECT kullaniciID FROM {scheman}.Des_Kullanici WHERE kullanici = 'desSuperAdmin');

-- Süper Admin'e yetkileri atama
INSERT INTO {scheman}.Des_KulYetki (kullaniciID, m1, m2, m3, m4, m5, m6, m7, m8, m9, m10, m11, m12, m13)
VALUES (@superAdminID, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);
";
			//Şifre Des.23!Tech => AQAAAAIAAYagAAAAEAsIwLrCwmnHFq0fs4+7elwHSWLwOp373WDRrn++RDUUdYCHQ9iXuF7bM5uaAs6Vig==
			#endregion



			using (var connection = new SqlConnection(connectionString))
			{

				connection.Open();
				using (var command1 = new SqlCommand(createTableSql, connection))
				{
					try
					{
						command1.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}

				using (var command2 = new SqlCommand(createTableSql2, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}
				using (var command2 = new SqlCommand(createTableSql3, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}
				using (var command2 = new SqlCommand(createTableSql4, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}
				using (var command2 = new SqlCommand(createTableTrigger4, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}

				using (var command2 = new SqlCommand(createTableSql5, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}
				using (var command2 = new SqlCommand(createTableTrigger5, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}

				using (var command2 = new SqlCommand(Des_PartDocumentBagla, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}
				using (var command2 = new SqlCommand(Des_PartDocumentBaglalOG, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}
				using (var command2 = new SqlCommand(Part_DocumentTrigger, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}
				using (var command2 = new SqlCommand(Ent_EPMDocState_ERROR, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}
				using (var command2 = new SqlCommand(Ent_EPMDocState_CANCELLED_ERROR, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}

				#region EPMDocument Ayarları

				using (var command2 = new SqlCommand(Des_CadDocumentBagla, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}
				using (var command2 = new SqlCommand(Des_EPMDocAttachmentsLog, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}
				using (var command2 = new SqlCommand(Des_CadDocumentBaglaLog, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}
				using (var command2 = new SqlCommand(EPM_DocumentTrigger, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}
				#endregion

				#region Kullanıcı Ayarları

				using (var command2 = new SqlCommand(Des_Kullanici, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}
				using (var command2 = new SqlCommand(Des_KulYetki, connection))
				{
					try
					{
						command2.ExecuteNonQuery();
					}
					catch (Exception)
					{
					}
				}
				#endregion


			}
		}


	}
}
