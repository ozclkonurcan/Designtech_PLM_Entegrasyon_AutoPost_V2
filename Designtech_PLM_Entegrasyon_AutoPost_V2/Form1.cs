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
using System.Net.Http;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Model.WindchillApiModel.CADDocumentMgmt;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Net.Http.Headers;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Model.WindchillApiModel;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using SqlKata.Execution;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Model;
using Microsoft.Extensions.Primitives;
using Azure;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Text.RegularExpressions;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Drawing.Imaging;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using Path = System.IO.Path;
using iText.IO.Font.Constants;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using IronOcr;
using Tesseract;
using static Humanizer.In;
using PdfSharp.Pdf;
using PdfiumViewer; 
using PdfDocument = PdfiumViewer.PdfDocument;
using IronPdf.Pages;
using PdfSharp.Drawing;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Vision.V1;
using FluentFTP;
using System.Net.Mail;
using Renci.SshNet;
using static Google.Rpc.Context.AttributeContext.Types;
using PdfSharp.Pdf.Content.Objects;
using Google.Api;
using static Humanizer.On;
using Designtech_PLM_Entegrasyon_AutoPost_V2.ViewModel.WTDocAttachmentsModel;
using System.Net.Http.Json;
using State = Designtech_PLM_Entegrasyon_AutoPost.Model.WindchillApiModel.State;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.Equivalence;
using Google.Protobuf.WellKnownTypes;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.WTPart.Alternate;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.WTPart.State;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.Revise;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.EPMDocument.Attachment;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EmailSettings;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.SqlSettigns;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModuluError.WTPart.State;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModuluError.WTPart.Alternate;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModuluError.Equivalence;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModuluError.EPMDocument.Attachment;
using static Google.Cloud.Vision.V1.ProductSearchResults.Types;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2
{
	public partial class Form1 : Form
	{
		private readonly IConfiguration _configuration;
		private readonly IDbConnection conn;
		private System.Windows.Forms.Timer timer;
		private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(Timeout.InfiniteTimeSpan);
		private bool _isRunning;
		private readonly HttpClient _httpClient;
		public QueryFactory _plm;

		private readonly IEquivalenceService _equivalenceService;
		private readonly IAlternateService _alternateService;
		private readonly IStateService _stateService;
		private readonly IWTPartReviseService _partReviseService;
		private readonly IAttachmentsService _attachmentsService;
		private readonly IEmailService _emailService;
		private readonly IClosedEnterationAttachmentsSerivce _closedEnterationAttachmentsSerivce;
		private readonly ISqlTriggerAndTableManagerService _sqlTriggerAndTableManagerService;
		//Error
		private readonly IErrorStateService _errorStateService;
		private readonly IErrorAlternateService _errorAlternateService;
		private readonly IErrorEquivalenceService _errorEquivalenceService;
		private readonly IErrorAttachmentsService _errorAttachmentsService;
		private readonly IErrorClosedEnterationAttachmentsSerivce _errorClosedEnterationAttachmentsSerivce;
		//Error







		public Form1(ApiService apiService, IDbConnection db, IConfiguration configuration)
		{
			conn = db;
			_configuration = configuration;
			_httpClient = new HttpClient();
			_plm = new PlmDatabase(configuration).Connect();

		}

		public Form1(IEquivalenceService equivalenceService, IAlternateService alternateService, IStateService stateService, IWTPartReviseService partReviseService, IAttachmentsService attachmentsService, IEmailService emailService, ISqlTriggerAndTableManagerService sqlTriggerAndTableManagerService, IClosedEnterationAttachmentsSerivce closedEnterationAttachmentsSerivce, IErrorStateService errorStateService, IErrorAlternateService errorAlternateService, IErrorEquivalenceService errorEquivalenceService, IErrorAttachmentsService errorAttachmentsService, IErrorClosedEnterationAttachmentsSerivce errorClosedEnterationAttachmentsSerivce)
		{
			InitializeComponent();
			_equivalenceService = equivalenceService;
			_alternateService = alternateService;
			_stateService = stateService;
			_partReviseService = partReviseService;
			_attachmentsService = attachmentsService;
			_emailService = emailService;
			_sqlTriggerAndTableManagerService = sqlTriggerAndTableManagerService;
			//Error
			_errorStateService = errorStateService;
			_errorAlternateService = errorAlternateService;
			_errorEquivalenceService = errorEquivalenceService;
			_errorAttachmentsService = errorAttachmentsService;
			//Error
			ShowData();
			DisplayJsonDataInListBox();
			UpdateLogList();
			lblDataCount.Text = listBox1.Items.Count.ToString();
			// Kapatma ayarlarý
			FormClosing += Form1_FormClosing;
			Resize += Form1_Resize;
			_closedEnterationAttachmentsSerivce = closedEnterationAttachmentsSerivce;
			_errorClosedEnterationAttachmentsSerivce = errorClosedEnterationAttachmentsSerivce;
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
				txtParola.Text = "";
				txtKullaniciAdi.Enabled = false;
				txtKullaniciAdi.Text = "";
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
					string serverWithPort = "";
					if (string.IsNullOrEmpty(txtSqlServerPortNumber.Text))
					{
						serverWithPort = $"{txtServerName.Text}";
					}
					else
					{
						serverWithPort = $"{txtServerName.Text}:{txtSqlServerPortNumber.Text}";
					}
					connectionString = $"Persist Security Info=False;User ID={txtKullaniciAdi.Text};Password={txtParola.Text};Initial Catalog={txtDatabaseAdi.Text};Server={serverWithPort};TrustServerCertificate=True";
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


					string directoryPath = "Configuration";
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
					var catalogValue = jsonObject["DatabaseSchema"].ToString();
					// Tablo varsa kontrol et

					var tableExists = TableExists($"[{catalogValue}].Change_Notice_LogTable", connectionString);
					var tableExistsEnt_EPMDocState = TableExists($"[{catalogValue}].Ent_EPMDocState", connectionString);
					var tableExistsEnt_EPMDocState_ERROR = TableExists($"[{catalogValue}].Ent_EPMDocState_ERROR", connectionString);
					var tableExistsEnt_EPMDocStateCancelled = TableExists($"[{catalogValue}].Ent_EPMDocState_CANCELLED", connectionString);
					var tableExistsEnt_EPMDocStateCancelled_ERROR = TableExists($"[{catalogValue}].Ent_EPMDocState_CANCELLED_ERROR", connectionString);

					var Des_PartDocumentBagla = TableExists($"[{catalogValue}].Des_PartDocumentBagla", connectionString);
					var Des_PartDocumentBaglaLog = TableExists($"[{catalogValue}].Des_PartDokumanBaglaLog", connectionString);

					var Des_CadDocumentBagla = TableExists($"[{catalogValue}].Des_CadDocumentBagla", connectionString);
					var Des_CadDocumentBaglaLog = TableExists($"[{catalogValue}].Des_CadDocumentBaglaLog", connectionString);

					//Attachments
					var Des_EPMDocAttachmentsLog = TableExists($"[{catalogValue}].Des_EPMDocAttachmentsLog", connectionString);
					//KullanýcýAyarTable
					var Des_Kullanici = TableExists($"[{catalogValue}].Des_Kullanici", connectionString);
					var Des_KulYetki = TableExists($"[{catalogValue}].Des_KulYetki", connectionString);



					var EPMDokumanStateTrigger = TriggerExists($"[{catalogValue}].EPMDokumanState", connectionString);
					var EPMDokumanState_CANCELLEDTrigger = TriggerExists($"[{catalogValue}].EPMDokumanState_CANCELLED", connectionString);
					var Part_DocumentTrigger = TriggerExists($"[{catalogValue}].Part_Document", connectionString);
					var EMPReferenceLinkTrigger = TriggerExists($"[{catalogValue}].EMP_Document", connectionString);

					//var tableExistsLOG = TableExists($"[{catalogValue}].WTPartAlternateLink_LOG", connectionString);
					//var tableExistsControlLog = TableExists($"[{catalogValue}].WTPartAlternateLink_ControlLog", connectionString);

					if (!tableExists || !tableExistsEnt_EPMDocState || !tableExistsEnt_EPMDocStateCancelled || !tableExistsEnt_EPMDocState_ERROR || !tableExistsEnt_EPMDocStateCancelled_ERROR || !Des_PartDocumentBagla || !Des_PartDocumentBaglaLog || !EPMDokumanStateTrigger || !EPMDokumanState_CANCELLEDTrigger || !Part_DocumentTrigger || !Des_Kullanici || !Des_KulYetki || !Des_CadDocumentBagla || !Des_CadDocumentBaglaLog || !EMPReferenceLinkTrigger || !Des_EPMDocAttachmentsLog)
					{
						// Tablo yoksa oluþtur
						await _sqlTriggerAndTableManagerService.CreateTableAndTrigger(connectionString);
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

		private bool TriggerExists(string triggerName, string connectionString)
		{
			using (var connection = new SqlConnection(connectionString))
			using (var command = connection.CreateCommand())
			{
				command.CommandText = $"IF OBJECT_ID('{triggerName}', 'TR') IS NULL SELECT 0 ELSE SELECT 1";
				connection.Open();
				return (int)command.ExecuteScalar() == 1;
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

				string directoryPath = "Configuration";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

				// Klasör yoksa oluþtur
				if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
				{
					Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
				}

				// Dosya varsa oku
				string jsonData = "";
				if (System.IO.File.Exists(filePath))
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
						["DatabaseSchema"] = "",
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
							["API_ENDPOINT_RELEASED"] = "",
							["API_ENDPOINT_INWORK"] = "",
							["API_ENDPOINT_CANCELLED"] = "",
							["API_ENDPOINT_SEND_FILE"] = "",
							["API_ENDPOINT_ALTERNATE_PART"] = "",
							["API_ENDPOINT_REMOVED"] = "",
							["WindchillServerName"] = "",
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
				jsonObject["DatabaseSchema"] = txtSqlSchemaName.Text;
				jsonObject["ServerName"] = txtServerName.Text;
				jsonObject["APIConnectionINFO"]["WindchillServerName"] = txtWindchillApi.Text;
				jsonObject["KullaniciAdi"] = txtKullaniciAdi.Text;
				jsonObject["Parola"] = txtParola.Text;


				jsonObject["ConnectionStrings"]["Plm"] = connectionString;

				// JSON nesnesini dosyaya geri yaz
				File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonObject, Formatting.Indented));

			}
			catch (Exception ex)
			{
				notificatonSettings("HATA" + ex.Message);
				MessageBox.Show("HATA !", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		private async void ShowData()
		{
			try
			{

				string directoryPath = "Configuration";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);






				if (File.Exists(filePath))
				{
					string jsonData = File.ReadAllText(filePath);
					JObject jsonObject = JObject.Parse(jsonData);

					// Ýlgili verileri çek ve kullanýcýya göster
					txtShowServerName.Text = jsonObject["ServerName"].ToString();
					txtShowCatalog.Text = jsonObject["Catalog"].ToString();
					txtSqlSchemaName.Text = jsonObject["DatabaseSchema"].ToString();
					txtShowWindchillServerName.Text = jsonObject["APIConnectionINFO"]["WindchillServerName"].ToString();
					txtSqlSchemaName.Text = jsonObject["DatabaseSchema"].ToString();
					txtShowWindchillUserName.Text = jsonObject["APIConnectionINFO"]["Username"].ToString();
					txtShowSqlSchemaName.Text = jsonObject["DatabaseSchema"].ToString();

					txtWindchillApi.Text = jsonObject["APIConnectionINFO"]["WindchillServerName"].ToString();
					txtBasicUsername.Text = jsonObject["APIConnectionINFO"]["Username"].ToString();
					txtBasicPassword.Text = jsonObject["APIConnectionINFO"]["Password"].ToString();

					txtServerName.Text = jsonObject["ServerName"].ToString();
					txtDatabaseAdi.Text = jsonObject["Catalog"].ToString();
					txtKullaniciAdi.Text = jsonObject["KullaniciAdi"].ToString();
					txtParola.Text = jsonObject["Parola"].ToString();

					txtDesWTCode.Text = "DES-" + jsonObject["DesVeriTasimaID"].ToString();


					#region Equivalence Trigger Aktif/Pasif Control
					bool isTriggerEnabled = await IsEquivalenceTriggerEnabledAsync();

					if (isTriggerEnabled)
					{
						rdbEquivalenceAcik.Checked = true;
					}
					else
					{
						rdbEquivalenceKapali.Checked = true;
					}
					#endregion

				}
				else
				{
					// Dosya yoksa veya okunamazsa hata mesajý göster
					notificatonSettings("HATA appsettings.json dosyasý bulunamadý veya okunamadý.");
					MessageBox.Show("appsettings.json dosyasý bulunamadý veya okunamadý.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (Exception ex)
			{
				notificatonSettings("HATA" + ex.Message);
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

		private async void btnApiEkle_Click(object sender, EventArgs e)
		{
			try
			{

				WindchillApiService windchillApiService = new WindchillApiService();

				string directoryPath = "Configuration";
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
						["DatabaseSchema"] = "",
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
							["API_ENDPOINT_RELEASED"] = "",
							["API_ENDPOINT_INWORK"] = "",
							["API_ENDPOINT_CANCELLED"] = "",
							["API_ENDPOINT_ALTERNATE_PART"] = "",
							["API_ENDPOINT_SEND_FILE"] = "",
							["API_ENDPOINT_REMOVED"] = "",
							["WindchillServerName"] = "",
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

				jsonObject["APIConnectionINFO"]["WindchillServerName"] = txtWindchillApi.Text;
				jsonObject["APIConnectionINFO"]["Username"] = txtBasicUsername.Text;
				jsonObject["APIConnectionINFO"]["Password"] = txtBasicPassword.Text;



				var WindchillServerName = jsonObject["APIConnectionINFO"]["WindchillServerName"].ToString();
				var BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
				var BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();


				if (string.IsNullOrEmpty(WindchillServerName))
				{
					WrsToken apiToken = await windchillApiService.GetApiToken(txtWindchillApi.Text, txtBasicUsername.Text, txtBasicPassword.Text);
					JToken csrfToken = JToken.FromObject(apiToken.NonceValue);

					// jsonObject içindeki ilgili yerin deðerini güncelle
					jsonObject["APIConnectionINFO"]["CSRF_NONCE"] = csrfToken;
				}
				else
				{
					WrsToken apiToken = await windchillApiService.GetApiToken(WindchillServerName, BasicUsername, BasicPassword);
					JToken csrfToken = JToken.FromObject(apiToken.NonceValue);

					// jsonObject içindeki ilgili yerin deðerini güncelle
					jsonObject["APIConnectionINFO"]["CSRF_NONCE"] = csrfToken;
				}


				// JSON nesnesini dosyaya geri yaz
				File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonObject, Formatting.Indented));




				MessageBox.Show("Windchill baðlantý ayarlarý kayýt edildi.");
				notificatonSettings("Windchill baðlantý ayarlarý kayýt edildi.");
				ShowData();
			}
			catch (Exception ex)
			{

				MessageBox.Show("HATA " + ex.Message, "HATA ! ", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}





		private void button1btnStopAutoPost_Click(object sender, EventArgs e)
		{
			try
			{

				string directoryPath = "Configuration";
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

				_isRunning = false;

				if (cancellationTokenSource != null)
				{
					cancellationTokenSource.Cancel();
				}
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


				string directoryPath = "Configuration";
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


				cancellationTokenSource = new CancellationTokenSource();
				var catalogValue = jsonObject["DatabaseSchema"].ToString();
				var connectionString = jsonObject["ConnectionStrings"]["Plm"].ToString();
				var conn = new SqlConnection(connectionString);

				//Task.WhenAll(_equivalenceService.getEquivalenceData(_configuration, conn, catalogValue));


				//await Task.Run(() => AutoPost(cancellationTokenSource.Token, anlikTarih));


				// Ýki görevi ayný anda baþlatýyoruz

				Task.Run(() => AutoPost(cancellationTokenSource.Token, anlikTarih));



				_isRunning = true;
				button1btnStopAutoPost.Enabled = true;
				notificatonSettings("Uygulama Baþlatýldý");

				btnStartAutoPost.Enabled = false;
			}
			catch (Exception ex)
			{
				notificatonSettings("Baþlatma sýrasýnda bir hata oluþtur Hata!" + ex.Message);
				MessageBox.Show("Baþlatma sýrasýnda bir hata oluþtur Hata!" + ex.Message);
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

		private async Task<int> AlternateLinkCountFunction()
		{
			try
			{
				string directoryPath = "Configuration";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);
				string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;

				JObject jsonObject = JObject.Parse(jsonData);
				var catalogValue = jsonObject["DatabaseSchema"].ToString();
				var connectionString = jsonObject["ConnectionStrings"]["Plm"].ToString();
				var conn = new SqlConnection(connectionString);

				var alternateLinkCount = (await conn.QueryAsync<dynamic>($"SELECT * FROM [{catalogValue}].[WTPartAlternateLink]")).ToList();
				int alternateLinkVeriSayisi = alternateLinkCount.Count();

				return alternateLinkVeriSayisi;
			}
			catch (Exception)
			{
				throw;
			}
		}



		private async void AutoPost(CancellationToken stoppingToken, DateTime anlikTarih)
		{
			try
			{
				int oldAlternateLinkCount = await AlternateLinkCountFunction();
				while (!stoppingToken.IsCancellationRequested)
				{
					ShowData();
					ApiService _apiService = new ApiService();



					//btnStartAutoPost.Enabled = false;


					string directoryPath = "Configuration";
					string fileName = "appsettings.json";
					string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

					string directoryPath2 = "Configuration";
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

					var catalogValue = jsonObject["DatabaseSchema"].ToString();
					var connectionString = jsonObject["ConnectionStrings"]["Plm"].ToString();
					var conn = new SqlConnection(connectionString);
					var apiURL = jsonObject["APIConnectionINFO"]["ApiURL"].ToString();
					//var apiFullUrl = jsonObject["APIConnectionINFO"]["API"].ToString();
					var CSRF_NONCE = jsonObject["APIConnectionINFO"]["CSRF_NONCE"].ToString();
					var API_ENDPOINT_RELEASED = jsonObject["APIConnectionINFO"]["API_ENDPOINT_RELEASED"].ToString();
					var API_ENDPOINT_INWORK = jsonObject["APIConnectionINFO"]["API_ENDPOINT_INWORK"].ToString();
					var API_ENDPOINT_CANCELLED = jsonObject["APIConnectionINFO"]["API_ENDPOINT_CANCELLED"].ToString();
					var API_ENDPOINT_ALTERNATE_PART = jsonObject["APIConnectionINFO"]["API_ENDPOINT_ALTERNATE_PART"].ToString();
					var API_ENDPOINT_SEND_FILE = jsonObject["APIConnectionINFO"]["API_ENDPOINT_SEND_FILE"].ToString();
					var API_ENDPOINT_REMOVED = jsonObject["APIConnectionINFO"]["API_ENDPOINT_REMOVED"].ToString();

					var ServerName = jsonObject["ServerName"].ToString();
					var WindchillServerName = jsonObject["APIConnectionINFO"]["WindchillServerName"].ToString();
					var BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
					var BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();




					var apiFullUrl = "";
					var apiAdres = "";
					var anaKaynak = "";
					var endPoint = "";




					JObject apiSendJsonDataArray = JObject.Parse(apiSendJsonData);

					foreach (var item1 in apiSendJsonDataArray.Properties())
					{
						foreach (var item in item1.Value)
						{

							var state = item["state"]?.ToString();
							var sablonDataDurumu = item["sablonDataDurumu"].ToString();
							var sourceApi = item["source_Api"].ToString();


							if (sourceApi.Contains("ProdMgmt") && sablonDataDurumu == "true")
							{
								if (state == "RELEASED" || state == "INWORK" || state == "CANCELLED" || state == "ALTERNATE_RELEASED" || state == "RELEASED_EquivalenceLink")
								{
									apiAdres = item["api_adres"].ToString();
									anaKaynak = item["ana_kaynak"].ToString();
									endPoint = item["alt_endpoint"].ToString();
									apiFullUrl = apiAdres + "/" + anaKaynak;
								}

							}

							else if (sourceApi.Contains("CADDocumentMgmt") && sablonDataDurumu == "true")
							{
								if (state == "CANCELLED" || state == "SEND_FILE")
								{
									apiAdres = item["api_adres"].ToString();
									anaKaynak = item["ana_kaynak"].ToString();
									endPoint = item["alt_endpoint"].ToString();
									apiFullUrl = apiAdres + "/" + anaKaynak;
								}
							}


							if (rdbEntegrasyonAcik.Checked)
							{
								if (sablonDataDurumu == "true" && state == "RELEASED" && sourceApi.Contains("ProdMgmt"))
								{
									await _partReviseService.ProcessReviseAsync(state, catalogValue, conn);
									await _stateService.getReleasedData(_configuration, conn, catalogValue, state, apiFullUrl, apiURL, sourceApi, endPoint, CSRF_NONCE, WindchillServerName, ServerName, BasicUsername, BasicPassword);
								}
								if (sablonDataDurumu == "true" && state == "CANCELLED" && sourceApi.Contains("ProdMgmt"))
								{
									await _partReviseService.ProcessReviseAsync(state, catalogValue, conn);
									await _stateService.getCancelledData(_configuration, conn, catalogValue, state, apiFullUrl, apiURL, sourceApi, endPoint, CSRF_NONCE, WindchillServerName, ServerName, BasicUsername, BasicPassword);
								}
								if (sablonDataDurumu == "true" && state == "INWORK" && sourceApi.Contains("ProdMgmt"))
								{
									await _partReviseService.ProcessReviseAsync(state, catalogValue, conn);
									await _stateService.getInworkData(_configuration, conn, catalogValue, state, apiFullUrl, apiURL, sourceApi, endPoint, CSRF_NONCE, WindchillServerName, ServerName, BasicUsername, BasicPassword);
								}

								if (sablonDataDurumu == "true" && state == "RELEASED_EquivalenceLink" && sourceApi.Contains("ProdMgmt") && rdbEquivalenceAcik.Checked)
								{
									await _partReviseService.ProcessReviseAsync(state, catalogValue, conn);
									await _equivalenceService.getEquivalenceData(_configuration, conn, catalogValue, apiFullUrl, apiURL, sourceApi, endPoint);
								}
								if (sablonDataDurumu == "true" && state == "ALTERNATE_RELEASED" && sourceApi.Contains("ProdMgmt"))
								{
									await _partReviseService.ProcessReviseAsync(state, catalogValue, conn);
									await _alternateService.getAlternateData(_configuration, conn, catalogValue, apiFullUrl, apiURL, sourceApi, endPoint);
									await _alternateService.getRemovedAlternateData(_configuration, conn, catalogValue, apiFullUrl, apiURL, sourceApi, endPoint);
								}
								if (sablonDataDurumu == "true" && state == "SEND_FILE" && sourceApi.Contains("CADDocumentMgmt"))
								{


									await _partReviseService.ProcessReviseAsync(state, catalogValue, conn);
									await _attachmentsService.GetAttachments(state, catalogValue, conn, apiFullUrl, apiURL, CSRF_NONCE, WindchillServerName, ServerName, BasicUsername, BasicPassword, sourceApi, endPoint, oldAlternateLinkCount, sablonDataDurumu);

								}

								if (sablonDataDurumu == "true" && state == "CANCELLED" && sourceApi.Contains("CADDocumentMgmt"))
								{


									await _partReviseService.ProcessReviseAsync(state, catalogValue, conn);
									await _attachmentsService.GetAttachments(state, catalogValue, conn, apiFullUrl, apiURL, CSRF_NONCE, WindchillServerName, ServerName, BasicUsername, BasicPassword, sourceApi, endPoint, oldAlternateLinkCount, sablonDataDurumu);

								}

								var now = DateTime.Now;
								if (now.Hour >= 22 && now.Hour < 23 && now.Minute <= 59)
								{
									if (sablonDataDurumu == "true" && state == "RELEASED" && sourceApi.Contains("ProdMgmt"))
									{
										await _partReviseService.ProcessReviseAsync(state, catalogValue, conn);
										await _errorStateService.getErrorReleasedData(_configuration, conn, catalogValue, state, apiFullUrl, apiURL, sourceApi, endPoint, CSRF_NONCE, WindchillServerName, ServerName, BasicUsername, BasicPassword);
									}
									if (sablonDataDurumu == "true" && state == "CANCELLED" && sourceApi.Contains("ProdMgmt"))
									{
										await _partReviseService.ProcessReviseAsync(state, catalogValue, conn);
										await _errorStateService.getErrorCancelledData(_configuration, conn, catalogValue, state, apiFullUrl, apiURL, sourceApi, endPoint, CSRF_NONCE, WindchillServerName, ServerName, BasicUsername, BasicPassword);
									}
									if (sablonDataDurumu == "true" && state == "INWORK" && sourceApi.Contains("ProdMgmt"))
									{
										await _partReviseService.ProcessReviseAsync(state, catalogValue, conn);
										await _errorStateService.getErrorInworkData(_configuration, conn, catalogValue, state, apiFullUrl, apiURL, sourceApi, endPoint, CSRF_NONCE, WindchillServerName, ServerName, BasicUsername, BasicPassword);
									}

									if (sablonDataDurumu == "true" && state == "RELEASED_EquivalenceLink" && sourceApi.Contains("ProdMgmt") && rdbEquivalenceAcik.Checked)
									{
										await _partReviseService.ProcessReviseAsync(state, catalogValue, conn);
										await _errorEquivalenceService.geErrorEquivalenceData(_configuration, conn, catalogValue, apiFullUrl, apiURL, sourceApi, endPoint);
									}
									if (sablonDataDurumu == "true" && state == "ALTERNATE_RELEASED" && sourceApi.Contains("ProdMgmt"))
									{
										await _partReviseService.ProcessReviseAsync(state, catalogValue, conn);
										await _errorAlternateService.getErrorAlternateData(_configuration, conn, catalogValue, apiFullUrl, apiURL, sourceApi, endPoint);
										await _errorAlternateService.getErrorRemovedAlternateData(_configuration, conn, catalogValue, apiFullUrl, apiURL, sourceApi, endPoint);
									}
									if (sablonDataDurumu == "true" && state == "SEND_FILE" && sourceApi.Contains("CADDocumentMgmt"))
									{
										await _partReviseService.ProcessReviseAsync(state, catalogValue, conn);
										await _errorClosedEnterationAttachmentsSerivce.GetErrorAttachments(state, catalogValue, conn, apiFullUrl, apiURL, CSRF_NONCE, WindchillServerName, ServerName, BasicUsername, BasicPassword, sourceApi, endPoint, oldAlternateLinkCount, sablonDataDurumu);
									}

									if (sablonDataDurumu == "true" && state == "CANCELLED" && sourceApi.Contains("CADDocumentMgmt"))
									{
										await _partReviseService.ProcessReviseAsync(state, catalogValue, conn);
										await _errorClosedEnterationAttachmentsSerivce.GetErrorAttachments(state, catalogValue, conn, apiFullUrl, apiURL, CSRF_NONCE, WindchillServerName, ServerName, BasicUsername, BasicPassword, sourceApi, endPoint, oldAlternateLinkCount, sablonDataDurumu);
									}


								}

							}
							if (rdbEntegrasyonKapali.Checked)
							{
								if (sablonDataDurumu == "true" && state == "SEND_FILE" && sourceApi.Contains("CADDocumentMgmt"))
								{
									await _closedEnterationAttachmentsSerivce.GetAttachments(state, catalogValue, conn, apiFullUrl, apiURL, CSRF_NONCE, WindchillServerName, ServerName, BasicUsername, BasicPassword, sourceApi, endPoint, oldAlternateLinkCount, sablonDataDurumu);
								}
								var now = DateTime.Now;
								if (now.Hour >= 22 && now.Hour < 23 && now.Minute <= 59)
								{
									if (sablonDataDurumu == "true" && state == "SEND_FILE" && sourceApi.Contains("CADDocumentMgmt"))
									{
										await _partReviseService.ProcessReviseAsync(state, catalogValue, conn);
										await _errorClosedEnterationAttachmentsSerivce.GetErrorAttachments(state, catalogValue, conn, apiFullUrl, apiURL, CSRF_NONCE, WindchillServerName, ServerName, BasicUsername, BasicPassword, sourceApi, endPoint, oldAlternateLinkCount, sablonDataDurumu);
									}
								}
							}


						}

					}

					await Task.Delay(1000, stoppingToken);
				}
			}
			catch (Exception ex)
			{
				notificatonSettings("Hata!" + ex.Message);
				MessageBox.Show(ex.Message);
			}
		}














		//Ýncelenecek


		#region FormVeDigerAyarlar


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
				listBox2.Refresh();
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
			catch (Exception)
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



		private void notificatonSettings(string text)
		{
			notifyIcon1.BalloonTipText = text;
			notifyIcon1.ShowBalloonTip(1000);
			this.ShowInTaskbar = true;
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
				//var anlikTarih2 = "2023-03-30";


				string directoryPath = "Configuration";
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

				cancellationTokenSource = new CancellationTokenSource();

				await Task.Run(() => AutoPost(cancellationTokenSource.Token, anlikTarih));


				_isRunning = true;
				button1btnStopAutoPost.Enabled = true;
				notificatonSettings("Uygulama Baþlatýldý");
				btnStartAutoPost.Enabled = false;
			}
			catch (Exception ex)
			{

				MessageBox.Show("Baþlatma sýrasýnda bir hata oluþtur Hata!" + ex.Message);
				btnStartAutoPost.Enabled = true;
			}
		}

		private void durdurToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{

				string directoryPath = "Configuration";
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

				_isRunning = false;

				if (cancellationTokenSource != null)
				{
					cancellationTokenSource.Cancel();
				}

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
				string directoryPath = "Configuration";
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


			}
		}


		private void btnKapat_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("Uygulamayý sonlandýrmak istiyormusun ?", "Exit", MessageBoxButtons.OKCancel) == DialogResult.OK)
			{
				string directoryPath = "Configuration";
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

				btnStartAutoPost.Enabled = true;

			}
		}

		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
		}

		private void Form1_FormClosing_1(object sender, FormClosingEventArgs e)
		{
			//Application.Exit();
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
			string logFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration\\logs", "TakvimFile");

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
					string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration\\logs", "TakvimFile", selectedLogFile);

					if (File.Exists(filePath))
					{
						string jsonContent = File.ReadAllText(filePath);

						// Parse the JSON content as an array directly
						JArray dataArray = JArray.Parse(jsonContent);

						// Clear the ListBox
						listBox1.Items.Clear();

						foreach (JObject dataObject in dataArray)
						{
							var displayStringSTATE = "";
							if (dataObject["State"]?["Display"] != null)
							{
								displayStringSTATE = $" ({dataObject["State"]["Display"]})";
								if (displayStringSTATE == " (Aktif)")
								{
									displayStringSTATE = " (Released)";
								}
								if (displayStringSTATE == " (Pasif)")
								{
									displayStringSTATE = " (Cancelled)";
								}
							}
							// Format the string with selected properties
							string displayString = $"{dataObject["Number"]} - {dataObject["Version"]} - {dataObject["Name"]}";


							// Check if message exists
							if (dataObject.ContainsKey("Mesaj") && !string.IsNullOrEmpty(dataObject["Mesaj"].ToString()) && dataObject["Mesaj"].ToString().Contains("kaldýrýldý"))
							{
								//displayString += $" - {dataObject["Mesaj"]}";
								if (dataObject.ContainsKey("Display") && !string.IsNullOrEmpty(dataObject["Display"]?.ToString()))
								{
									displayString = displayString.Replace(dataObject["State"]["Display"].ToString(), null);
								}
							}
							if (dataObject.ContainsKey("Mesaj") && !string.IsNullOrEmpty(dataObject["Mesaj"].ToString()) && dataObject["Mesaj"].ToString().Contains("Muadil parça"))
							{
								// Eðer "Number" anahtarý varsa ve deðeri boþ deðilse kontrol et
								if (dataObject.ContainsKey("Number") && !string.IsNullOrEmpty(dataObject["Number"]?.ToString()))
								{
									displayString = displayString.Replace(dataObject["Number"].ToString() + " - ", string.Empty);
								}

								// Eðer "Name" anahtarý varsa ve deðeri boþ deðilse kontrol et
								if (dataObject.ContainsKey("Name") && !string.IsNullOrEmpty(dataObject["Name"]?.ToString()))
								{
									displayString = displayString.Replace(dataObject["Name"].ToString(), string.Empty);
								}

								// Eðer "Version" anahtarý varsa ve deðeri boþ deðilse kontrol et
								if (dataObject.ContainsKey("Version") && !string.IsNullOrEmpty(dataObject["Version"]?.ToString()))
								{
									displayString = displayString.Replace(dataObject["Version"].ToString() + " - ", string.Empty);
								}
							}

							if (dataObject.ContainsKey("Mesaj") && !string.IsNullOrEmpty(dataObject["Mesaj"].ToString()))
							{
								displayString += $" {dataObject["Mesaj"]}";
							}

							displayString += $" - {dataObject["islemTarihi"]}";
							// Add to the ListBox
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

		private void groupBox3_Enter(object sender, EventArgs e)
		{

		}

		private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
		{

		}

		private void btnListbox2Reflesh_Click(object sender, EventArgs e)
		{
			try
			{
				UpdateLogList();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}


		public int IDquery()
		{
			var catalogValue = _configuration["Catalog"];
			var IdSeq = _plm.Query(catalogValue + ".id_sequence").OrderByDesc("value").FirstOrDefault();
			return Convert.ToInt32(IdSeq.value) + 100;
		}

		private void txtWindchillApi_TextChanged(object sender, EventArgs e)
		{

		}

		private void label5_Click(object sender, EventArgs e)
		{

		}

		private void label6_Click(object sender, EventArgs e)
		{

		}



		private async void btnDesVeriTasima_Click(object sender, EventArgs e)
		{
			try
			{

				string directoryPath = "Configuration";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

				// Klasör yoksa oluþtur
				if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
				{
					Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
				}

				// Dosya varsa oku
				string jsonData = "";
				if (System.IO.File.Exists(filePath))
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

					};
				}
				else
				{
					// JSON verisi mevcutsa, onu bir nesneye çevir
					jsonObject = JObject.Parse(jsonData);
				}

				// Yeni veriyi JSON nesnesine ekle veya güncelle
				jsonObject["DesVeriTasimaID"] = await returnVeriTasimaIdCode(txtDesVeriTasima.Text);

				// JSON nesnesini dosyaya geri yaz
				File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonObject, Formatting.Indented));
				MessageBox.Show("Aktarma baþarýlý");
			}
			catch (Exception ex)
			{
				notificatonSettings("HATA" + ex.Message);
				MessageBox.Show("HATA !", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}


		private async Task<string> returnVeriTasimaIdCode(string DesWTDocumentNumberName)
		{

			try
			{
				WindchillApiService _windchillApiService = new WindchillApiService();

				string directoryPath = "Configuration";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);



				if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
				{
					Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
				}



				// (Önceki kodlar burada)

				string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
				JObject jsonObject = JObject.Parse(jsonData);
				var catalogValue = jsonObject["DatabaseSchema"].ToString();
				var connectionString = jsonObject["ConnectionStrings"]["Plm"].ToString();
				var conn = new SqlConnection(connectionString);
				var CSRF_NONCE = jsonObject["APIConnectionINFO"]["CSRF_NONCE"].ToString();

				var ServerName = jsonObject["ServerName"].ToString();
				var WindchillServerName = jsonObject["APIConnectionINFO"]["WindchillServerName"].ToString();
				var BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
				var BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();


				//GetToken
				WrsToken apiToken = await _windchillApiService.GetApiToken(txtWindchillApi.Text, txtBasicUsername.Text, txtBasicPassword.Text);
				JToken csrfToken = JToken.FromObject(apiToken.NonceValue);
				var token = apiToken.NonceValue;


				var getVeriTasimaResponse = await _windchillApiService.GetApiVeriTasimaWTDoc(WindchillServerName, $"search/objects?%24filter=substringof(number%2C'{DesWTDocumentNumberName}')&typeId=WCTYPE%7Cwt.doc.WTDocument", BasicUsername, BasicPassword, token);
				var getVeriTasima = JsonConvert.DeserializeObject<WTDocumentRoot>(getVeriTasimaResponse);


				await _windchillApiService.WTDoc_ChekcOut(WindchillServerName, $"DocMgmt/Documents('OR:wt.doc.WTDocument:{getVeriTasima.Items.FirstOrDefault().Id.Split(':')[2]}')/PTC.DocMgmt.CheckOut", BasicUsername, BasicPassword, token);



				var SQL_WTDocument = $"SELECT [idA3masterReference] FROM {catalogValue}.WTDocument WHERE [idA2A2] = '{getVeriTasima.Items.FirstOrDefault().Id.Split(':')[2]}'";
				var resolvedItems_SQL_WTDocument = await conn.QuerySingleAsync<dynamic>(SQL_WTDocument);
				var SQL_WTDocumentWorkGroupID = $"SELECT TOP 1 [idA2A2] FROM {catalogValue}.WTDocument WHERE [statecheckoutInfo] = 'wrk' and [idA3masterReference] = '{resolvedItems_SQL_WTDocument.idA3masterReference}'";
				var resolvedItems_SQL_WTDocumentWorkGroupID = await conn.QuerySingleAsync<dynamic>(SQL_WTDocumentWorkGroupID);


				return resolvedItems_SQL_WTDocumentWorkGroupID.idA2A2.ToString();

			}
			catch (Exception)
			{
				return "";

			}


		}

		private void groupBox9_Enter(object sender, EventArgs e)
		{

		}
		#endregion

		private void txtDesWTCode_TextChanged(object sender, EventArgs e)
		{

		}

		private void listBox1_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Control && e.KeyCode == Keys.C && listBox1.SelectedItem != null)
			{
				Clipboard.SetText(listBox1.SelectedItem.ToString());
			}
		}

		private async void rdbEquivalenceAcik_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				string directoryPath = "Configuration";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);
				string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;

				JObject jsonObject = JObject.Parse(jsonData);
				var catalogValue = jsonObject["DatabaseSchema"].ToString();
				var connectionString = jsonObject["ConnectionStrings"]["Plm"].ToString();
				var conn = new SqlConnection(connectionString);

				string triggerName = $"{catalogValue}.Des_EquivalenceLink";
				string tableName = $"{catalogValue}.EquivalenceLink";



				string sql = $"ENABLE TRIGGER {triggerName} ON {tableName}";

				using (var connection = new SqlConnection(connectionString))
				{
					await connection.ExecuteAsync(sql);
				}

			}
			catch (Exception)
			{

			}


		}

		private async void rdbEquivalenceKapali_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				string directoryPath = "Configuration";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);
				string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;

				JObject jsonObject = JObject.Parse(jsonData);
				var catalogValue = jsonObject["DatabaseSchema"].ToString();
				var connectionString = jsonObject["ConnectionStrings"]["Plm"].ToString();
				var conn = new SqlConnection(connectionString);

				string triggerName = $"{catalogValue}.Des_EquivalenceLink";
				string tableName = $"{catalogValue}.EquivalenceLink";



				string sql = $"DISABLE TRIGGER {triggerName} ON {tableName}";

				using (var connection = new SqlConnection(connectionString))
				{
					await connection.ExecuteAsync(sql);
				}
			}
			catch (Exception)
			{

			}
		}


		private async Task<bool> IsEquivalenceTriggerEnabledAsync()
		{
			try
			{
				string directoryPath = "Configuration";
				string fileName = "appsettings.json";
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);
				string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;

				JObject jsonObject = JObject.Parse(jsonData);
				var catalogValue = jsonObject["DatabaseSchema"].ToString();
				var connectionString = jsonObject["ConnectionStrings"]["Plm"].ToString();

				string triggerName = $"{catalogValue}.Des_EquivalenceLink";

			


				using (var connection = new SqlConnection(connectionString))
				{

					await connection.OpenAsync();
					// Trigger durumunu kontrol et
					var isDisabled = await connection.ExecuteScalarAsync<int>($"SELECT is_disabled FROM sys.triggers WHERE name = '{triggerName.Split('.').Last()}'");
					// Parametreli sorgu
					bool isActive = isDisabled == 0;
					// Trigger durumu
					return isActive;
				}
			
			}
			catch (Exception)
			{
				// Hata durumunda varsayýlan olarak trigger'ýn kapalý olduðunu dönüyoruz
				return false;
			}
		}

	}
}
#endregion





