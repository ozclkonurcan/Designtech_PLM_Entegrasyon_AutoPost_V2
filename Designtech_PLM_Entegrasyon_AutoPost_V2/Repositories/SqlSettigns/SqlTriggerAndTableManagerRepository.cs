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




			#region WTPart Bölümü
			#region WTPart Tables

			string Des_WTPart_LogTable = $@"CREATE TABLE [{scheman}].[Des_WTPart_LogTable](
	[LogID] [int] IDENTITY(1,1) NOT NULL,
	[ParcaState] [nvarchar](200) NULL,
	[ParcaPartID] [bigint] NULL,
	[ParcaPartMasterID] [bigint] NULL,
	[ParcaName] [nvarchar](150) NULL,
	[ParcaNumber] [nvarchar](max) NULL,
	[ParcaVersion] [nchar](30) NULL,
	[KulAd] [nvarchar](50) NULL,
	[LogDate] [datetime] NULL,
	[EntegrasyonDurum] [tinyint] NULL,
	[LogMesaj] [nvarchar](300) NULL,
 CONSTRAINT [PK_Des_WTPart_LogTable] PRIMARY KEY CLUSTERED ([LogID] ASC));";


            string Des_WTPart_LogTable_Error = $@"CREATE TABLE [{scheman}].[Des_WTPart_LogTable_Error](
	[LogID] [int] NOT NULL,
	[ParcaState] [nvarchar](200) NULL,
	[ParcaPartID] [bigint] NULL,
	[ParcaPartMasterID] [bigint] NULL,
	[ParcaName] [nvarchar](150) NULL,
	[ParcaNumber] [nvarchar](max) NULL,
	[ParcaVersion] [nchar](30) NULL,
	[KulAd] [nvarchar](50) NULL,
	[LogDate] [datetime] NULL,
	[EntegrasyonDurum] [tinyint] NULL,
	[LogMesaj] [nvarchar](300) NULL
)";
            string Des_WTPart_LogTable_Takip = $@"CREATE TABLE [{scheman}].[Des_WTPart_LogTable_Takip](
	[LogID] [int] IDENTITY(1,1) NOT NULL,
	[ParcaState] [nvarchar](200) NULL,
	[ParcaPartID] [bigint] NULL,
	[ParcaPartMasterID] [bigint] NULL,
	[ParcaName] [nvarchar](150) NULL,
	[ParcaNumber] [nvarchar](max) NULL,
	[ParcaVersion] [nchar](30) NULL,
	[KulAd] [nvarchar](50) NULL,
	[LogDate] [datetime] NULL,
	[EntegrasyonDurum] [tinyint] NULL,
	[LogMesaj] [nvarchar](300) NULL,
 CONSTRAINT [PK_Des_WTPart_LogTable_Takip] PRIMARY KEY CLUSTERED 
([LogID] ASC));";

			#endregion






			#region WTPart Trigger


			string Des_WTPart_LogTable_Error_Control = $@"
CREATE TRIGGER [{scheman}].[Des_WTPart_LogTable_Error_Control]
ON [{scheman}].[Des_WTPart_LogTable]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [{scheman}].[Des_WTPart_LogTable_Error]
    WHERE [ParcaPartID] IN (SELECT [ParcaPartID] FROM inserted);

END;
";

			string Part_Released = $@"
CREATE TRIGGER [{scheman}].[Part_Released]
ON [{scheman}].[WTPart]
AFTER INSERT, UPDATE
AS
BEGIN
    DECLARE @Part_ID BIGINT,                                                               
            @State NVARCHAR(200),
            @Number NVARCHAR(200),
            @ParcaAd NVARCHAR(200),
            @Versiyon NVARCHAR(30),
            @KulID BIGINT,
            @KulAd NVARCHAR(200),
            @LogMesaj NVARCHAR(300),
            @ViewID BIGINT,
            @ViewAd NVARCHAR(30),
            @MasterID BIGINT;

    DECLARE @ParcaState NVARCHAR(200),
            @ParcaPartID BIGINT,
            @ParcaPartMasterID BIGINT,
            @ParcaName NVARCHAR(150),
            @ParcaNumber NVARCHAR(MAX),
            @ParcaVersion NCHAR(30),
            @EntegrasyonDurum TINYINT;

    -- INSERTED tablosundan gerekli değerleri alıyoruz
    SELECT @ParcaPartID = idA2A2, 
           @ParcaState = statestate, 
           @ParcaVersion = versionIdA2versionInfo + '.' + iterationIdA2iterationInfo, 
           @KulID = idA3D2iterationInfo, 
           @ParcaPartMasterID = idA3masterReference, 
           @ViewID = idA3view 
    FROM INSERTED;

    -- Eğer önemli değerler NULL ise, INSERT veya UPDATE işlemi yapılmaz
    IF @ParcaPartID IS NULL OR @ParcaState IS NULL OR @ParcaPartMasterID IS NULL
    BEGIN

        RETURN;
    END

    -- Diğer değerlerin alınması
    SELECT @ParcaName = name, @ParcaNumber = WTPartNumber 
    FROM {scheman}.WTPartMaster 
    WHERE idA2A2 = @ParcaPartMasterID;

    SELECT @KulAd = name 
    FROM {scheman}.WTUser 
    WHERE idA2A2 = @KulID;

    SELECT @ViewAd = name 
    FROM {scheman}.WTView 
    WHERE idA2A2 = @ViewID;

    --wrk Control
    DECLARE @wtpResult INT = 0; 
    IF EXISTS (SELECT 1 FROM [{scheman}].WTPart WHERE idA3masterReference = @ParcaPartMasterID AND statecheckoutInfo = 'wrk')
    BEGIN
        SET @wtpResult = 1;
    END
    ELSE
    BEGIN
        SET @wtpResult = ISNULL(@wtpResult, 2); 
    END

    -- Eğer kontrol sonucunda wtpResult 0 ise devam ediyoruz
    IF @wtpResult = 0 
    BEGIN
        -- Eğer ParcaPartID varsa güncelleme işlemi yapılacak
        IF EXISTS (SELECT 1 FROM [{scheman}].[Des_WTPart_LogTable] WHERE ParcaPartID = @ParcaPartID)
        BEGIN
            UPDATE [{scheman}].[Des_WTPart_LogTable]
            SET ParcaState = @ParcaState, 
                ParcaPartMasterID = @ParcaPartMasterID, 
                ParcaName = @ParcaName, 
                ParcaNumber = @ParcaNumber, 
                ParcaVersion = @ParcaVersion, 
                KulAd = @KulAd, 
                EntegrasyonDurum = @EntegrasyonDurum, 
                LogMesaj = 
                    CASE 
                        WHEN @ParcaState = 'RELEASED' THEN 'Parça gönderildi'
                        WHEN @ParcaState = 'CANCELLED' THEN 'Parça iptal edildi'
                        WHEN @ParcaState = 'INWORK' THEN 'Parça devam ediyor'
                        ELSE LogMesaj
                    END
            WHERE ParcaPartID = @ParcaPartID;
        END
        ELSE
        BEGIN
            -- Eğer ParcaPartID yoksa yeni bir satır ekliyoruz
            INSERT INTO [{scheman}].[Des_WTPart_LogTable] 
                (ParcaState, ParcaPartID, ParcaPartMasterID, ParcaName, ParcaNumber, ParcaVersion, KulAd, EntegrasyonDurum, LogMesaj)
            VALUES 
                (@ParcaState, @ParcaPartID, @ParcaPartMasterID, @ParcaName, @ParcaNumber, @ParcaVersion, @KulAd, @EntegrasyonDurum, 
                    CASE 
                        WHEN @ParcaState = 'RELEASED' THEN 'Parça gönderildi'
                        WHEN @ParcaState = 'CANCELLED' THEN 'Parça iptal edildi'
                        WHEN @ParcaState = 'INWORK' THEN 'Parça devam ediyor'
                        ELSE NULL
                    END);

            -- İkinci tabloya da ekleme işlemi yapılıyor
            INSERT INTO [{scheman}].[Des_WTPart_LogTable_Takip] 
                (ParcaState, ParcaPartID, ParcaPartMasterID, ParcaName, ParcaNumber, ParcaVersion, KulAd, EntegrasyonDurum, LogMesaj)
            VALUES 
                (@ParcaState, @ParcaPartID, @ParcaPartMasterID, @ParcaName, @ParcaNumber, @ParcaVersion, @KulAd, @EntegrasyonDurum, 
                    CASE 
                        WHEN @ParcaState = 'RELEASED' THEN 'Parça gönderildi'
                        WHEN @ParcaState = 'CANCELLED' THEN 'Parça iptal edildi'
                        WHEN @ParcaState = 'INWORK' THEN 'Parça devam ediyor'
                        ELSE NULL
                    END);
        END
    END
END;
";


			string Part_ReviseAndSaveAsClean = $@"
CREATE TRIGGER [{scheman}].[Part_ReviseAndSaveAsClean]
ON [{scheman}].[WTPart]
AFTER INSERT
AS
BEGIN
    DECLARE @Part_ID BIGINT,
            @Part_ID2 NVARCHAR(MAX),
            @State NVARCHAR(200),
            @Number NVARCHAR(200),
            @ParcaAd NVARCHAR(200),
            @Versiyon NVARCHAR(30),
            @KulID BIGINT,
            @KulAd NVARCHAR(200),
            @ViewID BIGINT,
            @ViewAd NVARCHAR(30),
            @MasterID BIGINT,
            @StringDefinitionID BIGINT,
            @UpdateCount INT;

    -- Retrieve values from INSERTED table
    SELECT @Part_ID = idA2A2,
           @Part_ID2 = idA2A2,
           @State = statestate,
           @Versiyon = versionIdA2versionInfo + '.' + iterationIdA2iterationInfo,
           @KulID = idA3D2iterationInfo,
           @MasterID = idA3masterReference,
           @ViewID = idA3view
    FROM INSERTED;

    -- Retrieve additional values
    SELECT @ParcaAd = name, @Number = WTPartNumber
    FROM {scheman}.WTPartMaster
    WHERE idA2A2 = @MasterID;

    SELECT @KulAd = name
    FROM {scheman}.WTUser
    WHERE idA2A2 = @KulID;

    SELECT @ViewAd = name
    FROM {scheman}.WTView
    WHERE idA2A2 = @ViewID;

    -- Process only if the state is 'INWORK'
    IF @State = 'INWORK'
    BEGIN
        SELECT @StringDefinitionID = idA2A2
        FROM [{scheman}].[StringDefinition]
        WHERE [displayName] = 'Entegrasyon Durumu';

        INSERT INTO {scheman}.Des_LogDataReviseAndSaveAsProcess 
            (statestate, PartID, AnaNumber, AnaParcaAd, Number, ParcaAd, Version, KulAd, LogCode, LogMesaj)
        VALUES 
            (@State, @Part_ID, NULL, NULL, @Number, @ParcaAd, @Versiyon, @KulAd, 'DES-WTP-005', 'Parça oluşturuldu ve işlemde');

        IF EXISTS (SELECT 1 
                   FROM [{scheman}].[StringValue] 
                   WHERE idA3A4 = @Part_ID 
                     AND idA3A6 = @StringDefinitionID)
        BEGIN
            UPDATE [{scheman}].[StringValue]
            SET [value] = NULL,
                [value2] = NULL
            WHERE idA3A4 = @Part_ID 
              AND idA3A6 = @StringDefinitionID;

            SET @UpdateCount = @@ROWCOUNT;

            INSERT INTO {scheman}.DebugTable 
                (Part_ID, StringDefinitionID, UpdateCount, DebugMessage) 
            VALUES 
                (@Part_ID, @StringDefinitionID, @UpdateCount, 'Güncelleme başarılı');
        END
        ELSE
        BEGIN
            INSERT INTO {scheman}.DebugTable 
                (Part_ID, StringDefinitionID, UpdateCount, DebugMessage) 
            VALUES 
                (@Part_ID, @StringDefinitionID, 0, 'Kayıt bulunamadı');
        END
    END
END";

			string Part_EquivalenceLink_Control = $@"
CREATE TRIGGER [{scheman}].[Part_EquivalenceLink_Control]
ON [{scheman}].[WTPart]
AFTER UPDATE
AS
BEGIN
    DECLARE @Part_ID BIGINT,
            @State NVARCHAR(200),
            @Number NVARCHAR(200),
            @ParcaAd NVARCHAR(200),
            @Versiyon NVARCHAR(30),
            @KulID BIGINT,
            @KulAd NVARCHAR(200),
            @ViewID BIGINT,
            @ViewAd NVARCHAR(30),
            @MasterID BIGINT,
            @idA3A6 BIGINT,
            @idA3B6 BIGINT,
            @AnaParcaPartID BIGINT,
            @MuadilParcaPartID BIGINT;

    SELECT @Part_ID = idA2A2,
           @State = statestate,
           @Versiyon = versionIdA2versionInfo + '.' + iterationIdA2iterationInfo,
           @KulID = idA3D2iterationInfo,
           @MasterID = idA3masterReference,
           @ViewID = idA3view
    FROM INSERTED;

    SELECT @ParcaAd = name, 
           @Number = WTPartNumber 
    FROM {scheman}.WTPartMaster 
    WHERE idA2A2 = @MasterID;

    SELECT @KulAd = name 
    FROM {scheman}.WTUser 
    WHERE idA2A2 = @KulID;

    SELECT @ViewAd = name 
    FROM {scheman}.WTView 
    WHERE idA2A2 = @ViewID;

    -- Equivalence Control
    SELECT @idA3A6 = idA3A6, @idA3B6 = idA3B6 
    FROM {scheman}.Des_EquivalenceLink_LogTable 
    WHERE idA3A5 = @Part_ID OR idA3B5 = @Part_ID;

    -- Alternate Control
    SELECT @AnaParcaPartID = AnaParcaPartID, @MuadilParcaPartID = MuadilParcaPartID 
    FROM {scheman}.Des_AlternateLink_LogTable 
    WHERE AnaParcaPartID = @Part_ID OR MuadilParcaPartID = @Part_ID;

    -- Equivalence Updates
    IF @idA3B6 = @ViewID 
    BEGIN
        UPDATE {scheman}.Des_EquivalenceLink_LogTable 
        SET APartState = @State 
        WHERE idA3A5 = @Part_ID AND idA3B5 != @Part_ID;
    END
    ELSE IF @idA3A6 = @ViewID 
    BEGIN
        UPDATE {scheman}.Des_EquivalenceLink_LogTable 
        SET BPartState = @State 
        WHERE idA3B5 = @Part_ID AND idA3A5 != @Part_ID;
    END;

    -- Alternate Updates
    IF @AnaParcaPartID = @Part_ID 
    BEGIN
        UPDATE {scheman}.Des_AlternateLink_LogTable 
        SET AnaParcaState = @State 
        WHERE AnaParcaPartID = @Part_ID AND MuadilParcaPartID != @Part_ID;
    END 
    ELSE IF @MuadilParcaPartID = @Part_ID 
    BEGIN
        UPDATE {scheman}.Des_AlternateLink_LogTable 
        SET MuadilParcaState = @State 
        WHERE MuadilParcaPartID = @Part_ID AND AnaParcaPartID != @Part_ID;
    END;
END";


			#endregion

			#region WTPart AlternateLink
			string MuadilTakip = @$"
CREATE TRIGGER [{scheman}].[MuadilTakip] 
ON [{scheman}].[WTPartAlternateLink]
AFTER INSERT,DELETE
AS 
BEGIN
    DECLARE 
        @MuadilParcaMasterID INT,
        @AnaParcaPartMasterID INT,
        @AnaParcaState NVARCHAR(50), 
        @AnaParcaPartID INT,
        @AnaParcaName NVARCHAR(255), 
        @AnaParcaNumber NVARCHAR(50),
        @AnaParcaVersion NVARCHAR(50), 
        @MuadilParcaState NVARCHAR(50), 
        @MuadilParcaPartID INT,
        @MuadilParcaName NVARCHAR(255), 
        @MuadilParcaNumber NVARCHAR(50),
        @MuadilParcaVersion NVARCHAR(50),
        @iterationIdA2iterationInfoA NVARCHAR(150),
        @iterationIdA2iterationInfoB NVARCHAR(150),
        @versionIdA2versionInfoA NVARCHAR(150),
        @versionIdA2versionInfoB NVARCHAR(150),
        @KulID BIGINT,
        @KulAd NVARCHAR(200);

    -- Kullanıcı bilgilerini al


    SELECT @KulAd = name 
    FROM {scheman}.WTUser 
    WHERE idA2A2 = @KulID;

    -- Ekleme işlemi
IF EXISTS (SELECT * FROM INSERTED) AND NOT EXISTS(SELECT * FROM DELETED)
    BEGIN
        SELECT @MuadilParcaMasterID = idA3B5, @AnaParcaPartMasterID = idA3A5 
        FROM INSERTED;

        -- Ana ve muadil parça bilgilerini al
        SELECT 
            @versionIdA2versionInfoA = versionIdA2versionInfo,
            @iterationIdA2iterationInfoA = iterationIdA2iterationInfo,
            @AnaParcaState = statestate,
            @AnaParcaPartID = idA2A2
        FROM {scheman}.WTPart 
        WHERE idA3masterReference = @AnaParcaPartMasterID
        AND versionIdA2versionInfo = (
            SELECT MAX(versionIdA2versionInfo)
            FROM {scheman}.WTPart
            WHERE idA3masterReference = @AnaParcaPartMasterID
        )
        AND iterationIdA2iterationInfo = (
            SELECT MAX(iterationIdA2iterationInfo)
            FROM {scheman}.WTPart
            WHERE idA3masterReference = @AnaParcaPartMasterID
            AND versionIdA2versionInfo = (
                SELECT MAX(versionIdA2versionInfo)
                FROM {scheman}.WTPart
                WHERE idA3masterReference = @AnaParcaPartMasterID
            )
        );

        SELECT 
            @versionIdA2versionInfoB = versionIdA2versionInfo,
            @iterationIdA2iterationInfoB = iterationIdA2iterationInfo,
            @MuadilParcaState = statestate,
            @MuadilParcaPartID = idA2A2
        FROM {scheman}.WTPart 
        WHERE idA3masterReference = @MuadilParcaMasterID
        AND versionIdA2versionInfo = (
            SELECT MAX(versionIdA2versionInfo)
            FROM {scheman}.WTPart
            WHERE idA3masterReference = @MuadilParcaMasterID
        )
        AND iterationIdA2iterationInfo = (
            SELECT MAX(iterationIdA2iterationInfo)
            FROM {scheman}.WTPart
            WHERE idA3masterReference = @MuadilParcaMasterID
            AND versionIdA2versionInfo = (
                SELECT MAX(versionIdA2versionInfo)
                FROM {scheman}.WTPart
                WHERE idA3masterReference = @MuadilParcaMasterID
            )
        );

        SELECT @AnaParcaName = name, @AnaParcaNumber = WTPartNumber 
        FROM {scheman}.WTPartMaster 
        WHERE idA2A2 = @AnaParcaPartMasterID;

        SELECT @MuadilParcaName = name, @MuadilParcaNumber = WTPartNumber 
        FROM {scheman}.WTPartMaster 
        WHERE idA2A2 = @MuadilParcaMasterID;

        SET @AnaParcaVersion = @versionIdA2versionInfoA + '.' + @iterationIdA2iterationInfoA;
        SET @MuadilParcaVersion = @versionIdA2versionInfoB + '.' + @iterationIdA2iterationInfoB;


	---Nul Control
	IF @AnaParcaPartID IS NULL OR @AnaParcaState IS NULL OR @AnaParcaNumber IS NULL OR @MuadilParcaState IS NULL OR @MuadilParcaPartID IS NULL OR @MuadilParcaNumber IS NULL
    BEGIN
        RETURN;
    END
	---Nul Control

        -- Log tablolarında güncelleme veya ekleme işlemi
        IF EXISTS (SELECT 1 FROM {scheman}.Des_AlternateLink_LogTable 
                   WHERE AnaParcaPartMasterID = @AnaParcaPartMasterID 
                   AND MuadilParcaMasterID = @MuadilParcaMasterID)
        BEGIN
            -- Kayıt varsa güncelle
            UPDATE {scheman}.Des_AlternateLink_LogTable
            SET AnaParcaState = @AnaParcaState,
                AnaParcaPartID = @AnaParcaPartID,
                AnaParcaName = @AnaParcaName,
                AnaParcaNumber = @AnaParcaNumber,
                AnaParcaVersion = @AnaParcaVersion,
                MuadilParcaState = @MuadilParcaState,
                MuadilParcaPartID = @MuadilParcaPartID,
                MuadilParcaName = @MuadilParcaName,
                MuadilParcaNumber = @MuadilParcaNumber,
                MuadilParcaVersion = @MuadilParcaVersion,
                KulAd = @KulAd,
                LogMesaj = 'Ana Parça: ' + @AnaParcaName + ' Muadil Parça: ' + @MuadilParcaName + ' ile ilişkisi güncellendi'
            WHERE AnaParcaPartMasterID = @AnaParcaPartMasterID
            AND MuadilParcaMasterID = @MuadilParcaMasterID;
        END
        ELSE
        BEGIN
            -- Kayıt yoksa ekle
            INSERT INTO {scheman}.Des_AlternateLink_LogTable (
                AnaParcaState, AnaParcaPartID, AnaParcaPartMasterID, 
                AnaParcaName, AnaParcaNumber, AnaParcaVersion, 
                MuadilParcaState, MuadilParcaPartID, MuadilParcaMasterID, 
                MuadilParcaName, MuadilParcaNumber, MuadilParcaVersion, 
                KulAd, LogMesaj
            ) 
            VALUES (
                @AnaParcaState, @AnaParcaPartID, @AnaParcaPartMasterID, 
                @AnaParcaName, @AnaParcaNumber, @AnaParcaVersion, 
                @MuadilParcaState, @MuadilParcaPartID, @MuadilParcaMasterID, 
                @MuadilParcaName, @MuadilParcaNumber, @MuadilParcaVersion, 
                @KulAd, 'Ana Parça: ' + @AnaParcaName + ' Muadil Parça: ' + @MuadilParcaName + ' ile ilişkisi eklendi'
            );
        END
    END

    -- Silme işlemi 

   IF NOT EXISTS(SELECT * FROM INSERTED) AND EXISTS (SELECT * FROM DELETED)
BEGIN
    SELECT @MuadilParcaMasterID = idA3B5, @AnaParcaPartMasterID = idA3A5 
    FROM DELETED;

    -- Ana ve muadil parça bilgilerini al
    SELECT 
        @versionIdA2versionInfoA = versionIdA2versionInfo,
        @iterationIdA2iterationInfoA = iterationIdA2iterationInfo,
        @AnaParcaState = statestate,
        @AnaParcaPartID = idA2A2
    FROM {scheman}.WTPart 
    WHERE idA3masterReference = @AnaParcaPartMasterID
    AND versionIdA2versionInfo = (
        SELECT MAX(versionIdA2versionInfo)
        FROM {scheman}.WTPart
        WHERE idA3masterReference = @AnaParcaPartMasterID
    )
    AND iterationIdA2iterationInfo = (
        SELECT MAX(iterationIdA2iterationInfo)
        FROM {scheman}.WTPart
        WHERE idA3masterReference = @AnaParcaPartMasterID
        AND versionIdA2versionInfo = (
            SELECT MAX(versionIdA2versionInfo)
            FROM {scheman}.WTPart
            WHERE idA3masterReference = @AnaParcaPartMasterID
        )
    );

    SELECT 
        @versionIdA2versionInfoB = versionIdA2versionInfo,
        @iterationIdA2iterationInfoB = iterationIdA2iterationInfo,
        @MuadilParcaState = statestate,
        @MuadilParcaPartID = idA2A2
    FROM {scheman}.WTPart 
    WHERE idA3masterReference = @MuadilParcaMasterID
    AND versionIdA2versionInfo = (
        SELECT MAX(versionIdA2versionInfo)
        FROM {scheman}.WTPart
        WHERE idA3masterReference = @MuadilParcaMasterID
    )
    AND iterationIdA2iterationInfo = (
        SELECT MAX(iterationIdA2iterationInfo)
        FROM {scheman}.WTPart
        WHERE idA3masterReference = @MuadilParcaMasterID
        AND versionIdA2versionInfo = (
            SELECT MAX(versionIdA2versionInfo)
            FROM {scheman}.WTPart
            WHERE idA3masterReference = @MuadilParcaMasterID
        )
    );

    SELECT @AnaParcaName = name, @AnaParcaNumber = WTPartNumber 
    FROM {scheman}.WTPartMaster 
    WHERE idA2A2 = @AnaParcaPartMasterID;

    SELECT @MuadilParcaName = name, @MuadilParcaNumber = WTPartNumber 
    FROM {scheman}.WTPartMaster 
    WHERE idA2A2 = @MuadilParcaMasterID;

    SET @AnaParcaVersion = @versionIdA2versionInfoA + '.' + @iterationIdA2iterationInfoA;
    SET @MuadilParcaVersion = @versionIdA2versionInfoB + '.' + @iterationIdA2iterationInfoB;


	---Nul Control
	IF @AnaParcaPartID IS NULL OR @AnaParcaState IS NULL OR @AnaParcaNumber IS NULL OR @MuadilParcaState IS NULL OR @MuadilParcaPartID IS NULL OR @MuadilParcaNumber IS NULL
    BEGIN
        RETURN;
    END
	---Nul Control


    -- Log tablosuna güncelleme işlemi
    IF EXISTS (SELECT 1 FROM {scheman}.Des_AlternateLinkRemoved_LogTable 
               WHERE AnaParcaPartMasterID = @AnaParcaPartMasterID 
               AND MuadilParcaMasterID = @MuadilParcaMasterID)
    BEGIN
        -- Kayıt varsa güncelle
        UPDATE {scheman}.Des_AlternateLinkRemoved_LogTable
        SET AnaParcaState = @AnaParcaState,
            AnaParcaPartID = @AnaParcaPartID,
            AnaParcaName = @AnaParcaName,
            AnaParcaNumber = @AnaParcaNumber,
            AnaParcaVersion = @AnaParcaVersion,
            MuadilParcaState = @MuadilParcaState,
            MuadilParcaPartID = @MuadilParcaPartID,
            MuadilParcaName = @MuadilParcaName,
            MuadilParcaNumber = @MuadilParcaNumber,
            MuadilParcaVersion = @MuadilParcaVersion,
            KulAd = @KulAd,
            LogMesaj = 'Ana Parça: ' + @AnaParcaName + ' Muadil Parça: ' + @MuadilParcaName + ' ile ilişkisi silindi'
        WHERE AnaParcaPartMasterID = @AnaParcaPartMasterID
        AND MuadilParcaMasterID = @MuadilParcaMasterID;
    END
    ELSE
    BEGIN
        -- Eğer kayıt yoksa ekleme yap (silinen ilişkileri kaydetmek için)
        INSERT INTO {scheman}.Des_AlternateLinkRemoved_LogTable (
            AnaParcaState, AnaParcaPartID, AnaParcaPartMasterID, 
            AnaParcaName, AnaParcaNumber, AnaParcaVersion, 
            MuadilParcaState, MuadilParcaPartID, MuadilParcaMasterID, 
            MuadilParcaName, MuadilParcaNumber, MuadilParcaVersion, 
            KulAd, LogMesaj
        ) 
        VALUES (
            @AnaParcaState, @AnaParcaPartID, @AnaParcaPartMasterID, 
            @AnaParcaName, @AnaParcaNumber, @AnaParcaVersion, 
            @MuadilParcaState, @MuadilParcaPartID, @MuadilParcaMasterID, 
            @MuadilParcaName, @MuadilParcaNumber, @MuadilParcaVersion, 
            @KulAd, 'Ana Parça: ' + @AnaParcaName + ' Muadil Parça: ' + @MuadilParcaName + ' ile ilişkisi silindi'
        );
    END
END

END;";

			#endregion


			#region WTPart ReferenceLink
			string Part_Document = $@"
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

    SELECT @DocType_ID = idA2typeDefinitionReference, @DocID = idA2A2 
    FROM [{scheman}].WTDocument 
    WHERE idA3masterReference = @DocMaster_IDGecici;

    SELECT @Eklenti = WTDocumentTypeName, @EklemeDurumu = EklemeDurumu 
    FROM [{scheman}].Des_PartDocumentBagla 
    WHERE WTDocumentTypeID = @DocType_ID;

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

            INSERT INTO [{scheman}].[Des_PartDokumanBaglaLog] (Part_ID, Part_Number, PartMasterID, DocMaster_ID, Eklenti)
            VALUES (@PartID, @Part_Number, @PartMasterID, @idA3B5, @Eklenti);

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
                SET WTDocumentNumber = @Eklenti + '_' + @Part_Number + '_' + CONVERT(nvarchar, @EklentiSayisi)
                WHERE idA2A2 = @idA3B5;
            END
        END
    END
END;";

			#endregion
			#endregion


			#region WTPart Alternate Bölümü
			#region WTPart Alternate Tables
			string Des_AlternateLink_LogTable = $@"
CREATE TABLE [{scheman}].[Des_AlternateLink_LogTable] (
    [LogID] INT IDENTITY(1, 1) NOT NULL,
    [AnaParcaState] NVARCHAR(200) NULL,
    [AnaParcaPartID] BIGINT NULL,
    [AnaParcaPartMasterID] BIGINT NULL,
    [AnaParcaName] NVARCHAR(150) NULL,
    [AnaParcaNumber] NVARCHAR(MAX) NULL,
    [AnaParcaVersion] NCHAR(30) NULL,
    [MuadilParcaState] NVARCHAR(200) NULL,
    [MuadilParcaPartID] BIGINT NULL,
    [MuadilParcaMasterID] BIGINT NULL,
    [MuadilParcaName] NVARCHAR(150) NULL,
    [MuadilParcaNumber] NVARCHAR(MAX) NULL,
    [MuadilParcaVersion] NCHAR(30) NULL,
    [KulAd] NVARCHAR(50) NULL,
    [LogDate] DATETIME NULL,
    [EntegrasyonDurum] TINYINT NULL,
    [LogMesaj] NVARCHAR(300) NULL,
    CONSTRAINT [PK_Des_AlternateLink_LogTable] PRIMARY KEY CLUSTERED ([LogID] ASC)
) ON [PRIMARY];

ALTER TABLE [{scheman}].[Des_AlternateLink_LogTable] ADD CONSTRAINT [DF_Des_AlternateLink_LogTable_LogDate] DEFAULT (getdate()) FOR [LogDate];
";

			string Des_AlternateLink_LogTable_Error = $@"
CREATE TABLE [{scheman}].[Des_AlternateLink_LogTable_Error] (
    [LogID] INT NOT NULL,
    [AnaParcaState] NVARCHAR(200) NULL,
    [AnaParcaPartID] BIGINT NULL,
    [AnaParcaPartMasterID] BIGINT NULL,
    [AnaParcaName] NVARCHAR(150) NULL,
    [AnaParcaNumber] NVARCHAR(MAX) NULL,
    [AnaParcaVersion] NCHAR(30) NULL,
    [MuadilParcaState] NVARCHAR(200) NULL,
    [MuadilParcaPartID] BIGINT NULL,
    [MuadilParcaMasterID] BIGINT NULL,
    [MuadilParcaName] NVARCHAR(150) NULL,
    [MuadilParcaNumber] NVARCHAR(MAX) NULL,
    [MuadilParcaVersion] NCHAR(30) NULL,
    [KulAd] NVARCHAR(50) NULL,
    [LogDate] DATETIME NULL,
    [EntegrasyonDurum] TINYINT NULL,
    [LogMesaj] NVARCHAR(300) NULL
) ON [PRIMARY];

ALTER TABLE [{scheman}].[Des_AlternateLink_LogTable_Error] ADD CONSTRAINT [DF_Des_AlternateLink_LogTable_Error_LogDate] DEFAULT (getdate()) FOR [LogDate];
";

			string Des_AlternateLink_LogTable_Takip = $@"
CREATE TABLE [{scheman}].[Des_AlternateLink_LogTable_Takip] (
    [LogID] INT IDENTITY(1, 1) NOT NULL,
    [AnaParcaState] NVARCHAR(200) NULL,
    [AnaParcaPartID] BIGINT NULL,
    [AnaParcaPartMasterID] BIGINT NULL,
    [AnaParcaName] NVARCHAR(150) NULL,
    [AnaParcaNumber] NVARCHAR(MAX) NULL,
    [AnaParcaVersion] NCHAR(30) NULL,
    [MuadilParcaState] NVARCHAR(200) NULL,
    [MuadilParcaPartID] BIGINT NULL,
    [MuadilParcaMasterID] BIGINT NULL,
    [MuadilParcaName] NVARCHAR(150) NULL,
    [MuadilParcaNumber] NVARCHAR(MAX) NULL,
    [MuadilParcaVersion] NCHAR(30) NULL,
    [KulAd] NVARCHAR(50) NULL,
    [LogDate] DATETIME NULL,
    [EntegrasyonDurum] TINYINT NULL,
    [LogMesaj] NVARCHAR(300) NULL,
    CONSTRAINT [PK_Des_AlternateLink_LogTable_Takip] PRIMARY KEY CLUSTERED ([LogID] ASC)
) ON [PRIMARY];

ALTER TABLE [{scheman}].[Des_AlternateLink_LogTable_Takip] ADD CONSTRAINT [DF_Des_AlternateLink_LogTable_Takip_LogDate] DEFAULT (getdate()) FOR [LogDate];
";

			string Des_AlternateLinkRemoved_LogTable = $@"
CREATE TABLE [{scheman}].[Des_AlternateLinkRemoved_LogTable] (
    [LogID] INT IDENTITY(1, 1) NOT NULL,
    [AnaParcaState] NVARCHAR(200) NULL,
    [AnaParcaPartID] BIGINT NULL,
    [AnaParcaPartMasterID] BIGINT NULL,
    [AnaParcaName] NVARCHAR(150) NULL,
    [AnaParcaNumber] NVARCHAR(MAX) NULL,
    [AnaParcaVersion] NCHAR(30) NULL,
    [MuadilParcaState] NVARCHAR(200) NULL,
    [MuadilParcaPartID] BIGINT NULL,
    [MuadilParcaMasterID] BIGINT NULL,
    [MuadilParcaName] NVARCHAR(150) NULL,
    [MuadilParcaNumber] NVARCHAR(MAX) NULL,
    [MuadilParcaVersion] NCHAR(30) NULL,
    [KulAd] NVARCHAR(50) NULL,
    [LogDate] DATETIME NULL,
    [EntegrasyonDurum] TINYINT NULL,
    [LogMesaj] NVARCHAR(300) NULL,
    CONSTRAINT [PK_Des_AlternateLinkRemoved_LogTable] PRIMARY KEY CLUSTERED ([LogID] ASC)
) ON [PRIMARY];

ALTER TABLE [{scheman}].[Des_AlternateLinkRemoved_LogTable] ADD CONSTRAINT [DF_Des_AlternateLinkRemoved_LogTable_LogDate] DEFAULT (getdate()) FOR [LogDate];
";

			string Des_AlternateLinkRemoved_LogTable_Error = $@"
CREATE TABLE [{scheman}].[Des_AlternateLinkRemoved_LogTable_Error] (
    [LogID] INT NOT NULL,
    [AnaParcaState] NVARCHAR(200) NULL,
    [AnaParcaPartID] BIGINT NULL,
    [AnaParcaPartMasterID] BIGINT NULL,
    [AnaParcaName] NVARCHAR(150) NULL,
    [AnaParcaNumber] NVARCHAR(MAX) NULL,
    [AnaParcaVersion] NCHAR(30) NULL,
    [MuadilParcaState] NVARCHAR(200) NULL,
    [MuadilParcaPartID] BIGINT NULL,
    [MuadilParcaMasterID] BIGINT NULL,
    [MuadilParcaName] NVARCHAR(150) NULL,
    [MuadilParcaNumber] NVARCHAR(MAX) NULL,
    [MuadilParcaVersion] NCHAR(30) NULL,
    [KulAd] NVARCHAR(50) NULL,
    [LogDate] DATETIME NULL,
    [EntegrasyonDurum] TINYINT NULL,
    [LogMesaj] NVARCHAR(300) NULL
) ON [PRIMARY];

ALTER TABLE [{scheman}].[Des_AlternateLinkRemoved_LogTable_Error] ADD CONSTRAINT [DF_Des_AlternateLinkRemoved_LogTable_LogDate_Error] DEFAULT (getdate()) FOR [LogDate];
";
			#endregion

			#region WTPart Alternate Trigger
			string Des_AlternateLink_EntegrasyonControl = $@"
CREATE TRIGGER [{scheman}].[Des_AlternateLink_EntegrasyonControl]
ON [{scheman}].[Des_AlternateLink_LogTable]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @APartState VARCHAR(50),
            @BPartState VARCHAR(50),
            @APartMasterID BIGINT,
            @BPartMasterID BIGINT,
            @AnaParcaVersion VARCHAR(50),
            @MuadilParcaVersion VARCHAR(50),
            @AnaParcaPartID BIGINT,
            @MuadilParcaPartID BIGINT;

    DECLARE cur CURSOR FOR
    SELECT AnaParcaState, MuadilParcaState, AnaParcaPartMasterID, MuadilParcaMasterID,
           AnaParcaVersion, MuadilParcaVersion, AnaParcaPartID, MuadilParcaPartID
    FROM inserted;

    OPEN cur;

    FETCH NEXT FROM cur INTO @APartState, @BPartState, @APartMasterID, @BPartMasterID,
                             @AnaParcaVersion, @MuadilParcaVersion,
                             @AnaParcaPartID, @MuadilParcaPartID;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Ana parça ve muadil parça RELEASED durumundaysa
        IF @APartState = 'RELEASED' AND @BPartState = 'RELEASED'
        BEGIN
            UPDATE [{scheman}].[Des_AlternateLink_LogTable]
            SET EntegrasyonDurum = 1 -- True
            WHERE AnaParcaPartID = @AnaParcaPartID
              AND MuadilParcaPartID = @MuadilParcaPartID
              AND AnaParcaPartMasterID = @APartMasterID; -- Eklenen Koşul
        END
        -- Ana parça veya muadil parça CANCELLED durumundaysa
        ELSE IF @APartState = 'CANCELLED' OR @BPartState = 'CANCELLED'
        BEGIN
            UPDATE [{scheman}].[Des_AlternateLink_LogTable]
            SET EntegrasyonDurum = 0 -- False
            WHERE AnaParcaPartID = @AnaParcaPartID
              AND MuadilParcaPartID = @MuadilParcaPartID
              AND AnaParcaPartMasterID = @APartMasterID; -- Eklenen Koşul
        END
        -- Ana parça veya muadil parça INWORK durumundaysa
        ELSE IF @APartState = 'INWORK' OR @BPartState = 'INWORK'
        BEGIN
            -- Durumları kontrol et
            IF (@APartState = 'INWORK' AND @BPartState = 'RELEASED')
               OR (@APartState = 'RELEASED' AND @BPartState = 'INWORK')
               OR (@APartState = 'INWORK' AND @BPartState = 'INWORK')
            BEGIN
                -- Herhangi bir parça için RELEASED durumu mevcut mu kontrol et
                IF (@APartState = 'INWORK' AND EXISTS (
                        SELECT 1
                        FROM [{scheman}].[WTPart] AS wt
                        WHERE wt.idA3masterReference = @APartMasterID
                          AND wt.statestate = 'RELEASED'
                    )) AND (@BPartState = 'INWORK' AND EXISTS (
                        SELECT 1
                        FROM [{scheman}].[WTPart] AS wt
                        WHERE wt.idA3masterReference = @BPartMasterID
                          AND wt.statestate = 'RELEASED'
                    ))
                BEGIN
                    UPDATE [{scheman}].[Des_AlternateLink_LogTable]
                    SET EntegrasyonDurum = 1 -- True
                    WHERE AnaParcaPartID = @AnaParcaPartID
                      AND MuadilParcaPartID = @MuadilParcaPartID
                      AND AnaParcaPartMasterID = @APartMasterID; -- Eklenen Koşul
                END
                ELSE
                BEGIN
                    UPDATE [{scheman}].[Des_AlternateLink_LogTable]
                    SET EntegrasyonDurum = 0 -- False
                    WHERE AnaParcaPartID = @AnaParcaPartID
                      AND MuadilParcaPartID = @MuadilParcaPartID
                      AND AnaParcaPartMasterID = @APartMasterID; -- Eklenen Koşul
                END
            END
            -- Diğer durumlar için EntegrasyonDurum'u False yap
            ELSE
            BEGIN
                UPDATE [{scheman}].[Des_AlternateLink_LogTable]
                SET EntegrasyonDurum = 0 -- False
                WHERE AnaParcaPartID = @AnaParcaPartID
                  AND MuadilParcaPartID = @MuadilParcaPartID
                  AND AnaParcaPartMasterID = @APartMasterID; -- Eklenen Koşul
            END
        END

        FETCH NEXT FROM cur INTO @APartState, @BPartState, @APartMasterID, @BPartMasterID,
                                 @AnaParcaVersion, @MuadilParcaVersion,
                                 @AnaParcaPartID, @MuadilParcaPartID;
    END;

    CLOSE cur;
    DEALLOCATE cur;
END;";

			string Des_AlternateLink_LogTable_Error_Control = $@"
CREATE TRIGGER [{scheman}].[Des_AlternateLink_LogTable_Error_Control]
ON [{scheman}].[Des_AlternateLink_LogTable]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [{scheman}].[Des_AlternateLink_LogTable_Error]
    WHERE
        [AnaParcaPartID] IN (SELECT [AnaParcaPartID] FROM inserted)
        AND [MuadilParcaPartID] IN (SELECT [MuadilParcaPartID] FROM inserted);
END;";

			string Des_AlternateLinkRemoved_LogTable_Error_Control = $@"
CREATE TRIGGER [{scheman}].[Des_AlternateLinkRemoved_LogTable_Error_Control]
ON [{scheman}].[Des_AlternateLinkRemoved_LogTable]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [{scheman}].[Des_AlternateLinkRemoved_LogTable_Error]
    WHERE
        [AnaParcaPartID] IN (SELECT [AnaParcaPartID] FROM inserted)
        AND [MuadilParcaPartID] IN (SELECT [MuadilParcaPartID] FROM inserted);
END;";

			#endregion
			#endregion


			#region WTPart EquivalenceLink Bölümü

			#region WTPart EquivalenceLink Tables
			string Des_EquivalenceLink_LogTable = $@"
CREATE TABLE [{scheman}].[Des_EquivalenceLink_LogTable] (
    [ID] BIGINT IDENTITY(1, 1) NOT NULL,
    [classnamekeyroleAObjectRef] NVARCHAR(50) NULL,
    [idA3A5] BIGINT NULL,
    [idA3A6] BIGINT NULL,
    [iterationIdA2iterationInfo] BIGINT NULL,
    [versionIdA2versionInfo] NVARCHAR(MAX) NULL,
    [APartState] NVARCHAR(MAX) NULL,
    [APartMasterID] BIGINT NULL,
    [APartName] NVARCHAR(MAX) NULL,
    [ApartWtNumber] NVARCHAR(MAX) NULL,
    [classnamekeyroleBObjectRef] NVARCHAR(50) NULL,
    [idA3B5] BIGINT NULL,
    [idA3B6] BIGINT NULL,
    [iterationIdA2iterationInfoB] BIGINT NULL,
    [versionIdA2versionInfoB] NVARCHAR(MAX) NULL,
    [BPartState] NVARCHAR(MAX) NULL,
    [BPartMasterID] BIGINT NULL,
    [BPartName] NVARCHAR(MAX) NULL,
    [BpartWtNumber] NVARCHAR(MAX) NULL,
    [idA2A2] BIGINT NULL,
    [EntegrasyonDurum] TINYINT NULL,
    CONSTRAINT [PK_Des_EquivalenceLink_LogTable] PRIMARY KEY CLUSTERED ([ID] ASC)
) ON [PRIMARY];
";

			string Des_EquivalenceLink_LogTable_Error = $@"
CREATE TABLE [{scheman}].[Des_EquivalenceLink_LogTable_Error] (
    [ID] BIGINT NOT NULL,
    [classnamekeyroleAObjectRef] NVARCHAR(50) NULL,
    [idA3A5] BIGINT NULL,
    [idA3A6] BIGINT NULL,
    [iterationIdA2iterationInfo] BIGINT NULL,
    [versionIdA2versionInfo] NVARCHAR(MAX) NULL,
    [APartState] NVARCHAR(MAX) NULL,
    [APartMasterID] BIGINT NULL,
    [APartName] NVARCHAR(MAX) NULL,
    [ApartWtNumber] NVARCHAR(MAX) NULL,
    [classnamekeyroleBObjectRef] NVARCHAR(50) NULL,
    [idA3B5] BIGINT NULL,
    [idA3B6] BIGINT NULL,
    [iterationIdA2iterationInfoB] BIGINT NULL,
    [versionIdA2versionInfoB] NVARCHAR(MAX) NULL,
    [BPartState] NVARCHAR(MAX) NULL,
    [BPartMasterID] BIGINT NULL,
    [BPartName] NVARCHAR(MAX) NULL,
    [BpartWtNumber] NVARCHAR(MAX) NULL,
    [idA2A2] BIGINT NULL,
    [EntegrasyonDurum] TINYINT NULL
) ON [PRIMARY];
";

			string Des_EquivalenceLink_LogTable_Takip = $@"
CREATE TABLE [{scheman}].[Des_EquivalenceLink_LogTable_Takip] (
    [ID] BIGINT IDENTITY(1, 1) NOT NULL,
    [classnamekeyroleAObjectRef] NVARCHAR(50) NULL,
    [idA3A5] BIGINT NULL,
    [idA3A6] BIGINT NULL,
    [iterationIdA2iterationInfo] BIGINT NULL,
    [versionIdA2versionInfo] NVARCHAR(MAX) NULL,
    [APartState] NVARCHAR(MAX) NULL,
    [APartMasterID] BIGINT NULL,
    [APartName] NVARCHAR(MAX) NULL,
    [ApartWtNumber] NVARCHAR(MAX) NULL,
    [classnamekeyroleBObjectRef] NVARCHAR(50) NULL,
    [idA3B5] BIGINT NULL,
    [idA3B6] BIGINT NULL,
    [iterationIdA2iterationInfoB] BIGINT NULL,
    [versionIdA2versionInfoB] NVARCHAR(MAX) NULL,
    [BPartState] NVARCHAR(MAX) NULL,
    [BPartMasterID] BIGINT NULL,
    [BPartName] NVARCHAR(MAX) NULL,
    [BpartWtNumber] NVARCHAR(MAX) NULL,
    [idA2A2] BIGINT NULL,
    [EntegrasyonDurum] TINYINT NULL,
    CONSTRAINT [PK_Des_EquivalenceLink_LogTable_Takip] PRIMARY KEY CLUSTERED ([ID] ASC)
) ON [PRIMARY];
";
			#endregion

			#region WTPart EquivalenceLink Trigger
			string Des_EquivalenceLink_EntegrarsyonControl = $@"
CREATE TRIGGER [{scheman}].[Des_EquivalenceLink_EntegrarsyonControl]
ON [{scheman}].[Des_EquivalenceLink_LogTable]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @APartState VARCHAR(50),
            @BPartState VARCHAR(50),
            @APartMasterID BIGINT,
            @idA3A6 BIGINT,
            @BPartMasterID BIGINT,
            @idA3B6 BIGINT,
            @versionIdA2versionInfo VARCHAR(50),
            @versionIdA2versionInfoB VARCHAR(50),
            @iterationIdA2iterationInfo INT,
            @idA3A5 BIGINT,
            @idA3B5 BIGINT,
            @iterationIdA2iterationInfoB INT;

    DECLARE cur CURSOR FOR
    SELECT APartState, BPartState, APartMasterID, BPartMasterID,
           idA3A6, idA3B6, idA3A5, idA3B5,
           versionIdA2versionInfo, versionIdA2versionInfoB,
           iterationIdA2iterationInfo, iterationIdA2iterationInfoB
    FROM inserted;

    OPEN cur;

    FETCH NEXT FROM cur INTO @APartState, @BPartState, @APartMasterID, @BPartMasterID,
                             @idA3A6, @idA3B6, @idA3A5, @idA3B5,
                             @versionIdA2versionInfo, @versionIdA2versionInfoB,
                             @iterationIdA2iterationInfo, @iterationIdA2iterationInfoB;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        -- Ana parça ve muadil parça RELEASED durumundaysa
        IF @APartState = 'RELEASED' AND @BPartState = 'RELEASED'
        BEGIN
            UPDATE [{scheman}].[Des_EquivalenceLink_LogTable]
            SET EntegrasyonDurum = 1 -- True
            WHERE idA3A5 = @idA3A5
              AND idA3B5 = @idA3B5
              AND APartMasterID = @APartMasterID; -- Eklenen Koşul
        END
        -- Ana parça veya muadil parça CANCELLED durumundaysa
        ELSE IF @APartState = 'CANCELLED' OR @BPartState = 'CANCELLED'
        BEGIN
            UPDATE [{scheman}].[Des_EquivalenceLink_LogTable]
            SET EntegrasyonDurum = 0 -- False
            WHERE idA3A5 = @idA3A5
              AND idA3B5 = @idA3B5
              AND APartMasterID = @APartMasterID; -- Eklenen Koşul
        END
        -- Ana parça veya muadil parça INWORK durumundaysa
        ELSE IF @APartState = 'INWORK' OR @BPartState = 'INWORK'
        BEGIN
            -- Durumları kontrol et
            IF (@APartState = 'INWORK' AND @BPartState = 'RELEASED')
               OR (@APartState = 'RELEASED' AND @BPartState = 'INWORK')
               OR (@APartState = 'INWORK' AND @BPartState = 'INWORK')
            BEGIN
                -- Herhangi bir parça için RELEASED durumu mevcut mu kontrol et
                IF (@APartState = 'INWORK' AND EXISTS (
                        SELECT 1
                        FROM [{scheman}].[WTPart] AS wt
                        WHERE wt.idA3masterReference = @APartMasterID
                          AND wt.statestate = 'RELEASED'
                    )) AND (@BPartState = 'INWORK' AND EXISTS (
                        SELECT 1
                        FROM [{scheman}].[WTPart] AS wt
                        WHERE wt.idA3masterReference = @BPartMasterID
                          AND wt.statestate = 'RELEASED'
                    ))
                BEGIN
                    UPDATE [{scheman}].[Des_EquivalenceLink_LogTable]
                    SET EntegrasyonDurum = 1 -- True
                    WHERE idA3A5 = @idA3A5
                      AND idA3B5 = @idA3B5
                      AND APartMasterID = @APartMasterID; -- Eklenen Koşul
                END
                ELSE
                BEGIN
                    UPDATE [{scheman}].[Des_EquivalenceLink_LogTable]
                    SET EntegrasyonDurum = 0 -- False
                    WHERE idA3A5 = @idA3A5
                      AND idA3B5 = @idA3B5
                      AND APartMasterID = @APartMasterID; -- Eklenen Koşul
                END
            END
            -- Diğer durumlar için EntegrasyonDurum'u False yap
            ELSE
            BEGIN
                UPDATE [{scheman}].[Des_EquivalenceLink_LogTable]
                SET EntegrasyonDurum = 0 -- False
                WHERE idA3A5 = @idA3A5
                  AND idA3B5 = @idA3B5
                  AND APartMasterID = @APartMasterID; -- Eklenen Koşul
            END
        END

        FETCH NEXT FROM cur INTO @APartState, @BPartState, @APartMasterID, @BPartMasterID,
                                 @idA3A6, @idA3B6, @idA3A5, @idA3B5,
                                 @versionIdA2versionInfo, @versionIdA2versionInfoB,
                                 @iterationIdA2iterationInfo, @iterationIdA2iterationInfoB;
    END;

    CLOSE cur;
    DEALLOCATE cur;
END;";

			string Des_EquivalenceLink_LogTable_Error_Control = $@"
CREATE TRIGGER [{scheman}].[Des_EquivalenceLink_LogTable_Error_Control]
ON [{scheman}].[Des_EquivalenceLink_LogTable]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [{scheman}].[Des_EquivalenceLink_LogTable_Error]
    WHERE
        [idA3A5] IN (SELECT [idA3A5] FROM inserted)
        AND [idA3B5] IN (SELECT [idA3B5] FROM inserted);
END;";

			string Des_EquivalenceLink =  $@"
CREATE TRIGGER [{scheman}].[Des_EquivalenceLink]
ON [{scheman}].[EquivalenceLink]
AFTER INSERT,UPDATE
AS
BEGIN
    DECLARE @idA3A5 BIGINT, @idA3B5 BIGINT;
    DECLARE @stateStateA5 NVARCHAR(MAX), @idA3masterReferenceA5 BIGINT;
    DECLARE @iterationIdA2iterationInfoA5 BIGINT, @versionIdA2versionInfoA5 NVARCHAR(MAX);
    DECLARE @stateStateB5 NVARCHAR(MAX), @idA3masterReferenceB5 BIGINT;
    DECLARE @iterationIdA2iterationInfoB5 BIGINT, @versionIdA2versionInfoB5 NVARCHAR(MAX);
    DECLARE @nameA5 NVARCHAR(MAX), @wtpartNumberA5 NVARCHAR(MAX);
    DECLARE @nameB5 NVARCHAR(MAX), @wtpartNumberB5 NVARCHAR(MAX);
	 DECLARE @idA3A6 BIGINT, @idA3B6 BIGINT;

    SELECT @idA3A5 = i.idA3A5, @idA3B5 = i.idA3B5,@idA3A6 = i.idA3A6, @idA3B6 = i.idA3B6
    FROM inserted i;

    SELECT 
        @stateStateA5 = p.statestate,
        @idA3masterReferenceA5 = p.idA3masterReference,
        @iterationIdA2iterationInfoA5 = p.iterationIdA2iterationInfo,
        @versionIdA2versionInfoA5 = p.versionIdA2versionInfo
    FROM [{scheman}].[WTPart] p
    WHERE p.idA2A2 = @idA3A5;

    SELECT 
        @stateStateB5 = p.statestate,
        @idA3masterReferenceB5 = p.idA3masterReference,
        @iterationIdA2iterationInfoB5 = p.iterationIdA2iterationInfo,
        @versionIdA2versionInfoB5 = p.versionIdA2versionInfo
    FROM [{scheman}].[WTPart] p
    WHERE p.idA2A2 = @idA3B5;

    SELECT 
        @nameA5 = m.name,
        @wtpartNumberA5 = m.WTPartNumber
    FROM [{scheman}].[WTPartMaster] m
    WHERE m.idA2A2 = @idA3masterReferenceA5;

    SELECT 
        @nameB5 = m.name,
        @wtpartNumberB5 = m.WTPartNumber
    FROM [{scheman}].[WTPartMaster] m
    WHERE m.idA2A2 = @idA3masterReferenceB5;


	---Nul Control
	IF @idA3A5 IS NULL OR @stateStateA5 IS NULL OR @wtpartNumberA5 IS NULL OR @stateStateB5 IS NULL OR @idA3B5 IS NULL OR @wtpartNumberB5 IS NULL
    BEGIN
        RETURN;
    END
	---Nul Control	

	IF @idA3masterReferenceA5 <> @idA3masterReferenceB5
	BEGIN
    IF EXISTS (
        SELECT 1 
        FROM [{scheman}].[Des_EquivalenceLink_LogTable]
        WHERE APartMasterID = @idA3masterReferenceA5 AND BPartMasterID = @idA3masterReferenceB5
    )
    BEGIN
        -- Aynı değerler varsa güncelleme yapıyoruz
        UPDATE [{scheman}].[Des_EquivalenceLink_LogTable]
        SET 
			idA3A6 = @idA3A6,
			idA3B6 = @idA3B6,
			idA3A5 = @idA3A5,
			idA3B5 = @idA3B5,
            iterationIdA2iterationInfo = @iterationIdA2iterationInfoA5,
            versionIdA2versionInfo = @versionIdA2versionInfoA5,
            APartState = @stateStateA5,
            APartName = @nameA5,
            ApartWtNumber = @wtpartNumberA5,
            iterationIdA2iterationInfoB = @iterationIdA2iterationInfoB5,
            versionIdA2versionInfoB = @versionIdA2versionInfoB5,
            BPartState = @stateStateB5,
            BPartName = @nameB5,
            BpartWtNumber = @wtpartNumberB5
        WHERE APartMasterID = @idA3masterReferenceA5 AND BPartMasterID = @idA3masterReferenceB5;
    END
    ELSE
    BEGIN
        INSERT INTO [{scheman}].[Des_EquivalenceLink_LogTable] (
            classnamekeyroleAObjectRef, idA3A5, idA3A6, iterationIdA2iterationInfo, 
            versionIdA2versionInfo, APartState, APartMasterID, APartName, ApartWtNumber, 
            classnamekeyroleBObjectRef, idA3B5, idA3B6, iterationIdA2iterationInfoB, 
            versionIdA2versionInfoB, BPartState, BPartMasterID, BPartName, BpartWtNumber, idA2A2)
        VALUES (
            'roleA', @idA3A5, @idA3A6, @iterationIdA2iterationInfoA5, 
            @versionIdA2versionInfoA5, @stateStateA5, @idA3masterReferenceA5, @nameA5, @wtpartNumberA5, 
            'roleB', @idA3B5, @idA3B6, @iterationIdA2iterationInfoB5, 
            @versionIdA2versionInfoB5, @stateStateB5, @idA3masterReferenceB5, @nameB5, @wtpartNumberB5, NULL);
			------------------------------------------------------------
			     INSERT INTO [{scheman}].[Des_EquivalenceLink_LogTable_Takip] (
            classnamekeyroleAObjectRef, idA3A5, idA3A6, iterationIdA2iterationInfo, 
            versionIdA2versionInfo, APartState, APartMasterID, APartName, ApartWtNumber, 
            classnamekeyroleBObjectRef, idA3B5, idA3B6, iterationIdA2iterationInfoB, 
            versionIdA2versionInfoB, BPartState, BPartMasterID, BPartName, BpartWtNumber, idA2A2)
        VALUES (
            'roleA', @idA3A5, @idA3A6, @iterationIdA2iterationInfoA5, 
            @versionIdA2versionInfoA5, @stateStateA5, @idA3masterReferenceA5, @nameA5, @wtpartNumberA5, 
            'roleB', @idA3B5, @idA3B6, @iterationIdA2iterationInfoB5, 
            @versionIdA2versionInfoB5, @stateStateB5, @idA3masterReferenceB5, @nameB5, @wtpartNumberB5, NULL);
    END;
END
END;
";


			#endregion

			#endregion


			#region EPMDocument Bölümü

			#region EPMDocument Tables
			string Des_EPMDocument_LogTable = $@"
CREATE TABLE [{scheman}].[Des_EPMDocument_LogTable] (
    [Ent_ID] BIGINT IDENTITY(1, 1) NOT NULL,
    [EPMDocID] BIGINT NULL,
    [StateDegeri] NVARCHAR(200) NULL,
    [idA3masterReference] BIGINT NULL,
    [CadName] NVARCHAR(200) NULL,
    [name] NVARCHAR(200) NULL,
    [docNumber] NVARCHAR(200) NULL,
    CONSTRAINT [PK_Des_EPMDocument_LogTable] PRIMARY KEY CLUSTERED ([Ent_ID] ASC)
) ON [PRIMARY];
";

			string Des_EPMDocument_LogTable_Cancelled = $@"
CREATE TABLE [{scheman}].[Des_EPMDocument_LogTable_Cancelled] (
    [Ent_ID] BIGINT IDENTITY(1, 1) NOT NULL,
    [EPMDocID] BIGINT NULL,
    [StateDegeri] NVARCHAR(200) NULL,
    [idA3masterReference] BIGINT NULL,
    [CadName] NVARCHAR(200) NULL,
    [name] NVARCHAR(200) NULL,
    [docNumber] NVARCHAR(200) NULL,
    CONSTRAINT [PK_Des_EPMDocument_LogTable_Cancelled] PRIMARY KEY CLUSTERED ([Ent_ID] ASC)
) ON [PRIMARY];
";

			string Des_EPMDocument_LogTable_Cancelled_Error = $@"
CREATE TABLE [{scheman}].[Des_EPMDocument_LogTable_Cancelled_Error] (
    [Ent_ID] BIGINT IDENTITY(1, 1) NOT NULL,
    [EPMDocID] BIGINT NULL,
    [StateDegeri] NVARCHAR(200) NULL,
    [idA3masterReference] BIGINT NULL,
    [CadName] NVARCHAR(200) NULL,
    [name] NVARCHAR(200) NULL,
    [docNumber] NVARCHAR(200) NULL,
    CONSTRAINT [PK_Des_EPMDocument_LogTable_Cancelled_Error] PRIMARY KEY CLUSTERED ([Ent_ID] ASC)
) ON [PRIMARY];
";

			string Des_EPMDocument_LogTable_Cancelled_Takip = $@"
CREATE TABLE [{scheman}].[Des_EPMDocument_LogTable_Cancelled_Takip] (
    [Ent_ID] BIGINT IDENTITY(1, 1) NOT NULL,
    [EPMDocID] BIGINT NULL,
    [StateDegeri] NVARCHAR(200) NULL,
    [idA3masterReference] BIGINT NULL,
    [CadName] NVARCHAR(200) NULL,
    [name] NVARCHAR(200) NULL,
    [docNumber] NVARCHAR(200) NULL,
    CONSTRAINT [PK_Des_EPMDocument_LogTable_Cancelled_Takip] PRIMARY KEY CLUSTERED ([Ent_ID] ASC)
) ON [PRIMARY];
";

			string Des_EPMDocument_LogTable_Error = $@"
CREATE TABLE [{scheman}].[Des_EPMDocument_LogTable_Error] (
    [Ent_ID] BIGINT IDENTITY(1, 1) NOT NULL,
    [EPMDocID] BIGINT NULL,
    [StateDegeri] NVARCHAR(200) NULL,
    [idA3masterReference] BIGINT NULL,
    [CadName] NVARCHAR(200) NULL,
    [name] NVARCHAR(200) NULL,
    [docNumber] NVARCHAR(200) NULL,
    CONSTRAINT [PK_Des_EPMDocument_LogTable_Error] PRIMARY KEY CLUSTERED ([Ent_ID] ASC)
) ON [PRIMARY];
";

			string Des_EPMDocument_LogTable_Takip = $@"
CREATE TABLE [{scheman}].[Des_EPMDocument_LogTable_Takip] (
    [Ent_ID] BIGINT IDENTITY(1, 1) NOT NULL,
    [EPMDocID] BIGINT NULL,
    [StateDegeri] NVARCHAR(200) NULL,
    [idA3masterReference] BIGINT NULL,
    [CadName] NVARCHAR(200) NULL,
    [name] NVARCHAR(200) NULL,
    [docNumber] NVARCHAR(200) NULL,
    CONSTRAINT [PK_Des_EPMDocument_LogTable_Takip] PRIMARY KEY CLUSTERED ([Ent_ID] ASC)
) ON [PRIMARY];
";
			#endregion

			#region EPMDocument Trigger
			string Des_EPMDocument_LogTable_Durum_Control = $@"
CREATE TRIGGER [{scheman}].[Des_EPMDocument_LogTable_Durum_Control]
ON [{scheman}].[Des_EPMDocument_LogTable]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [{scheman}].[Des_EPMDocument_LogTable_Cancelled]
    WHERE
        [EPMDocID] IN (SELECT [EPMDocID] FROM inserted);
END;";

			string Des_EPMDocument_LogTable_Error_Control = $@"
CREATE TRIGGER [{scheman}].[Des_EPMDocument_LogTable_Error_Control]
ON [{scheman}].[Des_EPMDocument_LogTable]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [{scheman}].[Des_EPMDocument_LogTable_Error]
    WHERE
        [EPMDocID] IN (SELECT [EPMDocID] FROM inserted);
END;";

			string Des_EPMDocument_LogTable_Cancelled_Durum_Control = $@"
CREATE TRIGGER [{scheman}].[Des_EPMDocument_LogTable_Cancelled_Durum_Control]
ON [{scheman}].[Des_EPMDocument_LogTable_Cancelled]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [{scheman}].[Des_EPMDocument_LogTable]
    WHERE
        [EPMDocID] IN (SELECT [EPMDocID] FROM inserted);
END;";

			string Des_EPMDocument_LogTable_Cancelled_Error_Control = $@"
CREATE TRIGGER [{scheman}].[Des_EPMDocument_LogTable_Cancelled_Error_Control]
ON [{scheman}].[Des_EPMDocument_LogTable_Cancelled]
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM [{scheman}].[Des_EPMDocument_LogTable_Cancelled_Error]
    WHERE
        [EPMDocID] IN (SELECT [EPMDocID] FROM inserted);
END;";

			string EPMDokumanState = $@"
CREATE TRIGGER [{scheman}].[EPMDokumanState]
ON [{scheman}].[EPMDocument]
AFTER UPDATE
AS
BEGIN
    DECLARE @EPMDocumentID BIGINT,
            @idA3masterReference BIGINT,
            @StateDegeri NVARCHAR(200),
            @CadName NVARCHAR(200),
            @name NVARCHAR(200),
            @docNumber NVARCHAR(200);

    DECLARE @empResult INT = 0;

    SELECT @EPMDocumentID = idA2A2, @StateDegeri = statestate, @idA3masterReference = idA3masterReference FROM inserted;

    SELECT @CadName = CADName, @name = name, @docNumber = documentNumber FROM {scheman}.EPMDocumentMaster WHERE idA2A2 = @idA3masterReference;

    --wrk Control
    IF EXISTS (SELECT 1 FROM [{scheman}].EPMDocument WHERE idA3masterReference = @idA3masterReference AND statecheckoutInfo = 'wrk')
    BEGIN
        SET @empResult = 1;
    END
    ELSE
    BEGIN
        SET @empResult = ISNULL(@empResult, 2);
    END
    --wrk Control

    IF @StateDegeri = 'RELEASED' AND @empResult = 0
    BEGIN
        IF EXISTS (SELECT 1 FROM {scheman}.EPMReferenceLink WHERE idA3A5 = @EPMDocumentID AND referenceType = 'DRAWING')
        BEGIN
            IF EXISTS (SELECT 1 FROM {scheman}.Des_EPMDocument_LogTable WHERE idA3masterReference = @idA3masterReference)
            BEGIN
                UPDATE {scheman}.Des_EPMDocument_LogTable
                SET StateDegeri = @StateDegeri,
                    EPMDocID = @EPMDocumentID,
                    CadName = @CadName,
                    name = @name,
                    docNumber = @docNumber
                WHERE idA3masterReference = @idA3masterReference;
            END
            ELSE
            BEGIN
                INSERT INTO {scheman}.Des_EPMDocument_LogTable (EPMDocID, StateDegeri, idA3masterReference, CadName, name, docNumber)
                VALUES (@EPMDocumentID, @StateDegeri, @idA3masterReference, @CadName, @name, @docNumber);
            END
        END
    END
END;";

			string EPMDokumanState_CANCELLED = $@"
CREATE TRIGGER [{scheman}].[EPMDokumanState_CANCELLED]
ON [{scheman}].[EPMDocument]
AFTER UPDATE
AS
BEGIN
    DECLARE @EPMDocumentID BIGINT,
            @idA3masterReference BIGINT,
            @StateDegeri NVARCHAR(200),
            @CadName NVARCHAR(200),
            @name NVARCHAR(200),
            @docNumber NVARCHAR(200);

    DECLARE @empResult INT = 0;

    SELECT @EPMDocumentID = idA2A2, @StateDegeri = statestate, @idA3masterReference = idA3masterReference FROM inserted;

    SELECT @CadName = CADName, @name = name, @docNumber = documentNumber FROM {scheman}.EPMDocumentMaster WHERE idA2A2 = @idA3masterReference;

    --wrk Control
    IF EXISTS (SELECT 1 FROM [{scheman}].EPMDocument WHERE idA3masterReference = @idA3masterReference AND statecheckoutInfo = 'wrk')
    BEGIN
        SET @empResult = 1;
    END
    ELSE
    BEGIN
        SET @empResult = ISNULL(@empResult, 2);
    END
    --wrk Control

    IF @StateDegeri = 'CANCELLED' AND @empResult = 0
    BEGIN
        IF EXISTS (SELECT 1 FROM {scheman}.EPMReferenceLink WHERE idA3A5 = @EPMDocumentID AND referenceType = 'DRAWING')
        BEGIN
            IF EXISTS (SELECT 1 FROM {scheman}.Des_EPMDocument_LogTable_Cancelled WHERE idA3masterReference = @idA3masterReference)
            BEGIN
                UPDATE {scheman}.Des_EPMDocument_LogTable_Cancelled
                SET StateDegeri = @StateDegeri,
                    EPMDocID = @EPMDocumentID,
                    CadName = @CadName,
                    name = @name,
                    docNumber = @docNumber
                WHERE idA3masterReference = @idA3masterReference;
            END
            ELSE
            BEGIN
                INSERT INTO {scheman}.Des_EPMDocument_LogTable_Cancelled (EPMDocID, StateDegeri, idA3masterReference, CadName, name, docNumber)
                VALUES (@EPMDocumentID, @StateDegeri, @idA3masterReference, @CadName, @name, @docNumber);
            END
        END
    END
END;";

			string EPM_Document = $@"
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
		--OR (versionIdA2versionInfo = 'A' AND iterationIdA2iterationInfo = 1)) bu bölümü sonradan ekledim nx ile creo dan oluşanların hepsi A.1 oluyor muş bu şartla belki revise olayını keseriz
IF EXISTS (SELECT 1 FROM [{scheman}].EPMDocument WHERE idA3masterReference = @idA3masterReferenceControl AND statecheckoutInfo = 'wrk')
BEGIN
    SET @empResult = 1;
END
ELSE
BEGIN
    SET @empResult = ISNULL(@empResult, 2); 
END;

IF EXISTS (SELECT 1 FROM [{scheman}].EPMDocument WHERE idA2A2 = @idA3A5 AND versionIdA2versionInfo <> 'A')
BEGIN
    SET @reviseControl = 1;
END
ELSE
BEGIN
    SET @reviseControl = ISNULL(@reviseControl, 2); 
END;

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
        SELECT @toplamSayi = COUNT(*)
        FROM [{scheman}].Des_CadDocumentBaglaLog
        WHERE mainIdA2A2 = @idA3B5;

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


END;";

			#endregion

			#endregion




			#region AlterneLinkLog için kullanılacak sql komutu

			string Des_LogDataReviseAndSaveAsProcess = $@"
                CREATE TABLE [{scheman}].[Des_LogDataReviseAndSaveAsProcess](
	[LogID] [int] IDENTITY(1,1) NOT NULL,
	[statestate] [nvarchar](100) NULL,
	[PartID] [bigint] NULL,
	[AnaNumber] [nvarchar](50) NULL,
	[AnaParcaAd] [nvarchar](100) NULL,
	[Number] [nvarchar](50) NULL,
	[ParcaAd] [nvarchar](150) NULL,
	[Version] [nchar](30) NULL,
	[KulAd] [nvarchar](50) NULL,
	[LogCode] [nvarchar](300) NULL,
	[LogMesaj] [nvarchar](300) NULL,
 CONSTRAINT [PK_Des_LogDataReviseAndSaveAsProcess] PRIMARY KEY CLUSTERED ([LogID] ASC));";

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
				connection.Open(); // Bağlantıyı sadece bir kez açın

				#region WTPart Bölümü

				ExecuteSqlCommand(connection, Des_WTPart_LogTable);
				ExecuteSqlCommand(connection, Des_WTPart_LogTable_Error);
				ExecuteSqlCommand(connection, Des_WTPart_LogTable_Takip);
				ExecuteSqlCommand(connection, Des_WTPart_LogTable_Error_Control);
				ExecuteSqlCommand(connection, Part_Released);
				ExecuteSqlCommand(connection, Part_ReviseAndSaveAsClean);
				ExecuteSqlCommand(connection, Part_EquivalenceLink_Control);

				#region WTPartAlternateLink Trigger
				ExecuteSqlCommand(connection, MuadilTakip);
				#endregion

				#region WTPart ReferenceLink Trigger
				ExecuteSqlCommand(connection, Part_Document);
				#endregion

				#region WTPART- MKI
				ExecuteSqlCommand(connection, Des_PartDocumentBagla);
				ExecuteSqlCommand(connection, Des_PartDocumentBaglalOG);
				#endregion

				#endregion

				#region WTPart Alternate Bölümü
				ExecuteSqlCommand(connection, Des_AlternateLink_LogTable);
				ExecuteSqlCommand(connection, Des_AlternateLink_LogTable_Error);
				ExecuteSqlCommand(connection, Des_AlternateLink_LogTable_Takip);
				ExecuteSqlCommand(connection, Des_AlternateLinkRemoved_LogTable);
				ExecuteSqlCommand(connection, Des_AlternateLinkRemoved_LogTable_Error);

				#region Alternate Trigger
				ExecuteSqlCommand(connection, Des_AlternateLink_EntegrasyonControl);
				ExecuteSqlCommand(connection, Des_AlternateLink_LogTable_Error_Control);
				ExecuteSqlCommand(connection, Des_AlternateLinkRemoved_LogTable_Error_Control);
				#endregion

				#endregion

				#region WTPart EquivalenceLink Bölümü
				ExecuteSqlCommand(connection, Des_EquivalenceLink_LogTable);
				ExecuteSqlCommand(connection, Des_EquivalenceLink_LogTable_Error);
				ExecuteSqlCommand(connection, Des_EquivalenceLink_LogTable_Takip);

				#region WTPart EquivalenceLink Trigger
				ExecuteSqlCommand(connection, Des_EquivalenceLink_EntegrarsyonControl);
				ExecuteSqlCommand(connection, Des_EquivalenceLink_LogTable_Error_Control);
				ExecuteSqlCommand(connection, Des_EquivalenceLink);
				#endregion

				#endregion

				#region EPMDocument Bölümü
				ExecuteSqlCommand(connection, Des_EPMDocument_LogTable);
				ExecuteSqlCommand(connection, Des_EPMDocument_LogTable_Cancelled);
				ExecuteSqlCommand(connection, Des_EPMDocument_LogTable_Cancelled_Error);
				ExecuteSqlCommand(connection, Des_EPMDocument_LogTable_Cancelled_Takip);
				ExecuteSqlCommand(connection, Des_EPMDocument_LogTable_Error);
				ExecuteSqlCommand(connection, Des_EPMDocument_LogTable_Takip);

				#region EPMDocument Trigger Bölümü
				ExecuteSqlCommand(connection, Des_EPMDocument_LogTable_Durum_Control);
				ExecuteSqlCommand(connection, Des_EPMDocument_LogTable_Error_Control);
				ExecuteSqlCommand(connection, Des_EPMDocument_LogTable_Cancelled_Durum_Control);
				ExecuteSqlCommand(connection, Des_EPMDocument_LogTable_Cancelled_Error_Control);
				ExecuteSqlCommand(connection, EPMDokumanState);
				ExecuteSqlCommand(connection, EPMDokumanState_CANCELLED);
				ExecuteSqlCommand(connection, EPM_Document);
				#endregion

				#region EPM ALTERNATE LINK - CZM
				ExecuteSqlCommand(connection, Des_CadDocumentBagla);
				ExecuteSqlCommand(connection, Des_CadDocumentBaglaLog);
				#endregion

				#endregion

				#region ConnectionSettings
				ExecuteSqlCommand(connection, Des_LogDataReviseAndSaveAsProcess);
				ExecuteSqlCommand(connection, createTableSql);
				ExecuteSqlCommand(connection, createTableSql2);
				ExecuteSqlCommand(connection, createTableSql3);
				#endregion

				#region EPMDocument Ayarları
				ExecuteSqlCommand(connection, Des_EPMDocAttachmentsLog);
				#endregion

				#region Kullanıcı Ayarları
				ExecuteSqlCommand(connection, Des_Kullanici);
				ExecuteSqlCommand(connection, Des_KulYetki);
				#endregion
			}
			}

// Yardımcı method
private void ExecuteSqlCommand(SqlConnection connection, string commandText)
		{
			using (var command = new SqlCommand(commandText, connection))
			{
				try
				{
					command.ExecuteNonQuery();
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Hata: {ex.Message}");
				}
			}
		}


		//using (var connection = new SqlConnection(connectionString))
		//{


		//	#region WTPart Bölümü

		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_WTPart_LogTable, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}

		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_WTPart_LogTable_Error, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}

		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_WTPart_LogTable_Takip, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}		
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Part_Released, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Part_ReviseAndSaveAsClean, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}

		//	connection.Open();
		//	using (var command1 = new SqlCommand(Part_EquivalenceLink_Control, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}

		//	#region WTPartAlternateLink Trigger
		//	connection.Open();
		//	using (var command1 = new SqlCommand(MuadilTakip, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}



		//	#endregion

		//	#region WTPart ReferenceLink Trigger
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Part_Document, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	#endregion




		//	#region WTPART- MKI
		//	using (var command2 = new SqlCommand(Des_PartDocumentBagla, connection))
		//	{
		//		try
		//		{
		//			command2.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	using (var command2 = new SqlCommand(Des_PartDocumentBaglalOG, connection))
		//	{
		//		try
		//		{
		//			command2.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	#endregion
		//	#endregion


		//	#region WTPart Alternate Bölümü
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_AlternateLink_LogTable, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_AlternateLink_LogTable_Error, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_AlternateLink_LogTable_Takip, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_AlternateLinkRemoved_LogTable, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_AlternateLinkRemoved_LogTable_Error, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}


		//	#region Alternate Trigger
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_AlternateLink_EntegrasyonControl, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_AlternateLink_LogTable_Error_Control, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_AlternateLinkRemoved_LogTable_Error_Control, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}


		//	#endregion




		//	#endregion

		//	#region WTPart EquivalenceLink Bölümü
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_EquivalenceLink_LogTable, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_EquivalenceLink_LogTable_Error, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_EquivalenceLink_LogTable_Takip, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}

		//	#region WTPart EquivalenceLink Trigger
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_EquivalenceLink_EntegrarsyonControl, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_EquivalenceLink_LogTable_Error_Control, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_EquivalenceLink, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	#endregion

		//	#endregion


		//	#region EPMDocument Bölümü
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_EPMDocument_LogTable, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_EPMDocument_LogTable_Cancelled, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_EPMDocument_LogTable_Cancelled_Error, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_EPMDocument_LogTable_Cancelled_Takip, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_EPMDocument_LogTable_Error, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_EPMDocument_LogTable_Takip, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}


		//	#region EPMDocument Trigger Bölümü
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_EPMDocument_LogTable_Durum_Control, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_EPMDocument_LogTable_Error_Control, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_EPMDocument_LogTable_Cancelled_Durum_Control, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(Des_EPMDocument_LogTable_Cancelled_Error_Control, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(EPMDokumanState, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(EPMDokumanState_CANCELLED, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	connection.Open();
		//	using (var command1 = new SqlCommand(EPM_Document, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}

		//	#endregion



		//	#region EPM ALTERNATE LINK - CZM
		//	using (var command2 = new SqlCommand(Des_CadDocumentBagla, connection))
		//	{
		//		try
		//		{
		//			command2.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}

		//	using (var command2 = new SqlCommand(Des_CadDocumentBaglaLog, connection))
		//	{
		//		try
		//		{
		//			command2.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	#endregion
		//	#endregion



		//	#region ConnectionSettings

		//	connection.Open();
		//	using (var command1 = new SqlCommand(createTableSql, connection))
		//	{
		//		try
		//		{
		//			command1.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}

		//	using (var command2 = new SqlCommand(createTableSql2, connection))
		//	{
		//		try
		//		{
		//			command2.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	using (var command2 = new SqlCommand(createTableSql3, connection))
		//	{
		//		try
		//		{
		//			command2.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}




		//	#endregion


		//	#region EPMDocument Ayarları



		//	using (var command2 = new SqlCommand(Des_EPMDocAttachmentsLog, connection))
		//	{
		//		try
		//		{
		//			command2.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}

		//	#endregion

		//	#region Kullanıcı Ayarları

		//	using (var command2 = new SqlCommand(Des_Kullanici, connection))
		//	{
		//		try
		//		{
		//			command2.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	using (var command2 = new SqlCommand(Des_KulYetki, connection))
		//	{
		//		try
		//		{
		//			command2.ExecuteNonQuery();
		//		}
		//		catch (Exception)
		//		{
		//		}
		//	}
		//	#endregion


		//}

	}


	}
