using Dapper;
using Designtech_PLM_Entegrasyon_AutoPost.ApiServices;
using Designtech_PLM_Entegrasyon_AutoPost.Helper;
using Designtech_PLM_Entegrasyon_AutoPost.Model;
using Designtech_PLM_Entegrasyon_AutoPost.Model.WindchillApiModel;
using Designtech_PLM_Entegrasyon_AutoPost.ViewModel;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2
{
	public partial class Form1 : Form
	{
		private readonly IConfiguration _configuration;
		private readonly IDbConnection conn;
		private System.Windows.Forms.Timer timer;
		private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(Timeout.InfiniteTimeSpan);
		private bool _isRunning;

		public Form1(ApiService apiService, IDbConnection db, IConfiguration configuration)
		{
			conn = db;
			_configuration = configuration;
		}

		public Form1()
		{
			InitializeComponent();
			ShowData();
			DisplayJsonDataInListBox();
			UpdateLogList();
			lblDataCount.Text = listBox1.Items.Count.ToString();
			// Kapatma ayarlarý
			FormClosing += Form1_FormClosing;
			Resize += Form1_Resize;




		}


		public class postDeneme
		{
			public string title { get; set; }
			public string description { get; set; }
			public string imageUrl { get; set; }
		}

		static string GetLocalIPAddress()
		{
			string localIP = "?";

			try
			{
				// Dns sýnýfýný kullanarak bilgisayarýn IP adresini alabiliriz.
				IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

				foreach (IPAddress ip in host.AddressList)
				{
					// IPv4 adresini seçiyoruz.
					if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
					{
						localIP = ip.ToString();
						break;
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("IP adresi alýnýrken bir hata oluþtu: " + ex.Message);
			}

			return localIP;
		}
		private void rbServerChoose_CheckedChanged(object sender, EventArgs e)
		{
			try
			{

				txtParola.Enabled = true;
				txtKullaniciAdi.Enabled = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show("HATA !", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void rbLocalChoose_CheckedChanged(object sender, EventArgs e)
		{
			try
			{

				txtParola.Enabled = false;
				txtKullaniciAdi.Enabled = false;
			}
			catch (Exception ex)
			{
				MessageBox.Show("HATA !", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private async void btnBaglantiKur_Click(object sender, EventArgs e)
		{
			try
			{

				LogService logService = new LogService(_configuration);
				string ipAddress = GetLocalIPAddress();



				string connectionString = "";

				if (rbServerChoose.Checked)
				{
					connectionString = $"Persist Security Info=False;User ID={txtKullaniciAdi.Text};Password={txtParola.Text};Initial Catalog={txtDatabaseAdi.Text};Server={txtServerName.Text};TrustServerCertificate=True";
				}
				else if (rbLocalChoose.Checked)
				{
					connectionString = $"Data Source={txtServerName.Text};Initial Catalog={txtDatabaseAdi.Text};Integrated Security=True;TrustServerCertificate=True";
				}

				bool connectionSuccess = await TestDatabaseConnection(connectionString);

				if (connectionSuccess)
				{
					//Baðlantý baþarýlý

					//appsettings.json yerine burada kendi konfigürasyon dosyanýza yazýn
					SaveConnectionString(connectionString);


					string directoryPath = "ConnectionData";
					string fileName = "appsettings.json";
					string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

					// Klasör yoksa oluþtur
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
					var catalogValue = jsonObject["Catalog"].ToString();
					// Tablo varsa kontrol et
					var tableExists = TableExists($"[{catalogValue}].Change_Notice_LogTable", connectionString);

					if (!tableExists)
					{
						// Tablo yoksa oluþtur
						CreateTable(connectionString);
					}


					//Baðlantý bilgilerini gösteren fonksiyon
					ShowData();
					//Diðer iþlemler

					logService.AddNewLogEntry("Baðlantý baþarýyla kuruldu.", null, "Baþarýlý", ipAddress);
					MessageBox.Show("Baðlantý baþarýlý");
				}
				else
				{
					logService.AddNewLogEntry("Baðlanto baþarýsýz!.", null, "HATA", ipAddress);
					//Baðlantý baþarýsýz
					MessageBox.Show("Baðlantý baþarýsýz");
				}
			}
			catch (Exception ex)
			{
				LogService logService = new LogService(_configuration);
				string ipAddress = GetLocalIPAddress();
				logService.AddNewLogEntry("HATA!." + ex.Message, null, "HATA", ipAddress);
				MessageBox.Show("HATA !", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private bool TableExists(string tableName, string connectionString)
		{

			using (var connection = new SqlConnection(connectionString))
			using (var command = connection.CreateCommand())
			{
				command.CommandText = $"IF OBJECT_ID('{tableName}', 'U') IS NULL SELECT 0 ELSE SELECT 1";
				connection.Open();
				return (int)command.ExecuteScalar() == 1;
			}
		}

		private void CreateTable(string connectionString)
		{

			string directoryPath = "ConnectionData";
			string fileName = "appsettings.json";
			string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

			// Klasör yoksa oluþtur
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
			var scheman = jsonObject["Catalog"].ToString();

			string createTableSql = @"
    CREATE TABLE " + scheman + @".Change_Notice_LogTable (
      idA2A2 varchar(50),
      idA3masterReference varchar(MAX),
      statestate varchar(MAX),
      name varchar(MAX),
      WTPartNumber varchar(MAX),
      updateStampA2 datetime,
      ProcessTimestamp datetime
 Version varchar(MAX),
 VersionID varchar(MAX)
    )";



			//            string createTableSql = @"
			//    CREATE TABLE " + scheman + @".Change_Notice_LogTable (
			//    ProcessTimestamp datetime,
			//    CreatedOn datetime,
			//    ID varchar(MAX),
			//    LastModified datetime,
			//    AlternateNumber varchar(MAX), 
			//    AssemblyModeValue varchar(MAX),
			//    AssemblyModeDisplay varchar(MAX),
			//    CADName varchar(MAX),
			//    CabinetName varchar(MAX),
			//    CheckOutStatus varchar(MAX),
			//    CheckoutState varchar(MAX),
			//    Comments varchar(MAX),
			//    ComponentType varchar(MAX),
			//    ConfigurableModuleValue varchar(MAX),
			//    ConfigurableModuleDisplay varchar(MAX),
			//    CreatedBy varchar(MAX), 
			//    DefaultTraceCodeValue varchar(MAX),
			//    DefaultTraceCodeDisplay varchar(MAX),
			//    DefaultUnitValue varchar(MAX),
			//    DefaultUnitDisplay varchar(MAX),
			//    DenemeNX varchar(MAX),
			//    Description varchar(MAX),
			//    EndItem bit,
			//    FolderLocation varchar(MAX),
			//    FolderName varchar(MAX),
			//    GatheringPart bit, 
			//    GeneralStatus varchar(MAX),
			//	[Identity] varchar(MAX),
			//    KaleKod varchar(MAX),
			//    Kaleargenumber varchar(MAX),
			//    Latest bit,
			//    Length varchar(MAX),
			//    LifeCycleTemplateName varchar(MAX),
			//    Material varchar(MAX),
			//    ModifiedBy varchar(MAX),
			//    NAME10 varchar(MAX),
			//    NAME20 varchar(MAX), 
			//    NAME201_PTCC_MultipleAliasAttributeValues varchar(MAX),
			//    NAME201 varchar(MAX),
			//    Name varchar(MAX),
			//    Name30 varchar(MAX),
			//	OEMPartSourcingStatus varchar(MAX),
			//    Number varchar(MAX),
			//    ObjectType varchar(MAX),
			//    OrganizationReference varchar(MAX),
			//    PARCAADI varchar(MAX),
			//    PTCWMNAME varchar(MAX),
			//    PhantomManufacturingPart bit,
			//    Revision varchar(MAX),
			//    SourceValue varchar(MAX),
			//    SourceDisplay varchar(MAX),
			//    SourceDuplicate varchar(MAX),
			//    Standard varchar(MAX),
			//    StateValue varchar(MAX),
			//    StateDisplay varchar(MAX),
			//    Supersedes varchar(MAX),
			//    Supplier varchar(MAX),
			//    Thickness varchar(MAX), 
			//    TypeIconPath varchar(MAX),
			//    TypeIconTooltip varchar(MAX),
			//    Version varchar(MAX),
			//    VersionID varchar(MAX),
			//    [View] varchar(MAX), 
			//    WorkInProgressStateValue varchar(MAX),
			//    WorkInProgressStateDisplay varchar(MAX)
			//)";



			//        CREATE TABLE dbo.Change_Notice_LogTable(
			//  idA2A2 varchar(50),
			//  idA3masterReference varchar(MAX),
			//  statestate varchar(MAX),
			//   name varchar(MAX),
			//    WTPartNumber varchar(MAX),
			//  updateStampA2 datetime,
			//  ProcessTimestamp datetime
			//)

			using (var connection = new SqlConnection(connectionString))
			using (var command = new SqlCommand(createTableSql, connection))
			{
				connection.Open();
				command.ExecuteNonQuery();
			}
		}

		private async Task<bool> TestDatabaseConnection(string connectionString)
		{
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				try
				{
					await connection.OpenAsync();
					return true;
				}
				catch
				{
					return false;
				}
			}
		}


		private void SaveConnectionString(string connectionString)
		{
			try
			{

				string directoryPath = "ConnectionData";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

				// Klasör yoksa oluþtur
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
				if (string.IsNullOrWhiteSpace(jsonData))
				{
					// JSON verisi boþsa yeni bir nesne oluþtur
					jsonObject = new JObject
					{
						["Catalog"] = "",
						["ServerName"] = "",
						["KullaniciAdi"] = "",
						["Parola"] = "",
						["ConnectionStrings"] = new JObject
						{
							["Plm"] = ""
						},
						["APIConnectionINFO"] = new JObject
						{
							["ApiURL"] = "",
							["ApiEndpoint"] = "",
							["API"] = "",
							["CSRF_NONCE"] = "",
							["Username"] = "",
							["Password"] = ""
						},
						["ConnectionType"] = false,
					};
				}
				else
				{
					// JSON verisi mevcutsa, onu bir nesneye çevir
					jsonObject = JObject.Parse(jsonData);
				}

				// Yeni veriyi JSON nesnesine ekle veya güncelle
				jsonObject["Catalog"] = txtDatabaseAdi.Text;
				jsonObject["ServerName"] = txtServerName.Text;
				jsonObject["KullaniciAdi"] = txtKullaniciAdi.Text;
				jsonObject["Parola"] = txtParola.Text;
				jsonObject["ConnectionStrings"]["Plm"] = connectionString;

				// JSON nesnesini dosyaya geri yaz
				File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonObject, Formatting.Indented));

			}
			catch (Exception ex)
			{
				MessageBox.Show("HATA !", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		private void ShowData()
		{
			try
			{

				// JSON dosyasýndaki verileri çek
				string directoryPath = "ConnectionData";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

				if (File.Exists(filePath))
				{
					string jsonData = File.ReadAllText(filePath);
					JObject jsonObject = JObject.Parse(jsonData);

					// Ýlgili verileri çek ve kullanýcýya göster
					txtShowServerName.Text = jsonObject["ServerName"].ToString();
					txtShowCatalog.Text = jsonObject["Catalog"].ToString();
					txtShowApiURL.Text = jsonObject["APIConnectionINFO"]["API"].ToString();
				}
				else
				{
					// Dosya yoksa veya okunamazsa hata mesajý göster
					MessageBox.Show("appsettings.json dosyasý bulunamadý veya okunamadý.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("HATA !", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		private void groupBox1_Enter(object sender, EventArgs e)
		{

		}

		private void Form1_Load(object sender, EventArgs e)
		{
			try
			{

				if (this.WindowState == FormWindowState.Minimized)
				{
					this.Hide();
					ShowData();
					notifyIcon1.Visible = true;
				}
				else if (this.WindowState == FormWindowState.Normal)
				{
					ShowData();
					notifyIcon1.Visible = false;
				}

			}
			catch (Exception ex)
			{
				MessageBox.Show("HATA !", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void btnApiEkle_Click(object sender, EventArgs e)
		{
			try
			{

				string directoryPath = "ConnectionData";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

				// Klasör yoksa oluþtur
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
				if (string.IsNullOrWhiteSpace(jsonData))
				{
					// JSON verisi boþsa yeni bir nesne oluþtur
					jsonObject = new JObject
					{


						["Catalog"] = "",
						["ServerName"] = "",
						["KullaniciAdi"] = "",
						["Parola"] = "",
						["ConnectionStrings"] = new JObject
						{
							["Plm"] = ""
						},
						["APIConnectionINFO"] = new JObject
						{
							["ApiURL"] = "",
							["ApiEndpoint"] = "",
							["API"] = "",
							["CSRF_NONCE"] = "",
							["Username"] = "",
							["Password"] = ""

						},
						["ConnectionType"] = false,
					};
				}
				else
				{
					// JSON verisi mevcutsa, onu bir nesneye çevir
					jsonObject = JObject.Parse(jsonData);
				}

				// Yeni veriyi JSON nesnesine ekle veya güncelle

				jsonObject["APIConnectionINFO"]["ApiURL"] = txtApiUrl.Text;
				jsonObject["APIConnectionINFO"]["ApiEndpoint"] = txtApiEndpoint.Text;
				jsonObject["APIConnectionINFO"]["API"] = txtApiUrl.Text + "/" + txtApiEndpoint.Text;
				jsonObject["APIConnectionINFO"]["CSRF_NONCE"] = txt_CSRF_NONCE.Text;
				jsonObject["APIConnectionINFO"]["Username"] = txtBasicUsername.Text;
				jsonObject["APIConnectionINFO"]["Password"] = txtBasicPassword.Text;


				// JSON nesnesini dosyaya geri yaz
				File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonObject, Formatting.Indented));


				MessageBox.Show("Api sisteme kayýt edildi.");
				notificatonSettings("Api sisteme kayýt edildi.");
				ShowData();
			}
			catch (Exception ex)
			{
				MessageBox.Show("HATA !", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		private void button1btnStopAutoPost_Click(object sender, EventArgs e)
		{
			try
			{

				string directoryPath = "ConnectionData";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);



				// Dosya varsa oku
				string jsonData = "";
				if (File.Exists(filePath))
				{
					jsonData = File.ReadAllText(filePath);
				}

				// JSON verisini kontrol et ve gerekirse düzenle
				JObject jsonObject;
				jsonObject = JObject.Parse(jsonData);

				jsonObject["ConnectionType"] = false;
				File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonObject, Formatting.Indented));
				notificatonSettings("Uygulama Durduruldu");
				//btnStartAutoPost.Enabled = true;
				//çalýþtýrToolStripMenuItem.Enabled = true;
				//button1btnStopAutoPost.Enabled = false;
				_isRunning = false;

				if (cancellationTokenSource != null)
				{
					cancellationTokenSource.Cancel();
				}
				çalýþtýrToolStripMenuItem.Enabled = true;
				btnStartAutoPost.Enabled = true;
			}
			catch (Exception)
			{
				MessageBox.Show("Henüz programý baþlatmadan direkt durduramazsýn !!!!!!");
			}
		}

		private async void btnStartAutoPost_Click(object sender, EventArgs e)
		{
			try
			{

				DateTime anlikTarih = DateTime.Today;
				var anlikTarih2 = "2023-03-30";


				string directoryPath = "ConnectionData";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);



				// Dosya varsa oku
				string jsonData = "";
				if (File.Exists(filePath))
				{
					jsonData = File.ReadAllText(filePath);
				}

				// JSON verisini kontrol et ve gerekirse düzenle
				JObject jsonObject;
				jsonObject = JObject.Parse(jsonData);

				jsonObject["ConnectionType"] = true;
				File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonObject, Formatting.Indented));

				//cancellationTokenSource = new CancellationTokenSource();
				//Task.Run(() => AutoPost(cancellationTokenSource.Token));
				cancellationTokenSource = new CancellationTokenSource();
				//cancellationTokenSource.CancelAfter(TimeSpan.Zero);

				await Task.Run(() => AutoPost(cancellationTokenSource.Token, anlikTarih));
				//await Task.Run(() => AutoPost_INWORK(cancellationTokenSource.Token, anlikTarih));
				//await Task.Run(() => AutoPost_CANCELED(cancellationTokenSource.Token, anlikTarih));


				_isRunning = true;
				button1btnStopAutoPost.Enabled = true;
				notificatonSettings("Uygulama Baþlatýldý");

				çalýþtýrToolStripMenuItem.Enabled = false;
				btnStartAutoPost.Enabled = false;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Baþlatma sýrasýnda bir hata oluþtur Hata!" + ex.Message);
				çalýþtýrToolStripMenuItem.Enabled = true;
				btnStartAutoPost.Enabled = true;
			}
		}
		public class Config
		{
			public List<Field> WTPartMaster { get; set; }
		}

		public class Field
		{
			public string ID { get; set; }
			public string Name { get; set; }
			public string SQLName { get; set; }
			public bool IsActive { get; set; }
		}

		#region Tasks Auto Post

		private async void AutoPost(CancellationToken stoppingToken, DateTime anlikTarih)
		{
			try
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					ShowData();
					ApiService _apiService = new ApiService();


					//btnStartAutoPost.Enabled = false;


					string directoryPath = "ConnectionData";
					string fileName = "appsettings.json";
					string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

					string directoryPath2 = "ApiSendDataSettingsFolder";
					string fileName2 = "ApiSendDataSettings.json";
					string filePath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath2, fileName2);

					if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
					{
						Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
					}

					if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath2)))
					{
						Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath2));
					}


					// (Önceki kodlar burada)

					string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
					string apiSendJsonData = File.Exists(filePath2) ? File.ReadAllText(filePath2) : string.Empty;

					JObject jsonObject = JObject.Parse(jsonData);
					var catalogValue = jsonObject["Catalog"].ToString();
					var connectionString = jsonObject["ConnectionStrings"]["Plm"].ToString();
					var conn = new SqlConnection(connectionString);
					var apiURL = jsonObject["APIConnectionINFO"]["ApiURL"].ToString();
					var apiFullUrl = jsonObject["APIConnectionINFO"]["API"].ToString();
					var CSRF_NONCE = jsonObject["APIConnectionINFO"]["CSRF_NONCE"].ToString();
					var BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
					var BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();

					var endPoint = "";

					JObject apiSendJsonDataArray = JObject.Parse(apiSendJsonData);

					foreach (var item1 in apiSendJsonDataArray.Properties())
					{
						foreach (var item in item1.Value)
						{


						if (item["state"]?.ToString() == "RELEASED")
						{
							endPoint = "Designtech/RELEASED";
						} else if(item["state"]?.ToString() == "INWORK")
						{
							endPoint = "Designtech/INWORK";
						}
						else if (item["state"]?.ToString() == "CANCELLED")
						{
							endPoint = "Designtech/CANCELED";
						}

						var state = item["state"].ToString();
						var sablonDataDurumu = item["sablonDataDurumu"].ToString();
						var sourceApi = item["source_Api"].ToString();
							
						if (sablonDataDurumu == "true")
						{
							await ProcessStateAsync(state, catalogValue, conn, apiURL, CSRF_NONCE, BasicUsername, BasicPassword, anlikTarih, sourceApi, endPoint);
						}
						}

					}

					await Task.Delay(5000, stoppingToken);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			finally
			{
			}
		}

		private async Task ProcessStateAsync(string state, string catalogValue, SqlConnection conn, string apiURL, string CSRF_NONCE, string BasicUsername, string BasicPassword, DateTime anlikTarih, string sourceApi,string endPoint)
		{
			var formattedTarih = DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
			var formattedTarih2 = DateTime.Today.ToString("yyyy.MM.dd HH:mm:ss.fffffff");

			var sql = "";
			if (sourceApi.Contains("ProdMgmt"))
			{

			 sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = '{state}' and [latestiterationInfo] = 1 and (updateStampA2 >= @formattedTarih or updateStampA2 >= @formattedTarih2)";
			}

			if (sourceApi.Contains("CADDocumentMgmt"))
			{

				 sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.EPMDocument WHERE [statestate] = '{state}' and [latestiterationInfo] = 1 and (updateStampA2 >= @formattedTarih or updateStampA2 >= @formattedTarih2)";
			}

			var resolvedItems = await conn.QueryAsync<dynamic>(sql, new { formattedTarih, formattedTarih2 });

			try
			{
				foreach (var partItem in resolvedItems)
				{
					WindchillApiService windchillApiService = new WindchillApiService();
					var json = await windchillApiService.GetApiData("192.168.1.11", $"{sourceApi+ partItem.idA2A2}')", BasicUsername, BasicPassword, CSRF_NONCE);

					try
					{
						var response = JsonConvert.DeserializeObject<Part>(json);
						var turkishDateFormat2 = response.LastModified.ToString();
						var iso8601Date2 = ConvertToIso8601Format(turkishDateFormat2);

						response.LastModified = Convert.ToDateTime(iso8601Date2);

						var existingLog = await conn.QuerySingleOrDefaultAsync<WTChangeOrder2MasterViewModel>(
							$"SELECT [idA2A2],[statestate], [ProcessTimestamp], [updateStampA2] FROM [{catalogValue}].[Change_Notice_LogTable] WHERE [idA2A2] = @idA2A2",
							new { idA2A2 = response.ID.Split(':')[2] });

						if (existingLog == null)
						{
							await InsertLogAndPostDataAsync(response, catalogValue, conn, apiURL, endPoint);
						}
						//else if (existingLog.updateStampA2 != partItem.updateStampA2)
						else if ((existingLog.statestate != response.State.Value) || (existingLog.updateStampA2 != response.LastModified))
						{
							await UpdateLogAndPostDataAsync(response, catalogValue, conn, apiURL, endPoint);
						}
						// If LastUpdateTimestamp has not changed, do nothing
					}
					catch (Exception ex)
					{
						// Handle the exception
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}






		//private async void AutoPost(CancellationToken stoppingToken, DateTime anlikTarih)
		//{
		//	try
		//	{
		//		while (!stoppingToken.IsCancellationRequested)
		//		{
		//			ShowData();
		//			ApiService _apiService = new ApiService();
		//			//btnStartAutoPost.Enabled = false;


		//			string directoryPath = "ConnectionData";
		//			string fileName = "appsettings.json";
		//			string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

		//			string directoryPath2 = "ApiSendDataSettingsFolder";
		//			string fileName2 = "ApiSendDataSettings.json";
		//			string filePath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath2, fileName2);

		//			if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
		//			{
		//				Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
		//			}

		//			if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath2)))
		//			{
		//				Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath2));
		//			}

		//			string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
		//			string apiSendJsonData = File.Exists(filePath2) ? File.ReadAllText(filePath2) : string.Empty;

		//			JObject jsonObject = JObject.Parse(jsonData);
		//			var catalogValue = jsonObject["Catalog"].ToString();
		//			var connectionString = jsonObject["ConnectionStrings"]["Plm"].ToString();
		//			var conn = new SqlConnection(connectionString);
		//			var apiURL = jsonObject["APIConnectionINFO"]["ApiURL"].ToString();
		//			var apiEndpoint = jsonObject["APIConnectionINFO"]["ApiEndpoint"].ToString();
		//			var apiFullUrl = jsonObject["APIConnectionINFO"]["API"].ToString();

		//			var CSRF_NONCE = jsonObject["APIConnectionINFO"]["CSRF_NONCE"].ToString();
		//			var BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
		//			var BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();

		//			//JObject apiSendJsonDataObject = JObject.Parse(apiSendJsonData);
		//			JArray apiSendJsonDataObject = JArray.Parse(apiSendJsonData);
		//			//var apiSendJsonDataList = apiSendJsonDataObject["WTPartMaster"];
		//			var apiSendJsonDataList2 = apiSendJsonDataObject["sablonDataDurumu"];

		//			//var jsonConfig = apiSendJsonDataObject["WTPartMaster"].ToString();
		//			//var config = JsonConvert.DeserializeObject<List<Field>>(jsonConfig);

		//			//string formattedTarih = anlikTarih.ToString("yyyy-MM-dd ");
		//			string formattedTarih = DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
		//			string formattedTarih2 = DateTime.Today.ToString("yyyy.MM.dd HH:mm:ss.fffffff");





		//			//CDC.fn_cdc_get_net_changes_WTPart
		//			//and[updateStampA2] = { formattedTarih}
		//			var sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = 'RELEASED' and [latestiterationInfo] = 1 and (updateStampA2 >= @formattedTarih or updateStampA2 >= @formattedTarih2)";

		//			var resolvedItems = await conn.QueryAsync<dynamic>(sql, new { formattedTarih, formattedTarih2 });
		//			try
		//			{


		//				foreach (var item in resolvedItems)
		//			{






		//				//               var existingColumns = conn.Query<string>(
		//				//	"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'WTPartMaster' AND COLUMN_NAME IN @ColumnNames",
		//				//	new { ColumnNames = config.Where(field => field.IsActive && !field.Name.Contains("idA2A2") && !field.Name.Contains("idA3masterReference")).Select(field => field.Name) }
		//				//);
		//				//var nonExistingColumns = config
		//				//	.Where(field => field.IsActive)
		//				//	.Select(x => x.Name)
		//				//	.ToList();
		//				WindchillApiService windchillApiService = new WindchillApiService();
		//				var json = await windchillApiService.GetApiData("192.168.1.11", $"ProdMgmt/Parts('OR:wt.part.WTPart:{item.idA2A2}')",BasicUsername,BasicPassword,CSRF_NONCE);

		//				//var jsonObject2 = JObject.Parse(json);

		//				//var jsonObject3 = new JObject();

		//				//// Yeni bir filteredJsonObject oluþtur
		//				//var filteredJsonObject = new JObject();

		//					//try
		//					//{
		//					//	foreach (var columnName in nonExistingColumns)
		//					//	{
		//					//		if (jsonObject2.ContainsKey(columnName))
		//					//		{
		//					//			filteredJsonObject.Add(columnName, jsonObject2[columnName]);
		//					//		}
		//					//	}
		//					//}
		//					//catch (Exception ex)
		//					//{

		//					//}

		//					try
		//					{
		//						//if (nonExistingColumns.Any())
		//						//{
		//						//}



		//						//var response = JsonConvert.DeserializeObject<Part>(filteredJsonObject.ToString());
		//						var response = JsonConvert.DeserializeObject<Part>(json);
		//							var turkishDateFormat2 = response.LastModified.ToString();
		//							var iso8601Date2 = ConvertToIso8601Format(turkishDateFormat2);

		//							response.LastModified = Convert.ToDateTime(iso8601Date2);

		//							var existingLog = await conn.QuerySingleOrDefaultAsync<WTChangeOrder2MasterViewModel>(
		//								$"SELECT [idA2A2], [ProcessTimestamp], [updateStampA2] FROM [{catalogValue}].[Change_Notice_LogTable] WHERE [idA2A2] = @idA2A2",
		//								new { idA2A2 = response.ID.Split(':')[2] });
		//						//,commandTimeout: Int32.MaxValue

		//							if (existingLog == null)
		//							{
		//								await InsertLogAndPostDataAsync(response, catalogValue, conn, apiURL, apiEndpoint);
		//							}
		//							else if (existingLog.updateStampA2 != response.LastModified)
		//							{
		//								await UpdateLogAndPostDataAsync(response, catalogValue, conn, apiURL, apiEndpoint);
		//							}
		//							// If LastUpdateTimestamp has not changed, do nothing

		//					}
		//					catch (Exception ex)
		//					{

		//					}



		//				}

		//			}
		//			catch (Exception ex)
		//			{
		//				MessageBox.Show(ex.Message);
		//			}

		//			await Task.Delay(5000, stoppingToken);
		//			//await Task.Delay(10000);
		//		}
		//	}
		//	catch (Exception ex)
		//	{
		//		MessageBox.Show(ex.Message);
		//		//btnStartAutoPost.Enabled = false;
		//		//MessageBox.Show("Hata!" + ex.Message);
		//		// Handle exceptions or log errors
		//	}
		//	finally
		//	{
		//		//btnStartAutoPost.Enabled = true;
		//	}
		//}


		private async void AutoPost_INWORK(CancellationToken stoppingToken, DateTime anlikTarih)
		{
			try
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					ShowData();
					ApiService _apiService = new ApiService();
					//btnStartAutoPost.Enabled = false;

					string directoryPath = "ConnectionData";
					string fileName = "appsettings.json";
					string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

					string directoryPath2 = "ApiSendDataSettingsFolder";
					string fileName2 = "ApiSendDataSettings.json";
					string filePath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath2, fileName2);

					if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
					{
						Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
					}

					if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath2)))
					{
						Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath2));
					}

					string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
					string apiSendJsonData = File.Exists(filePath2) ? File.ReadAllText(filePath2) : string.Empty;

					JObject jsonObject = JObject.Parse(jsonData);
					var catalogValue = jsonObject["Catalog"].ToString();
					var connectionString = jsonObject["ConnectionStrings"]["Plm"].ToString();
					var conn = new SqlConnection(connectionString);
					var apiURL = jsonObject["APIConnectionINFO"]["ApiURL"].ToString();
					//var apiEndpoint = jsonObject["APIConnectionINFO"]["ApiEndpoint"].ToString();
					var apiEndpoint = "Designtech/INWORK";
					var apiFullUrl = jsonObject["APIConnectionINFO"]["API"].ToString();

					JObject apiSendJsonDataObject = JObject.Parse(apiSendJsonData);
					var apiSendJsonDataList = apiSendJsonDataObject["WTPartMaster"];

					var CSRF_NONCE = jsonObject["APIConnectionINFO"]["CSRF_NONCE"].ToString();
					var BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
					var BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();

					//var jsonConfig = apiSendJsonDataObject["WTPartMaster"].ToString();
					//var config = JsonConvert.DeserializeObject<List<Field>>(jsonConfig);

					//string formattedTarih = anlikTarih.ToString("yyyy-MM-dd ");
					string formattedTarih = DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
					string formattedTarih2 = DateTime.Today.ToString("yyyy.MM.dd HH:mm:ss.fffffff");
					//CDC.fn_cdc_get_net_changes_WTPart
					//and[updateStampA2] = { formattedTarih}
					var sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = 'INWORK' and [latestiterationInfo] = 1 and (updateStampA2 >= @formattedTarih or updateStampA2 >= @formattedTarih2)";

					var resolvedItems = await conn.QueryAsync<dynamic>(sql, new { formattedTarih, formattedTarih2 });


					try
					{

				


			
					foreach (var item in resolvedItems)
					{






						//               var existingColumns = conn.Query<string>(
						//	"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'WTPartMaster' AND COLUMN_NAME IN @ColumnNames",
						//	new { ColumnNames = config.Where(field => field.IsActive && !field.Name.Contains("idA2A2") && !field.Name.Contains("idA3masterReference")).Select(field => field.Name) }
						//);
						//var nonExistingColumns = config
						//	.Where(field => field.IsActive)
						//	.Select(x => x.Name)
						//	.ToList();
						WindchillApiService windchillApiService = new WindchillApiService();
						var json = await windchillApiService.GetApiData("192.168.1.11", $"ProdMgmt/Parts('OR:wt.part.WTPart:{item.idA2A2}')",BasicUsername,BasicPassword,CSRF_NONCE);

						//var jsonObject2 = JObject.Parse(json);

						//var jsonObject3 = new JObject();

						//// Yeni bir filteredJsonObject oluþtur
						//var filteredJsonObject = new JObject();
						//	try
						//	{
						//		foreach (var columnName in nonExistingColumns)
						//		{
						//			if (jsonObject2.ContainsKey(columnName))
						//			{
						//				filteredJsonObject.Add(columnName, jsonObject2[columnName]);
						//			}
						//		}

						//	}
						//	catch (Exception ex)
						//	{

						//	}
							
							try
							{
								//if (nonExistingColumns.Any())
								//{


								//}

								//var response = JsonConvert.DeserializeObject<Part>(filteredJsonObject.ToString());
								var response = JsonConvert.DeserializeObject<Part>(json);
									var turkishDateFormat2 = response.LastModified.ToString();
									var iso8601Date2 = ConvertToIso8601Format(turkishDateFormat2);

									response.LastModified = Convert.ToDateTime(iso8601Date2);

									var existingLog = await conn.QuerySingleOrDefaultAsync<WTChangeOrder2MasterViewModel>(
										$"SELECT [idA2A2], [ProcessTimestamp], [updateStampA2] FROM [{catalogValue}].[Change_Notice_LogTable] WHERE [idA2A2] = @idA2A2",
										new { idA2A2 = response.ID.Split(':')[2] }
										);
								//,commandTimeout: Int32.MaxValue
									if (existingLog == null)
									{
										await InsertLogAndPostDataAsync(response, catalogValue, conn, apiURL, apiEndpoint);
									}
									else if (existingLog.updateStampA2 != response.LastModified)
									{
										await UpdateLogAndPostDataAsync(response, catalogValue, conn, apiURL, apiEndpoint);
									}
									// If LastUpdateTimestamp has not changed, do nothing

							}
							catch (Exception ex)
							{

							}
			

					
						}

					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
					}
					await Task.Delay(5000, stoppingToken);
					//await Task.Delay(10000);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
				//btnStartAutoPost.Enabled = false;
				//MessageBox.Show("Hata!" + ex.Message);
				// Handle exceptions or log errors
			}
			finally
			{
				//btnStartAutoPost.Enabled = true;
			}
		}



		private async void AutoPost_CANCELED(CancellationToken stoppingToken, DateTime anlikTarih)
		{
			try
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					ShowData();
					ApiService _apiService = new ApiService();
					//btnStartAutoPost.Enabled = false;

					string directoryPath = "ConnectionData";
					string fileName = "appsettings.json";
					string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

					string directoryPath2 = "ApiSendDataSettingsFolder";
					string fileName2 = "ApiSendDataSettings.json";
					string filePath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath2, fileName2);

					if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
					{
						Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
					}

					if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath2)))
					{
						Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath2));
					}

					string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
					string apiSendJsonData = File.Exists(filePath2) ? File.ReadAllText(filePath2) : string.Empty;

					JObject jsonObject = JObject.Parse(jsonData);
					var catalogValue = jsonObject["Catalog"].ToString();
					var connectionString = jsonObject["ConnectionStrings"]["Plm"].ToString();
					var conn = new SqlConnection(connectionString);
					var apiURL = jsonObject["APIConnectionINFO"]["ApiURL"].ToString();
					//var apiEndpoint = jsonObject["APIConnectionINFO"]["ApiEndpoint"].ToString();
					var apiEndpoint = "Designtech/CANCELED";
					var apiFullUrl = jsonObject["APIConnectionINFO"]["API"].ToString();


					var CSRF_NONCE = jsonObject["APIConnectionINFO"]["CSRF_NONCE"].ToString();
					var BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
					var BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();

					JObject apiSendJsonDataObject = JObject.Parse(apiSendJsonData);
					var apiSendJsonDataList = apiSendJsonDataObject["WTPartMaster"];

					//var jsonConfig = apiSendJsonDataObject["WTPartMaster"].ToString();
					//var config = JsonConvert.DeserializeObject<List<Field>>(jsonConfig);

					//string formattedTarih = anlikTarih.ToString("yyyy-MM-dd ");
					string formattedTarih = DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
					string formattedTarih2 = DateTime.Today.ToString("yyyy.MM.dd HH:mm:ss.fffffff");

					//CDC.fn_cdc_get_net_changes_WTPart
					//and[updateStampA2] = { formattedTarih}
					var sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = 'CANCELLED' and [latestiterationInfo] = 1 and (updateStampA2 >= @formattedTarih or updateStampA2 >= @formattedTarih2)";

					var resolvedItems = await conn.QueryAsync<dynamic>(sql, new { formattedTarih, formattedTarih2 });

					try
					{

					
			

					foreach (var item in resolvedItems)
					{






						//               var existingColumns = conn.Query<string>(
						//	"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'WTPartMaster' AND COLUMN_NAME IN @ColumnNames",
						//	new { ColumnNames = config.Where(field => field.IsActive && !field.Name.Contains("idA2A2") && !field.Name.Contains("idA3masterReference")).Select(field => field.Name) }
						//);
						//var nonExistingColumns = config
						//	.Where(field => field.IsActive)
						//	.Select(x => x.Name)
						//	.ToList();
						WindchillApiService windchillApiService = new WindchillApiService();
						var json = await windchillApiService.GetApiData("192.168.1.11", $"ProdMgmt/Parts('OR:wt.part.WTPart:{item.idA2A2}')",BasicUsername,BasicPassword,CSRF_NONCE);

						//var jsonObject2 = JObject.Parse(json);

						//var jsonObject3 = new JObject();

						// Yeni bir filteredJsonObject oluþtur
						//var filteredJsonObject = new JObject();
						//	try
						//	{
						//		foreach (var columnName in nonExistingColumns)
						//		{
						//			if (jsonObject2.ContainsKey(columnName))
						//			{
						//				filteredJsonObject.Add(columnName, jsonObject2[columnName]);
						//			}
						//		}
						//	}
						//	catch (Exception ex)
						//	{

						//	}
							try
							{
								//if (nonExistingColumns.Any())
								//{

								//}


								//var response = JsonConvert.DeserializeObject<Part>(filteredJsonObject.ToString());
								var response = JsonConvert.DeserializeObject<Part>(json);
								var turkishDateFormat2 = response.LastModified.ToString();
									var iso8601Date2 = ConvertToIso8601Format(turkishDateFormat2);

									response.LastModified = Convert.ToDateTime(iso8601Date2);

									var existingLog = await conn.QuerySingleOrDefaultAsync<WTChangeOrder2MasterViewModel>(
										$"SELECT [idA2A2], [ProcessTimestamp], [updateStampA2] FROM [{catalogValue}].[Change_Notice_LogTable] WHERE [idA2A2] = @idA2A2",
										new { idA2A2 = response.ID.Split(':')[2] });
								
								//,commandTimeout: Int32.MaxValue

									if (existingLog == null)
									{
										await InsertLogAndPostDataAsync(response, catalogValue, conn, apiURL, apiEndpoint);
									}
									else if (existingLog.updateStampA2 != response.LastModified)
									{
										await UpdateLogAndPostDataAsync(response, catalogValue, conn, apiURL, apiEndpoint);
									}
									// If LastUpdateTimestamp has not changed, do nothing

							}
							catch (Exception ex)
							{

							}
				


					
						}

					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
					}

					await Task.Delay(5000, stoppingToken);
					//await Task.Delay(10000);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);

				//btnStartAutoPost.Enabled = false;
				//MessageBox.Show("Hata!" + ex.Message);
				// Handle exceptions or log errors
			}
			finally
			{
				//btnStartAutoPost.Enabled = true;
			}
		}


		#endregion

		private async Task InsertLogAndPostDataAsync(Part response, string catalogValue, SqlConnection conn, string apiURL, string apiEndpoint)
		{
			try
			{
				ApiService _apiService = new ApiService();
				var jsonData3 = JsonConvert.SerializeObject(response);
				await _apiService.PostDataAsync(apiURL, apiEndpoint, jsonData3);
				await conn.ExecuteAsync(
					$"INSERT INTO [{catalogValue}].[Change_Notice_LogTable] ([idA2A2], [ProcessTimestamp], [updateStampA2],[statestate], [name], [WTPartNumber],[Version],[VersionID]) VALUES (@idA2A2, @ProcessTimestamp, @updateStampA2,@statestate, @name, @WTPartNumber,@Version,@VersionID )",
					new { idA2A2 = response.ID.Split(':')[2], ProcessTimestamp = DateTime.UtcNow, updateStampA2 = response.LastModified, statestate = response.State.Value, name = response.Name, WTPartNumber = response.Number,Version = response.Version, VersionID = response.VersionID });


				LogService logService = new LogService(_configuration);
				logService.CreateJsonFileLog(jsonData3);

			}
			catch (Exception)
			{

			}


		}

		private async Task UpdateLogAndPostDataAsync(Part response, string catalogValue, SqlConnection conn, string apiURL, string apiEndpoint)
		{
			try
			{

				ApiService _apiService = new ApiService();
				var jsonData3 = JsonConvert.SerializeObject(response);
				await _apiService.PostDataAsync(apiURL, apiEndpoint, jsonData3);
				await conn.ExecuteAsync(
					$"UPDATE [{catalogValue}].[Change_Notice_LogTable] SET [ProcessTimestamp] = @ProcessTimestamp, [updateStampA2] = @updateStampA2, [statestate] = @statestate,[name] = @name , [WTPartNumber] = @WTPartNumber, [Version] = @Version, [VersionID] = @VersionID WHERE [idA2A2] = @idA2A2",
					new { idA2A2 = response.ID.Split(':')[2], ProcessTimestamp = DateTime.UtcNow, updateStampA2 = response.LastModified, statestate = response.State.Value, name = response.Name, WTPartNumber = response.Number, Version = response.Version, VersionID = response.VersionID });


				LogService logService = new LogService(_configuration);
				logService.CreateJsonFileLog(jsonData3);

			}
			catch (Exception)
			{

			}

		}





		//  private async void AutoPost(CancellationToken stoppingToken)
		//  {
		//      try
		//      {
		//          while (!stoppingToken.IsCancellationRequested)
		//          {
		//              ShowData();
		//              ApiService _apiService = new ApiService();
		//          btnStartAutoPost.Enabled = false;

		//          string directoryPath = "ConnectionData";
		//          string fileName = "appsettings.json";
		//          string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);



		//              string directoryPath2 = "ApiSendDataSettingsFolder";
		//              string fileName2 = "ApiSendDataSettings.json";
		//              string filePath2 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath2, fileName2);

		//              // Klasör yoksa oluþtur
		//              if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
		//          {
		//              Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
		//          }

		//              // Klasör yoksa oluþtur
		//              if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath2)))
		//              {
		//                  Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath2));
		//              }

		//              // Dosya varsa oku
		//              string jsonData = "";
		//          if (File.Exists(filePath))
		//          {
		//              jsonData = File.ReadAllText(filePath);
		//          }


		//              string apiSendJsonData = "";
		//              if (File.Exists(filePath2))
		//              {
		//                  apiSendJsonData = File.ReadAllText(filePath2);
		//              }

		//              // JSON verisini kontrol et ve gerekirse düzenle
		//              JObject jsonObject;
		//              jsonObject = JObject.Parse(jsonData);
		//              var catalogValue = jsonObject["Catalog"].ToString();
		//              var connectionString = jsonObject["ConnectionStrings"]["Plm"].ToString();
		//              var conn = new SqlConnection(connectionString);
		//                                  var apiURL = jsonObject["APIConnectionINFO"]["ApiURL"].ToString();
		//              var apiEndpoint = jsonObject["APIConnectionINFO"]["ApiEndpoint"].ToString();
		//              var apiFullUrl = jsonObject["APIConnectionINFO"]["API"].ToString();

		//              JObject apiSendJsonDataObject;
		//              apiSendJsonDataObject = JObject.Parse(apiSendJsonData);
		//              var apiSendJsonDataList = apiSendJsonDataObject["WTPartMaster"];


		//              var jsonConfig = apiSendJsonDataObject["WTPartMaster"].ToString();
		//              var config = JsonConvert.DeserializeObject<List<Field>>(jsonConfig);


		//              //LogService logService = new LogService(_configuration);
		//              //ApiService apiService = new ApiService(_env);
		//              //while (!stoppingToken.IsCancellationRequested)



		//              //var sql = $"SELECT [CN_NUMBER], [CHANGE_NOTICE], [STATE], [LastUpdateTimestamp] FROM {catalogValue}.dbo.Change_Notice WHERE [STATE] = 'RESOLVED'";
		//              var sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = 'RELEASED' and [latestiterationInfo] = 1";

		//              var resolvedItems = await conn.QueryAsync<dynamic>(sql);

		//              foreach (var item in resolvedItems)
		//              {



		//                  var existingColumns = conn.Query<string>(
		//                      "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'WTPartMaster' AND COLUMN_NAME IN @ColumnNames",
		//                      new { ColumnNames = config.Where(field => field.IsActive && !field.Name.Contains("idA2A2") && !field.Name.Contains("idA3masterReference")).Select(field => field.Name) }
		//                  );
		//                  var nonExistingColumns = config
		//                      .Where(field => field.IsActive && existingColumns.Contains(field.Name))
		//                      .ToList();

		//                  if (nonExistingColumns.Any())
		//                  {
		//                      //var sql2 = $"SELECT ";
		//                      //var fieldsToSelect = existingColumns
		//                      //    .Select(field => $"[{field}]");

		//                      //sql2 += string.Join(", ", fieldsToSelect);
		//                      //sql2 += $" FROM {catalogValue}.WTPartMaster WHERE [idA2A2] = {item.idA3masterReference}";

		//                      ////var sql2 = $"SELECT [name], [WTPartNumber] FROM {catalogValue}.WTPartMaster WHERE [idA2A2] = {item.idA3masterReference}";

		//                      //var resolvedItems2 = await conn.QueryFirstAsync<dynamic>(sql2);

		//                      //resolvedItems2.idA2A2 = item.idA2A2;
		//                      //resolvedItems2.idA3masterReference = item.idA3masterReference;
		//                      //resolvedItems2.statestate = item.statestate;
		//                      //resolvedItems2.updateStampA2 = item.updateStampA2;




		//	WindchillApiService windchillApiService = new WindchillApiService();
		//	var json = await windchillApiService.GetApiData("192.168.1.11", $"ProdMgmt/Parts('OR:wt.part.WTPart:{item.idA2A2}')");
		//	var response = JsonConvert.DeserializeObject<Part>(json);
		//	var turkishDateFormat2 = response.LastModified.ToString();
		//	var iso8601Date2 = ConvertToIso8601Format(turkishDateFormat2);

		//	response.LastModified = Convert.ToDateTime(iso8601Date2);
		//	// Check if the CN_NUMBER already exists in Change_Notice_LogTable
		//	var existingLog = await conn.QuerySingleOrDefaultAsync<WTChangeOrder2MasterViewModel>(
		//                      $"SELECT [idA2A2], [ProcessTimestamp], [updateStampA2] FROM [{catalogValue}].[Change_Notice_LogTable] WHERE [idA2A2] = @idA2A2",
		//                      new { idA2A2 = response.ID.Split(':')[2] });

		//                  if (existingLog == null)
		//                  {
		//                      // If CN_NUMBER doesn't exist, insert a new log entry
		//                      await conn.ExecuteAsync(
		//                          $"INSERT INTO [{catalogValue}].[Change_Notice_LogTable] ([idA2A2], [ProcessTimestamp], [updateStampA2], [name] , [WTPartNumber]) VALUES (@idA2A2, @ProcessTimestamp, @updateStampA2, @name, @WTPartNumber )",
		//                          new { idA2A2 = response.ID.Split(':')[2], ProcessTimestamp = DateTime.UtcNow, updateStampA2 =response.LastModified , name = response.Name, WTPartNumber = response.Number});

		//		var jsonData3 = JsonConvert.SerializeObject(response);

		//		LogService logService = new LogService(_configuration);
		//		logService.CreateJsonFileLog(jsonData3);

		//		await _apiService.PostDataAsync(apiURL, apiEndpoint, jsonData3);
		//	}
		//                  else
		//                  {
		//                      // If CN_NUMBER exists, check if LastUpdateTimestamp has changed
		//                      if (existingLog.updateStampA2 != response.LastModified)
		//                      {
		//                          // If LastUpdateTimestamp has changed, update the log entry
		//                          await conn.ExecuteAsync(
		//                              $"UPDATE [{catalogValue}].[Change_Notice_LogTable] SET [ProcessTimestamp] = @ProcessTimestamp, [updateStampA2] = @updateStampA2, [name] = @name , [WTPartNumber] = @WTPartNumber WHERE [idA2A2] = @idA2A2",
		//                              new { idA2A2 = response.ID.Split(':')[2], ProcessTimestamp = DateTime.UtcNow, updateStampA2 = response.LastModified, name= response.Name, WTPartNumber = response.Number });

		//			var jsonData3 = JsonConvert.SerializeObject(response);


		//			LogService logService = new LogService(_configuration);
		//			logService.CreateJsonFileLog(jsonData3);
		//			await _apiService.PostDataAsync(apiURL, apiEndpoint, jsonData3);
		//                      }
		//                      else
		//                      {
		//                          continue;
		//                          // If LastUpdateTimestamp has not changed, do nothing
		//                          //logService.AddNewLogEntry($"{item.CN_NUMBER} 'ýn tarihi deðiþmedi, iþlem yapýlmadý", null, "Post Edilmedi", null);
		//                      }
		//                  }




		//}
		//              }

		//              await Task.Delay(10000, stoppingToken);
		//              continue;
		//              //await Task.Delay(10000);
		//          }

		//          btnStartAutoPost.Enabled = true;
		//      }
		//      catch (Exception ex)
		//      {
		//          btnStartAutoPost.Enabled = false;
		//          MessageBox.Show("Hata!" + ex.Message);
		//          //LogService logService = new LogService(_configuration);
		//          //logService.AddNewLogEntry("Auto Post Aktif edilemedi: " + ex.Message, null, "Auto Post Aktif Deðil", null);
		//      }
		//  }

		// Tarih alanýný ISO 8601 formatýna dönüþtüren fonksiyon

		static string ConvertToIso8601Format(string veritabaniTarihiStr)
		{
			try
			{
				// Türkçe tarih formatýndaki noktalarý tirelere çevir
				veritabaniTarihiStr = veritabaniTarihiStr.Replace('.', '-');

				// Ýki farklý format için uygun formattan baþla
				string[] formats = { "d-M-yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss" };

				DateTime dateTime = DateTime.ParseExact(veritabaniTarihiStr, formats, null, System.Globalization.DateTimeStyles.None).AddHours(3);
				return dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Hata: {ex.Message}");
				return veritabaniTarihiStr; // veya hata durumunu iþlemek için baþka bir þey
			}
		}

		static string ConvertToIso8601Format2(string veritabaniTarihiStr)
		{
			try
			{
				// Türkçe tarih formatýndaki noktalarý tirelere çevir
				veritabaniTarihiStr = veritabaniTarihiStr.Replace('.', '-');

				// Ýki farklý format için uygun formattan baþla
				string[] formats = { "d-M-yyyy HH:mm:ss", "dd-MM-yyyy HH:mm:ss", "dd.MM.yyyy HH:mm:ss" };

				DateTime dateTime = DateTime.ParseExact(veritabaniTarihiStr, formats, null, System.Globalization.DateTimeStyles.None);
				return dateTime.ToString("yyyy-MM-dd");
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Hata: {ex.Message}");
				return veritabaniTarihiStr; // veya hata durumunu iþlemek için baþka bir þey
			}
		}

		//static string ConvertToIso8601Format(string turkishDateFormat)
		//{
		//    DateTime dateTime = DateTime.ParseExact(turkishDateFormat, "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
		//    return dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
		//}

		private void CreateJsonFile(Part dataModel)
		{
			try
			{
				string directoryPath = "ListData";
				string fileName = "relasedData.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

				// Klasör yoksa oluþtur
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

				if (string.IsNullOrWhiteSpace(jsonData))
				{
					// JSON verisi boþsa yeni bir nesne oluþtur
					jsonObject = new JObject
					{
						["data"] = new JArray()
					};
				}
				else
				{
					// JSON verisi mevcutsa, onu bir nesneye çevir
					jsonObject = JObject.Parse(jsonData);
				}

				// Yeni veriyi JSON nesnesine ekle veya güncelle
				JArray dataArray = (JArray)jsonObject["data"];
				dataArray.Add(JObject.FromObject(dataModel));

				// JSON dosyasýna yaz
				File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonObject, Formatting.Indented));
			}
			catch (Exception ex)
			{
				MessageBox.Show("Hata! JSON dosyasý oluþturulamadý: " + ex.Message);
			}
		}


		private void DisplayJsonDataInListBox()
		{
			try
			{
				string directoryPath = "ListData";
				string fileName = "relasedData.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

				// JSON dosyasýndaki veriyi oku
				string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : "[]";

				// JSON verisini listeye dönüþtür
				JObject jsonObject = JObject.Parse(jsonData);

				// "data" alanýný bir JArray olarak al
				JArray dataArray = (JArray)jsonObject["data"];

				// ListBox'ý temizle
				listBox1.Items.Clear();

				foreach (JObject dataObject in dataArray)
				{
					dynamic dataModel = new ExpandoObject();

					foreach (var property in dataObject.Properties())
					{
						((IDictionary<string, object>)dataModel).Add(property.Name, property.Value?.ToString());
					}

					// Formatlanmýþ string oluþtur
					string displayString = $"[{listBox1.Items.Count + 1}]   ";

					foreach (var property in ((IDictionary<string, object>)dataModel))
					{
						displayString += $"{property.Key} : {property.Value}, ";
					}

					// Son virgülü kaldýr
					displayString = displayString.TrimEnd(',', ' ');

					// Add the formatted string to the ListBox for display
					listBox1.Items.Add(displayString);
					lblDataCount.Text = listBox1.Items.Count.ToString();
				}



			}
			catch (Exception ex)
			{
				//MessageBox.Show("Hata! JSON verisi ListBox'ta gösterilemedi: " + ex.Message);
			}
		}


		private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			lblDataCount.Text = listBox1.Items.Count.ToString();
		}
		// Her yeni item eklendiðinde



		private void NotifyIcon_DoubleClick(object sender, EventArgs e)
		{
			this.Show(); // Formu tekrar göster
			this.WindowState = FormWindowState.Normal;  // Eðer form minimize durumdaysa normal boyuta getir
			notifyIcon1.Visible = true;
		}

		private void ExitMenuItem_Click(object sender, EventArgs e)
		{
			this.Close(); // Çýkýþ menüsüne týklandýðýnda uygulamayý kapat
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
			{

				e.Cancel = true;
				Hide();
				notificatonSettings("Program kapatýlýyor.");
			}
		}


		//private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		//{
		//    if (e.CloseReason != CloseReason.ApplicationExitCall)
		//    {
		//        e.Cancel = true;
		//        Hide();
		//    }
		//    //if (e.CloseReason == CloseReason.UserClosing && this.WindowState == FormWindowState.Minimized)
		//    //{
		//    //    e.Cancel = true; // Kapatma iþlemini iptal et
		//    //    this.Hide();      // Formu gizle
		//    //}
		//}



		private void notificatonSettings(string text)
		{
			notifyIcon1.BalloonTipText = text;
			notifyIcon1.ShowBalloonTip(1000);
			this.ShowInTaskbar = false;
			notifyIcon1.Visible = true;
		}

		private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			Show();
			WindowState = FormWindowState.Normal;
		}

		private async void çalýþtýrToolStripMenuItem_Click(object sender, EventArgs e)
		{

			try
			{

			

				DateTime anlikTarih = DateTime.Today;
				var anlikTarih2 = "2023-03-30";


				string directoryPath = "ConnectionData";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);



				// Dosya varsa oku
				string jsonData = "";
				if (File.Exists(filePath))
				{
					jsonData = File.ReadAllText(filePath);
				}

				// JSON verisini kontrol et ve gerekirse düzenle
				JObject jsonObject;
				jsonObject = JObject.Parse(jsonData);

				jsonObject["ConnectionType"] = true;
				File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonObject, Formatting.Indented));

				//cancellationTokenSource = new CancellationTokenSource();
				//Task.Run(() => AutoPost(cancellationTokenSource.Token));
				cancellationTokenSource = new CancellationTokenSource();
				//cancellationTokenSource.CancelAfter(TimeSpan.Zero);

				await Task.Run(() => AutoPost(cancellationTokenSource.Token, anlikTarih));
				await Task.Run(() => AutoPost_INWORK(cancellationTokenSource.Token, anlikTarih));
				await Task.Run(() => AutoPost_CANCELED(cancellationTokenSource.Token, anlikTarih));


				_isRunning = true;
				button1btnStopAutoPost.Enabled = true;
				notificatonSettings("Uygulama Baþlatýldý");
				çalýþtýrToolStripMenuItem.Enabled = false;
				btnStartAutoPost.Enabled = false;
			}
			catch (Exception ex)
			{
				
				MessageBox.Show("Baþlatma sýrasýnda bir hata oluþtur Hata!" + ex.Message);
				çalýþtýrToolStripMenuItem.Enabled = true;
				btnStartAutoPost.Enabled = true;
			}
		}

		private void durdurToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{

				string directoryPath = "ConnectionData";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);



				// Dosya varsa oku
				string jsonData = "";
				if (File.Exists(filePath))
				{
					jsonData = File.ReadAllText(filePath);
				}

				// JSON verisini kontrol et ve gerekirse düzenle
				JObject jsonObject;
				jsonObject = JObject.Parse(jsonData);

				jsonObject["ConnectionType"] = false;
				File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonObject, Formatting.Indented));
				notificatonSettings("Uygulama Durduruldu");
				//btnStartAutoPost.Enabled = true;
				//çalýþtýrToolStripMenuItem.Enabled = true;
				//button1btnStopAutoPost.Enabled = false;
				_isRunning = false;

				if (cancellationTokenSource != null)
				{
					cancellationTokenSource.Cancel();
				}

				çalýþtýrToolStripMenuItem.Enabled = true;
				btnStartAutoPost.Enabled = true;
			}
			catch (Exception)
			{
				MessageBox.Show("Henüz programý baþlatmadan direkt durduramazsýn !!!!!!");

			}
		}

		private void kapatToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Uygulamayý sonlandýrmak istiyormusun ?", "Exit", MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				string directoryPath = "ConnectionData";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);



				// Dosya varsa oku
				string jsonData = "";
				if (File.Exists(filePath))
				{
					jsonData = File.ReadAllText(filePath);
				}

				// JSON verisini kontrol et ve gerekirse düzenle
				JObject jsonObject;
				jsonObject = JObject.Parse(jsonData);

				jsonObject["ConnectionType"] = false;
				File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonObject, Formatting.Indented));
				notificatonSettings("Uygulama kapatýldý");
				Application.Exit();

				çalýþtýrToolStripMenuItem.Enabled = true;
				btnStartAutoPost.Enabled = true;

			}
		}


		private void btnKapat_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Uygulamayý sonlandýrmak istiyormusun ?", "Exit", MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				string directoryPath = "ConnectionData";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);



				// Dosya varsa oku
				string jsonData = "";
				if (File.Exists(filePath))
				{
					jsonData = File.ReadAllText(filePath);
				}

				// JSON verisini kontrol et ve gerekirse düzenle
				JObject jsonObject;
				jsonObject = JObject.Parse(jsonData);

				jsonObject["ConnectionType"] = false;
				File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonObject, Formatting.Indented));
				notificatonSettings("Uygulama kapatýldý");
				Application.Exit();

				çalýþtýrToolStripMenuItem.Enabled = true;
				btnStartAutoPost.Enabled = true;

			}
		}

		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
		}

		private void Form1_FormClosing_1(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason != CloseReason.ApplicationExitCall)
			{
				e.Cancel = true;
				Hide();
			}
		}





		private void Form1_Resize(object sender, EventArgs e)
		{
			//if (WindowState == FormWindowState.Minimized)
			//{
			//	Hide();
			//	notificatonSettings("Uygulama küçültüldü");
			//}


			if (this.WindowState == FormWindowState.Minimized)
			{
				this.Hide();
				notifyIcon1.Visible = true;
				notificatonSettings("Uygulama küçültüldü");
			}
			else if (this.WindowState == FormWindowState.Normal)
			{
				notifyIcon1.Visible = false;
			}
		}

		private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
		{
			contextMenuStrip1.Enabled = WindowState == FormWindowState.Minimized;
		}


		//LOG AYARLARI

		private void UpdateLogList()
		{
			string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "TakvimFile");

			if (!Directory.Exists(logFolder))
			{
				MessageBox.Show("Log klasörü bulunamadý.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			var logFiles = Directory.GetFiles(logFolder, "*.json")
				.OrderByDescending(file => new FileInfo(file).LastWriteTime)
				.Select(Path.GetFileName) // Sadece dosya adýný al
				.ToList();

			listBox2.Items.Clear();
			listBox2.Items.AddRange(logFiles.ToArray());
		}



		private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				string selectedLogFile = listBox2.SelectedItem as string;

				if (selectedLogFile != null)
				{
					string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "TakvimFile", selectedLogFile);

					if (File.Exists(filePath))
					{
						string jsonContent = File.ReadAllText(filePath);

						// JSON içeriðini oku
						JObject jsonObject = JObject.Parse(jsonContent);

						// "data" alanýný bir JArray olarak al
						JArray dataArray = (JArray)jsonObject["data"];

						// ListBox'ý temizle
						listBox1.Items.Clear();

						// JSON verisini listeleyerek ListBox'a ekle
						for (int i = 0; i < dataArray.Count; i++)
						{
							JObject dataObject = (JObject)dataArray[i];

							dynamic dataModel = new ExpandoObject();

							foreach (var property in dataObject.Properties())
							{
								((IDictionary<string, object>)dataModel).Add(property.Name, property.Value?.ToString());
							}

							// Formatlanmýþ string oluþtur
							string displayString = $"[{i + 1}]   ";

							foreach (var property in ((IDictionary<string, object>)dataModel))
							{
								displayString += $"{property.Key} : {property.Value}, ";
							}

							// Son virgülü kaldýr
							displayString = displayString.TrimEnd(',', ' ');

							// ListBox'a ekle
							listBox1.Items.Add(displayString);
							lblDataCount.Text = listBox1.Items.Count.ToString();
						}
					}
					else
					{
						MessageBox.Show("Dosya bulunamadý: " + filePath, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Hata! JSON verisi ListBox'ta gösterilemedi: " + ex.Message);
			}
		}



		//private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
		//{
		//    string selectedLogFile = listBox2.SelectedItem as string;

		//    if (selectedLogFile != null)
		//    {
		//        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "TakvimFile", selectedLogFile);

		//        if (File.Exists(filePath))
		//        {
		//            string jsonContent = File.ReadAllText(filePath);
		//            listBox1.Items.Add(jsonContent);
		//        }
		//        else
		//        {
		//            MessageBox.Show("Dosya bulunamadý: " + filePath, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
		//        }
		//    }

		//}

		private void btnConnectionReflesh_Click(object sender, EventArgs e)
		{
			try
			{

				ShowData();
			}
			catch (Exception ex)
			{
				MessageBox.Show("HATA! " + ex.Message);
			}
		}

		private void txtShowServerName_TextChanged(object sender, EventArgs e)
		{

		}

		//LOG AYARLARI




		//AUTO POSTUN API ÝLE YAPILAN ÞEKLÝ

		private async void AutoPost2(CancellationToken stoppingToken)
		{
			try
			{
				WindchillApiService windchillApiService = new WindchillApiService();


				while (!stoppingToken.IsCancellationRequested)
				{

					var json = await windchillApiService.GetApiData("192.168.1.11", "ProdMgmt/Parts('')","","","");
					var response = JsonConvert.DeserializeObject<ProdMgmtParts>(json);

					Parallel.ForEach(response.Value, resp =>
					{
						//if(resp.State.Value == "RELEASED"  )
						//{

						//}
					});



					await Task.Delay(10000, stoppingToken);
				}

				//btnStartAutoPost.Enabled = true;
			}
			catch (Exception ex)
			{
				//btnStartAutoPost.Enabled = false;
				MessageBox.Show("Hata!" + ex.Message);
			}
		}
	}
}
