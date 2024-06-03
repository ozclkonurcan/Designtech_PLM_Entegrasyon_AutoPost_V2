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



        public Form1(ApiService apiService, IDbConnection db, IConfiguration configuration)
        {
            conn = db;
            _configuration = configuration;
            _httpClient = new HttpClient();
            _plm = new PlmDatabase(configuration).Connect();
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
                    //var tableExistsLOG = TableExists($"[{catalogValue}].WTPartAlternateLink_LOG", connectionString);
                    //var tableExistsControlLog = TableExists($"[{catalogValue}].WTPartAlternateLink_ControlLog", connectionString);

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

            #region AlterneLinkLog için kullanýlacak sql komutu

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
            #endregion



            using (var connection = new SqlConnection(connectionString))
            {

                connection.Open();
                using (var command1 = new SqlCommand(createTableSql, connection))
                {
                    command1.ExecuteNonQuery();
                }

                // Create the second table
                using (var command2 = new SqlCommand(createTableSql2, connection))
                {
                    command2.ExecuteNonQuery();
                }
                // Create the third table
                using (var command2 = new SqlCommand(createTableSql3, connection))
                {
                    command2.ExecuteNonQuery();
                }
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


        private void EmailControlString(WTUsers emailControlString)
        {
            try
            {

                string directoryPath = "Configuration";
                string fileName = "EmailController.json";
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
                        ["FromEmail"] = "",
                        ["FromEmailPassword"] = "",
                        ["PortNumber"] = "",
                        ["EmmilClearSettings"] = new JObject
                        {
                            ["ClearHours"] = 1,
                        },

                        ["EmailList"] = new JArray()
                    };
                }
                else
                {
                    // JSON verisi mevcutsa, onu bir nesneye çevir
                    jsonObject = JObject.Parse(jsonData);
                }

                // EmailList dizisine kullanýcýlarý ekle
                JArray emailList = (JArray)jsonObject["EmailList"];
                foreach (var user in emailControlString.Users)
                {
                    var existingUser = emailList.FirstOrDefault(u => (string)u["ID"] == user.ID);
                    if (existingUser != null)
                    {
                        existingUser["Name"] = user.Name;
                        existingUser["EMail"] = user.EMail;
                        existingUser["FullName"] = user.FullName;
                    }
                    else
                    {
                        // Ayný ID'ye sahip bir kullanýcý yoksa, yeni kullanýcý olarak ekle
                        JObject newUser = new JObject
                        {
                            ["ID"] = user.ID,
                            ["Name"] = user.Name,
                            ["EMail"] = user.EMail,
                            ["FullName"] = user.FullName,
                            ["Durum"] = false // Varsayýlan olarak Durum false olarak ayarlanabilir
                        };
                        emailList.Add(newUser);
                    }

                    //// Eðer ayný ID'ye sahip bir kullanýcý zaten varsa, eklemeyi atla
                    //if (emailList.Any(u => (string)u["ID"] == user.ID))
                    //{
                    //    continue;
                    //}

                    //JObject newUser = new JObject
                    //{
                    //    ["ID"] = user.ID,
                    //    ["Name"] = user.Name,
                    //    ["EMail"] = user.EMail,
                    //    ["FullName"] = user.FullName,
                    //    ["Durum"] = false // Varsayýlan olarak Durum false olarak ayarlanabilir
                    //};
                    //emailList.Add(newUser);
                }

                // JSON dosyasýndaki kullanýcýlarý kontrol et ve eþleþmeyenleri kaldýr
                foreach (var email in emailList.ToList())
                {
                    // JSON'da olan bir kullanýcý ID'si, API'den gelen kullanýcýlar listesinde yoksa, kaldýr
                    if (!emailControlString.Users.Any(u => u.ID == (string)email["ID"]))
                    {
                        email.Remove();
                    }
                }

                // JSON nesnesini dosyaya geri yaz
                File.WriteAllText(filePath, JsonConvert.SerializeObject(jsonObject, Formatting.Indented));


            }
            catch (Exception ex)
            {
                notificatonSettings("HATA" + ex.Message);
                MessageBox.Show("HATA !", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
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


        private void ShowData()
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
                    txtKullaniciAdi.Text = jsonObject["KullaniciAdi"].ToString(); ;
                    txtParola.Text = jsonObject["Parola"].ToString(); ;



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
                MessageBox.Show("HATA !", ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
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


                await Task.Run(() => AutoPost(cancellationTokenSource.Token, anlikTarih));


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
                                if (state == "RELEASED" || state == "INWORK" || state == "CANCELLED" || state == "ALTERNATE_RELEASED")
                                {
                                    apiAdres = item["api_adres"].ToString();
                                    anaKaynak = item["ana_kaynak"].ToString();
                                    endPoint = item["alt_endpoint"].ToString();
                                    apiFullUrl = apiAdres + "/" + anaKaynak;
                                }
                            }

                            //else if (sourceApi.Contains("CADDocumentMgmt") && sablonDataDurumu == "true")
                            //{
                            //    if (state == "RELEASED" || state == "INWORK" || state == "CANCELLED" || state == "ALTERNATE_RELEASED" || state == "REMOVED_PART" || state == "SEND_FILE" || state == "CADSTOK")
                            //    {
                            //        apiAdres = item["api_adres"].ToString();
                            //        anaKaynak = item["ana_kaynak"].ToString();
                            //        endPoint = item["alt_endpoint"].ToString();
                            //        apiFullUrl = apiAdres + "/" + anaKaynak;
                            //    }
                            //}

                            if (sablonDataDurumu == "true" && state != "INWORK")
                            {
                                await ProcessStateAsync(state, catalogValue, conn, apiFullUrl, apiURL, CSRF_NONCE, WindchillServerName, ServerName, BasicUsername, BasicPassword, anlikTarih, sourceApi, endPoint, oldAlternateLinkCount, sablonDataDurumu, API_ENDPOINT_ALTERNATE_PART, API_ENDPOINT_REMOVED, API_ENDPOINT_SEND_FILE);
                                await ProcessReviseAsync(state, catalogValue, conn);
                            }

                            if (sablonDataDurumu == "true" && state == "INWORK")
                            {
                                await ProcessInworkAsync(state, catalogValue, conn, apiFullUrl, apiURL, CSRF_NONCE, WindchillServerName, ServerName, BasicUsername, BasicPassword, anlikTarih, sourceApi, endPoint, oldAlternateLinkCount, sablonDataDurumu);
                                await ProcessReviseAsync(state, catalogValue, conn);
                            }
                        }

                    }

                    await Task.Delay(5000, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                notificatonSettings("Hata!" + ex.Message);
                MessageBox.Show(ex.Message);
            }
            finally
            {
            }
        }

        private async Task ProcessStateAsync(string state, string catalogValue, SqlConnection conn, string apiFullUrl, string apiURL, string CSRF_NONCE, string WindchillServerName, string ServerName, string BasicUsername, string BasicPassword, DateTime anlikTarih, string sourceApi, string endPoint, int oldAlternateLinkCount, string sablonDataDurumu, string API_ENDPOINT_ALTERNATE_PART, string API_ENDPOINT_REMOVED, string API_ENDPOINT_SEND_FILE)
        {

            var ViewKod = $"SELECT  [idA2A2] FROM {catalogValue}.WTView WHERE [name] = 'Design'";

            var resolvedViewKod = await conn.QueryFirstOrDefaultAsync<dynamic>(ViewKod);

            bool ilkCalistirmaProdMgmt = true;
            bool ilkCalistirmaCADDocumentMgmt = true;
            var sql = "";
            var formattedTarih = "";
            var formattedTarih2 = "";



            if (sourceApi.Contains("ProdMgmt"))
            {
                if (ilkCalistirmaProdMgmt)
                {
                    formattedTarih = DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                    formattedTarih2 = DateTime.Today.AddDays(-3).ToString("yyyy.MM.dd HH:mm:ss.fffffff");
                    if (state == "ALTERNATE_RELEASED")
                    {
                        sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = 'RELEASED' and [latestiterationInfo] = 1 and [idA3view] = {resolvedViewKod.idA2A2} and (updateStampA2 >= @formattedTarih or updateStampA2 >= @formattedTarih2)";
                    }
                    else
                    {
                        sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = '{state}' and [latestiterationInfo] = 1  and [idA3view] = {resolvedViewKod.idA2A2} and (updateStampA2 >= @formattedTarih or updateStampA2 >= @formattedTarih2)";
                    }
                    ilkCalistirmaProdMgmt = false;
                }
                else
                {
                    formattedTarih = DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                    formattedTarih2 = DateTime.Today.ToString("yyyy.MM.dd HH:mm:ss.fffffff");
                    if (state == "ALTERNATE_RELEASED")
                    {
                        sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = 'RELEASED' and [latestiterationInfo] = 1  and [idA3view] = {resolvedViewKod.idA2A2} and (updateStampA2 >= @formattedTarih or updateStampA2 >= @formattedTarih2)";
                    }
                    else
                    {
                        sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = '{state}' and [latestiterationInfo] = 1  and [idA3view] = {resolvedViewKod.idA2A2} and (updateStampA2 >= @formattedTarih or updateStampA2 >= @formattedTarih2)";
                    }
                }

            }

            if (sourceApi.Contains("CADDocumentMgmt"))
            {

                formattedTarih = DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                formattedTarih2 = DateTime.Today.ToString("yyyy.MM.dd HH:mm:ss.fffffff");
                sql = $"SELECT [Ent_ID], [EPMDocID], [StateDegeri] FROM {catalogValue}.Ent_EPMDocState WHERE [StateDegeri] = 'RELEASED'";


            }






            var resolvedItems = await conn.QueryAsync<dynamic>(sql, new { formattedTarih, formattedTarih2 });

            //var sql2 = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2],[versionIdA2versionInfo],[versionLevelA2versionInfo] FROM {catalogValue}.WTPart WHERE [statestate] = 'INWORK' and [latestiterationInfo] = 1 and [idA3masterReference] = {resolvedItems.idA3masterReference} and MAX([versionIdA2versionInfo])";



            WTUsers globalUserEmail = new WTUsers();

            try
            {
                WindchillApiService windchillApiService = new WindchillApiService();
                foreach (var partItem in resolvedItems)
                {

                    var json = "";
                    var cadJSON = "";
                    var cadJSON2 = "";
                    var jsonWTUSER = "";
                    var cadAssociationsJSON = "";
                    if (sourceApi.Contains("ProdMgmt"))
                    {
                        json = await windchillApiService.GetApiData(WindchillServerName, $"{sourceApi + partItem.idA2A2}')?$expand=Alternates($expand=AlternatePart)", BasicUsername, BasicPassword, CSRF_NONCE);
                        jsonWTUSER = await windchillApiService.GetApiData(WindchillServerName,$"PrincipalMgmt/Users?$select=EMail,Name,FullName", BasicUsername, BasicPassword, CSRF_NONCE);
                    }
                    if (sourceApi.Contains("CADDocumentMgmt"))
                    {
                        cadJSON = await windchillApiService.GetApiData(WindchillServerName, $"{sourceApi + partItem.EPMDocID}')?$expand=Attachments", BasicUsername, BasicPassword, CSRF_NONCE);
                        cadAssociationsJSON = await windchillApiService.GetApiData(WindchillServerName, $"{sourceApi + partItem.EPMDocID}')/PartDocAssociations", BasicUsername, BasicPassword, CSRF_NONCE);
                     cadJSON2 = await windchillApiService.GetApiData(WindchillServerName, $"CADDocumentMgmt/CADDocuments('OR:wt.epm.EPMDocument:{partItem.EPMDocID}')?$expand=Representations", BasicUsername, BasicPassword, CSRF_NONCE);
                    }










                    try
                    {
                        if (sourceApi.Contains("ProdMgmt"))
                        {
                            var response = JsonConvert.DeserializeObject<Part>(json);
                            var responseCreator = JsonConvert.DeserializeObject<WTUsers>(jsonWTUSER);
                            EmailControlString(responseCreator);
                            globalUserEmail = responseCreator;
                            var turkishDateFormat2 = response.LastModified.ToString();
                            var iso8601Date2 = ConvertToIso8601Format(turkishDateFormat2);
                         
                            response.LastModified = Convert.ToDateTime(iso8601Date2);

                            var existingLog = await conn.QueryFirstOrDefaultAsync<WTChangeOrder2MasterViewModel>(
                                $"SELECT [idA2A2],[statestate], [ProcessTimestamp], [updateStampA2] FROM [{catalogValue}].[Change_Notice_LogTable] WHERE [idA2A2] = @idA2A2",
                                new { idA2A2 = response.ID.Split(':')[2] });






                            if (state != "ALTERNATE_RELEASED")
                            {

                                if (existingLog == null)
                                {

                                    await InsertLogAndPostDataAsync(response, catalogValue, conn, apiFullUrl, apiURL, endPoint, state, partItem.idA2A2);


                                }

                                //else if (existingLog.updateStampA2 != partItem.updateStampA2)
                                else if ((existingLog.statestate != response.State.Value) || (existingLog.updateStampA2 != response.LastModified))
                                {
                                    await UpdateLogAndPostDataAsync(response, catalogValue, conn, apiFullUrl, apiURL, endPoint, state, partItem.idA2A2);

                                }



                                //Sürekli versiyon anladýðýnda yeni parçalar çýkýyor bu parçalarda tetik aldýðý için sonsuz döngü oluyor 
                                //checkOut ve checkIn yapmadan güncelleme iþlemi yapmamýz lazým bu yüzden bunu iptal ediyorum.



                            }


                            #region AlternateLinkVeriSayýsý
                            var newAlternateLinkCount = (await conn.QueryAsync<dynamic>(
                            $"SELECT * FROM [{catalogValue}].[WTPartAlternateLink]")).ToList();
                            int newAlternateLinkVeriSayisi = newAlternateLinkCount.Count();



                            #endregion


                            //Bunu yapmamýzýn sebebii muadil parça eklenmeden önce eðer muadil parçacanýn ana parçasý önce gönderilmez ise onu göndermeyi saðlamak

                            bool alternatePartControlBoolType = false;
                            bool alternatePartControlBoolType2 = false;

                            foreach (var item in response.Alternates)
                            {
                                var alternateParcaControl = await conn.QueryAsync<WTChangeOrder2MasterViewModel>(
                         $"SELECT [idA2A2],[statestate], [ProcessTimestamp], [updateStampA2] FROM [{catalogValue}].[Change_Notice_LogTable] WHERE [idA2A2] = @idA2A2",
                         new { idA2A2 = item.AlternatePart.ID.Split(':')[2] });
                                alternatePartControlBoolType = alternateParcaControl.Any(x => x.statestate == item.AlternatePart.State.Value);

                                var alternateParcaControl2 = await conn.QueryAsync<WTChangeOrder2MasterViewModel>(
                         $"SELECT [idA2A2],[statestate], [ProcessTimestamp], [updateStampA2] FROM [{catalogValue}].[Change_Notice_LogTable] WHERE [WTPartNumber] = @WTPartNumber",
                         new { WTPartNumber = item.AlternatePart.Number });
                                alternatePartControlBoolType2 = alternateParcaControl2.Any(x => x.statestate == item.AlternatePart.State.Value);
                            }


                            if (response.State.Value == "RELEASED" && response.Alternates != null && response.Alternates.Count != 0 && state == "ALTERNATE_RELEASED" && (alternatePartControlBoolType || alternatePartControlBoolType2))
                            {

                                foreach (var item in response.Alternates)
                                {


                                    //var alternateLinkLogs = await conn.QueryFirstOrDefaultAsync<WTPartAlternateLink_LOG>(
                                    //$"SELECT * FROM [{catalogValue}].[WTPartAlternateLink_LOG] WHERE [AnaParcaID] = @AnaParcaID,[AnaParcaNumber] = @AnaParcaNumber,[AnaParcaName] = @AnaParcaName,[ID] = @ID, [Name] = @Name,[Number] = @Number",
                                    //new { AnaParcaID= response.ID, AnaParcaNumber = response.Number, AnaParcaName = response.Name, ID = item.AlternatePart.ID.Split(':')[2],Name = item.AlternatePart.Name,Number = item.AlternatePart.Number });


                                    var alternateLinkLogs = await conn.QueryFirstOrDefaultAsync<WTPartAlternateLink_LOG>(
                                    $@"
    SELECT *
    FROM [{catalogValue}].[WTPartAlternateLink_LOG]
    WHERE [AnaParcaID] = @AnaParcaID
    AND [AnaParcaNumber] = @AnaParcaNumber
    AND [AnaParcaName] = @AnaParcaName
    AND [ID] = @ID
    AND [Name] = @Name
    AND [Number] = @Number",
                                    new
                                    {
                                        AnaParcaID = response.ID,
                                        AnaParcaNumber = response.Number,
                                        AnaParcaName = response.Name,
                                        ID = item.AlternatePart.ID.Split(':')[2],
                                        Name = item.AlternatePart.Name,
                                        Number = item.AlternatePart.Number
                                    });



                                    //AlternateLinkde bulunan sayýlarda uyuþmazlýk var ise tekrar post edilicek veriler birisndee çakýþma olabilir.

                                    if (newAlternateLinkVeriSayisi < oldAlternateLinkCount)
                                    {
                                        int fark = oldAlternateLinkCount - newAlternateLinkVeriSayisi;
                                        oldAlternateLinkCount -= fark;
                                    }
                                    else if (newAlternateLinkVeriSayisi > oldAlternateLinkCount)
                                    {
                                        int fark = newAlternateLinkVeriSayisi - oldAlternateLinkCount;
                                        oldAlternateLinkCount += fark;
                                    }
                                    //AlternateLinkde bulunan sayýlarda uyuþmazlýk var ise tekrar post edilicek veriler birisndee çakýþma olabilir.


                                    var kekw = new Part
                                    {

                                        BirimAdi = response.BirimAdi,
                                        Alternates = new List<Alternates>
                                    {
                                        new Alternates
                                        {
                                        AlternatePart = item.AlternatePart,
                                        CreatedOn = item.CreatedOn,
                                        ID = item.ID,
                                        LastModified = item.LastModified,
                                        ObjectType = item.ObjectType,
                                        }
                                    },
                                        BirimKodu = response.BirimKodu,
                                        CLASSIFICATION = response.CLASSIFICATION,
                                        CreatedOn = response.CreatedOn,
                                        Description = response.Description,
                                        ID = response.ID,
                                        LastModified = response.LastModified,
                                        MuhasebeAdi = response.MuhasebeAdi,
                                        //MuhasebeKodu = response.MuhasebeKodu,
                                        Name = response.Name,
                                        Number = response.Number,
                                        State = response.State,
                                        Version = response.Version,
                                        VersionID = response.VersionID

                                    };




                                    // Tarih ve saat karþýlaþtýrmasý

                                    /*

									Kontrol yapýcaz bu kontrol de eðer ana parça boþ muadil parça bilgisi dolu ise yeni muadil olarak ekleyeceðiz yani ana parça boþtan kastýmýz ana parça log tablosunda yer almayacak ama number deðeri yer alacak bu durumda baþka parça entegre olmuþ muadil parça olacak
									tam tersi durumunda ana parça log tablosunda mevcut ise o zaman güncelleme yapacak

									 */





                                    if (response.State.Value == "RELEASED" && item.AlternatePart.State.Value == "RELEASED" && (alternateLinkLogs == null))
                                    {
                                        await RELEASED_AlternatesInsertLogAndPostDataAsync(kekw, item, catalogValue, conn, apiFullUrl, apiURL, endPoint);
                                    }
                                    //else if (item.AlternatePart.State.Value == "RELEASED" && (item.LastModified != alternateLinkLogs.ProcessTimestamp) && (alternateLinkLogs.AnaParcaNumber != null && alternateLinkLogs.Number != null))
                                    //{
                                    //    await RELEASED_AlternatesUpdateLogAndPostDataAsync(kekw, item, catalogValue, conn, apiFullUrl, apiURL, endPoint);
                                    //}

                                    //}else if (item.AlternatePart.State.Value == "RELEASED" && (item.AlternatePart.LastModified.Value.Date != alternateLinkLogs.ProcessTimestamp.Date && item.AlternatePart.LastModified.Value.Hour != alternateLinkLogs.ProcessTimestamp.Hour && item.AlternatePart.LastModified.Value.Minute != alternateLinkLogs.ProcessTimestamp.Minute))
                                    //    {
                                    //        await RELEASED_AlternatesUpdateLogAndPostDataAsync(kekw, item, catalogValue, conn, apiFullUrl, apiURL, endPoint);
                                    //    }

                                    //if(alternateLinkLogs != null)
                                    //{
                                    //    DateTime lastModified = Convert.ToDateTime(item.AlternatePart.LastModified);
                                    //    DateTime processTimestamp = alternateLinkLogs.ProcessTimestamp;

                                    //    // Tarih ve saati karþýlaþtýrmak için saniye ve daha küçük zaman birimlerini göz ardý etme
                                    //    DateTime roundedLastModified = new DateTime(lastModified.Year, lastModified.Month, lastModified.Day, lastModified.Hour, lastModified.Minute,  0);
                                    //    DateTime roundedProcessTimestamp = new DateTime(processTimestamp.Year, processTimestamp.Month, processTimestamp.Day, processTimestamp.Hour, processTimestamp.Minute, 0);
                                    //    roundedProcessTimestamp = roundedProcessTimestamp.AddHours(3);
                                    //    if (item.AlternatePart.State.Value == "RELEASED" && roundedLastModified != roundedProcessTimestamp)
                                    //{
                                    //    await RELEASED_AlternatesUpdateLogAndPostDataAsync(kekw, item, catalogValue, conn, apiFullUrl, apiURL, endPoint);
                                    //}
                                    //}
                                    //else if (item.AlternatePart.State.Value == "RELEASED" && (item.AlternatePart.LastModified != alternateLinkLogs.ProcessTimestamp))
                                    //{
                                    //    await RELEASED_AlternatesUpdateLogAndPostDataAsync(kekw, item, catalogValue, conn, apiFullUrl, apiURL, endPoint);
                                    //    //await RELEASED_AlternatesUpdateLogAndPostDataAsync(response, item, catalogValue, conn, apiFullUrl, apiURL, "ALTERNATE_RELEASED");
                                    //}

                                }
                            }

                        }
                        if (sourceApi.Contains("CADDocumentMgmt"))
                        {
                            var CADResponse = JsonConvert.DeserializeObject<RootObject>(cadJSON2);
                            string partCode = "";
                            if (cadAssociationsJSON != null)
                            {
                                var CADAssociationsResponse = JsonConvert.DeserializeObject<CADDocumentResponse>(cadAssociationsJSON);
                                if (CADAssociationsResponse != null || CADAssociationsResponse.Value.Count != 0)
                                {
                                    var CADAssociations = CADAssociationsResponse.Value.SingleOrDefault().ID;
                                    string pattern = @"OR:wt\.part\.WTPart:(\d+)_Calculated_OR:wt\.epm\.EPMDocument:";
                                    Regex regex = new Regex(pattern);
                                    Match match = regex.Match(CADAssociations);

                                    if (match.Success)
                                    {
                                        partCode = match.Groups[1].Value;

                                    }

                                }

                            }

                            try
                            {

                                if (cadJSON2 != null || cadJSON2 != "")
                                {

                                    var representation = CADResponse.Representations.SingleOrDefault();
                                    if (representation != null)
                                    {
                                        if (representation.AdditionalFiles != null && representation.AdditionalFiles.Count > 0)
                                        {

                                            foreach (var item in CADResponse.Representations.SingleOrDefault().AdditionalFiles)
                                        {

                                        if (item != null)
                                        {

                                            //var pdfSettings = CADResponse.Attachments.FirstOrDefault().Content;
                                            if (item.FileName.Contains(".pdf") || item.FileName.Contains(".PDF"))
                                            {
                                                var pdfUrl = item.URL;
                                                var pdfFileName = item.Label.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase).ToString();
                                                await SendPdfToCustomerFunctionAsync(pdfUrl, pdfFileName, apiFullUrl, apiURL, endPoint, partItem.EPMDocID, catalogValue, conn, CADResponse, state, partCode);
                                            }
                                        }
                                        }
                                    }
                                    }


                                }

                            }
                            catch (Exception)
                            {

                            }
                            // If LastUpdateTimestamp has not changed, do nothing
                        }

                        //if (sourceApi.Contains("CADDocumentMgmt"))
                        //{
                        //    var CADResponse = JsonConvert.DeserializeObject<TeknikResim>(cadJSON);
                        //    string partCode = "";
                        //    if (cadAssociationsJSON != null)
                        //    {
                        //    var CADAssociationsResponse = JsonConvert.DeserializeObject<CADDocumentResponse>(cadAssociationsJSON);
                        //        if (CADAssociationsResponse != null || CADAssociationsResponse.Value.Count != 0)
                        //        {
                        //            var CADAssociations = CADAssociationsResponse.Value.SingleOrDefault().ID;
                        //        string pattern = @"OR:wt\.part\.WTPart:(\d+)_Calculated_OR:wt\.epm\.EPMDocument:";
                        //        Regex regex = new Regex(pattern);
                        //        Match match = regex.Match(CADAssociations);

                        //        if (match.Success)
                        //        {
                        //             partCode = match.Groups[1].Value;

                        //        }

                        //   }

                        //    }

                        //    try
                        //    {

                        //        if (cadJSON != null || cadJSON != "")
                        //        {

                        //            if (CADResponse.Attachments != null && CADResponse.Attachments.Count > 0)
                        //            {

                        //                //var selectedAttachment = CADResponse.Attachments.FirstOrDefault(a => a.Content != null && a.Content.Label != null && a.Content.Label.Contains(CADResponse.Number));
                        //                var selectedAttachment = CADResponse.Attachments.FirstOrDefault(a =>
                        //                a.Content != null &&
                        //                a.Content.Label != null &&
                        //                a.Content.Label.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                        //            );

                        //                if (selectedAttachment != null)
                        //                {

                        //                    //var pdfSettings = CADResponse.Attachments.FirstOrDefault().Content;
                        //                    if (selectedAttachment is not null)
                        //                    {
                        //                        var pdfUrl = selectedAttachment.Content.URL;
                        //                        var pdfFileName = selectedAttachment.Content.Label;
                        //                        await SendPdfToCustomerFunctionAsync(pdfUrl, pdfFileName, apiFullUrl, apiURL, endPoint, partItem.EPMDocID, catalogValue, conn, CADResponse, state, partCode);
                        //                    }
                        //                }
                        //            }


                        //        }

                        //    }
                        //    catch (Exception)
                        //    {

                        //    }
                        //    // If LastUpdateTimestamp has not changed, do nothing
                        //}










                    }
                    catch (Exception)
                    {
                        continue;
                        // Handle the exception
                    }
                }



                try
                {


                    // WTPartAlternateLink tablosundaki tüm verileri al
                    var AlternateLinkDatas = await conn.QueryAsync<WTPartAlternateLink>($"SELECT * FROM [{catalogValue}].[WTPartAlternateLink]");

                    // WTPartAlternateLink_ControlLog tablosundaki tüm verileri al
                    var AlternateLinkLogDatas = await conn.QueryAsync<WTPartAlternateLink>($"SELECT * FROM [{catalogValue}].[WTPartAlternateLink_ControlLog]");

                    // Yeni eklenen verileri bul
                    var newAddedData = AlternateLinkDatas.Except(AlternateLinkLogDatas, new WTPartAlternateLinkComparer());

                    // Eðer yeni eklenen veriler varsa, bu verileri ekleyin
                    if (newAddedData.Any())
                    {
                        foreach (var item in newAddedData)
                        {
                            var wtpartAlternatePart = await conn.QueryFirstOrDefaultAsync<dynamic>(
           $@"
    SELECT * 
    FROM [{catalogValue}].[WTPart] 
    WHERE [idA3MasterReference] = @idA3MasterReference 
    AND latestiterationInfo = '1' 
    AND statestate = 'RELEASED'
    AND versionIdA2versionInfo = (
        SELECT MAX(versionIdA2versionInfo) 
        FROM [{catalogValue}].[WTPart]
        WHERE [idA3MasterReference] = @idA3MasterReference
    )",
           new { idA3MasterReference = item.IdA3B5 });


                            // Parçanýn RELEASED durumda olup olmadýðýný kontrol edin
                            if (wtpartAlternatePart != null && wtpartAlternatePart.statestate == "RELEASED")
                            {
                                // Yeni eklenen verileri WTPartAlternateLink_ControlLog tablosuna ekleyin
                                await conn.ExecuteAsync($@"
                INSERT INTO [{catalogValue}].[WTPartAlternateLink_ControlLog] (
                    AdministrativeLockIsNull, 
                    TypeAdministrativeLock, 
                    ClassNameKeyDomainRef, 
                    IdA3DomainRef, 
                    InheritedDomain, 
                    ReplacementType, 
                    ClassNameKeyRoleAObjectRef, 
                    IdA3A5, 
                    ClassNameKeyRoleBObjectRef, 
                    IdA3B5, 
                    SecurityLabels, 
                    CreateStampA2, 
                    MarkForDeleteA2, 
                    ModifyStampA2, 
                    ClassNameA2A2, 
                    IdA2A2, 
                    UpdateCountA2, 
                    UpdateStampA2
                ) 
                VALUES (
                    @AdministrativeLockIsNull, 
                    @TypeAdministrativeLock, 
                    @ClassNameKeyDomainRef, 
                    @IdA3DomainRef, 
                    @InheritedDomain, 
                    @ReplacementType, 
                    @ClassNameKeyRoleAObjectRef, 
                    @IdA3A5, 
                    @ClassNameKeyRoleBObjectRef, 
                    @IdA3B5, 
                    @SecurityLabels, 
                    @CreateStampA2, 
                    @MarkForDeleteA2, 
                    @ModifyStampA2, 
                    @ClassNameA2A2, 
                    @IdA2A2, 
                    @UpdateCountA2, 
                    @UpdateStampA2
                )", new
                                {
                                    item.AdministrativeLockIsNull,
                                    item.TypeAdministrativeLock,
                                    item.ClassNameKeyDomainRef,
                                    item.IdA3DomainRef,
                                    item.InheritedDomain,
                                    item.ReplacementType,
                                    item.ClassNameKeyRoleAObjectRef,
                                    item.IdA3A5,
                                    item.ClassNameKeyRoleBObjectRef,
                                    item.IdA3B5,
                                    item.SecurityLabels,
                                    item.CreateStampA2,
                                    item.MarkForDeleteA2,
                                    item.ModifyStampA2,
                                    item.ClassNameA2A2,
                                    item.IdA2A2,
                                    item.UpdateCountA2,
                                    item.UpdateStampA2
                                });
                            }
                        }
                    }



                    // Silinen verileri kontrol et
                    var deletedData = AlternateLinkLogDatas.Except(AlternateLinkDatas, new WTPartAlternateLinkComparer1());

                    // Eðer silinen veriler varsa, bu verileri iþleyin
                    if (deletedData.Any() && state == "ALTERNATE_RELEASED" && sablonDataDurumu == "true")
                    {
                        foreach (var item in deletedData)
                        {

                            var wtpart = (await conn.QueryFirstOrDefaultAsync<dynamic>(
            $"SELECT * FROM [{catalogValue}].[WTPart] WHERE [idA3MasterReference] = @idA3MasterReference and latestiterationInfo = '1'",
            new { idA3MasterReference = item.IdA3A5 }));

                            var wtpartAlternatePart = (await conn.QueryFirstOrDefaultAsync<dynamic>(
            $"SELECT * FROM [{catalogValue}].[WTPart] WHERE [idA3MasterReference] = @idA3MasterReference and latestiterationInfo = '1'",
            new { idA3MasterReference = item.IdA3B5 }));

                                           var wtparControlLog = (await conn.QueryFirstOrDefaultAsync<dynamic>(
            $"SELECT * FROM [{catalogValue}].[WTPartAlternateLink_ControlLog] WHERE [IdA3A5] = @IdA3A5 and [IdA3B5] = @IdA3B5",
            new { IdA3A5 = item.IdA3A5, IdA3B5 = item.IdA3B5 }));


                            if (wtparControlLog != null)
                            {
                                var wtpartMasterAlternatePart = (await conn.QueryFirstOrDefaultAsync<dynamic>(
                            $"SELECT * FROM [{catalogValue}].[WTPartMaster] WHERE [idA2A2] = @idA2A2",
                            new { idA2A2 = item.IdA3B5 }));


                            var removedJson = await windchillApiService.GetApiData(WindchillServerName, $"{sourceApi + wtpart.idA2A2}')?$expand=Alternates($expand=AlternatePart;$filter=startswith(AlternatePart/ID,'OR:wt.part.WTPart:{wtpartAlternatePart.idA2A2}'))", BasicUsername, BasicPassword, CSRF_NONCE);
                            var removedResponse = JsonConvert.DeserializeObject<Part>(removedJson);



                            removedResponse.Alternates = new List<Alternates>();
                        
                                // Check if wtpartMasterAlternatePart is not null
                                if (wtpartMasterAlternatePart != null)
                            {
                                // Create a single Alternates object with necessary data
                                // (Include all required fields)
                                var alternates = new Alternates
                                {
                                    ID = "wt.part.WTPartAlternateLink:" + wtpartAlternatePart.idA2A2,
                                    AlternatePart = new AlternatePart
                                    {
                                        ID = "OR:wt.part.WTPart:" + wtpartMasterAlternatePart.idA2A2,
                                        Name = wtpartMasterAlternatePart.name,
                                        Number = wtpartMasterAlternatePart.WTPartNumber,
                                        State = new Designtech_PLM_Entegrasyon_AutoPost.Model.WindchillApiModel.State
                                        {
                                            // State sýnýfýna ait diðer alanlarý doldurun
                                            Value = wtpartAlternatePart.statestate,
                                        }
                                        // Add other required fields (State, MuhasebeKodu, MuhasebeAdi, etc.)
                                    }
                                };

                                // Add the complete alternates object to the list
                                removedResponse.Alternates.Add(alternates);
                            }




                            // Silinen verileri WTPartAlternateLink_ControlLog tablosundan sil
                            await RemovedLogAndPostDataAsync(removedResponse, catalogValue, conn, apiFullUrl, apiURL, endPoint);

                            await conn.ExecuteAsync($@"
				DELETE FROM [{catalogValue}].[WTPartAlternateLink_ControlLog]
				 WHERE [IdA3A5] = @IdA3A5 and [IdA3B5] = @IdA3B5", new { IdA3A5 = item.IdA3A5, IdA3B5 = item.IdA3B5 });

                            //                        await conn.ExecuteAsync($@"
                            //DELETE FROM [{catalogValue}].[WTPartAlternateLink_ControlLog]
                            //WHERE IdA2A2 IN @Ids", new { Ids = deletedData.Select(d => d.IdA2A2).ToArray() });

                            //                        await conn.ExecuteAsync($@"
                            //DELETE FROM [{catalogValue}].[WTPartAlternateLink_LOG]
                            //WHERE Number IN @Ids", new { Ids = removedResponse.Alternates.Select(d => d.AlternatePart.Number).ToArray() });

                            await conn.ExecuteAsync($@"
    DELETE FROM [{catalogValue}].[WTPartAlternateLink_LOG]
    WHERE AnaParcaNumber = @AnaParcaNumber AND Number IN @Numbers",
                     new
                     {
                         AnaParcaNumber = removedResponse.Number,
                         Numbers = removedResponse.Alternates.Select(d => d.AlternatePart.Number).ToArray()
                     });

                            }

                        }
                    }

                    //AlternateLink Silininen verileri bulma için karþýlaþtýrma yapýlan foksiyon

                }
                catch (Exception)
                {
                }

            }
            catch (Exception ex)
            {
                notificatonSettings("Hata!" + ex.Message);
                MessageBox.Show(ex.Message);
            }

        }






        private async Task ProcessInworkAsync(string state, string catalogValue, SqlConnection conn, string apiFullUrl, string apiURL, string CSRF_NONCE, string WindchillServerName, string ServerName, string BasicUsername, string BasicPassword, DateTime anlikTarih, string sourceApi, string endPoint, int oldAlternateLinkCount, string sablonDataDurumu)
        {

            //bool ilkCalistirmaProdMgmt = true;
            //bool ilkCalistirmaCADDocumentMgmt = true;
            var sql = "";
            var formattedTarih = DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
            var formattedTarih2 = DateTime.Today.ToString("yyyy.MM.dd HH:mm:ss.fffffff");

            var ViewKod = $"SELECT  [idA2A2] FROM {catalogValue}.WTView WHERE [name] = 'Design'";

            var resolvedViewKod = await conn.QueryFirstOrDefaultAsync<dynamic>(ViewKod);

            if (sourceApi.Contains("ProdMgmt"))
            {
                sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = '{state}' and [latestiterationInfo] = 1  and [idA3view] =  {resolvedViewKod.idA2A2}  and (updateStampA2 >= @formattedTarih or updateStampA2 >= @formattedTarih2)";
            }

            if (sourceApi.Contains("CADDocumentMgmt"))
            {


                sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.EPMDocument WHERE [statestate] = '{state}' and [latestiterationInfo] = 1 and (updateStampA2 >= @formattedTarih or updateStampA2 >= @formattedTarih2)";


            }





            var resolvedItems = await conn.QueryAsync<dynamic>(sql, new { formattedTarih, formattedTarih2 });






            try
            {
                WindchillApiService windchillApiService = new WindchillApiService();
                foreach (var partItem in resolvedItems)
                {

                    var json = "";
                    if (sourceApi.Contains("ProdMgmt"))
                    {
                        json = await windchillApiService.GetApiData(WindchillServerName, $"{sourceApi + partItem.idA2A2}')", BasicUsername, BasicPassword, CSRF_NONCE);
                    }

                    if (sourceApi.Contains("CADDocumentMgmt"))
                    {
                        json = await windchillApiService.GetApiData(ServerName, $"{sourceApi + partItem.idA2A2}')", BasicUsername, BasicPassword, CSRF_NONCE);
                    }







                    try
                    {
                        var response = JsonConvert.DeserializeObject<Part>(json);




                        var turkishDateFormat2 = response.LastModified.ToString();
                        var iso8601Date2 = ConvertToIso8601Format(turkishDateFormat2);

                        response.LastModified = Convert.ToDateTime(iso8601Date2);

                        var existingLog = await conn.QueryFirstOrDefaultAsync<WTChangeOrder2MasterViewModel>(
                            $"SELECT [idA2A2],[statestate], [ProcessTimestamp], [updateStampA2] FROM [{catalogValue}].[Change_Notice_LogTable] WHERE [idA2A2] = @idA2A2",
                            new { idA2A2 = response.ID.Split(':')[2] });




                        if (state != "ALTERNATE_RELEASED")
                        {

                            if (existingLog == null)
                            {

                                continue;

                                //await InsertLogProcessInworkAsync(response, catalogValue, conn);

                            }
                            //else if (existingLog.updateStampA2 != partItem.updateStampA2)
                            else if ((existingLog.statestate != response.State.Value) || (existingLog.updateStampA2 != response.LastModified))
                            {
                                await UpdateLogProcessInworkAsync(response, catalogValue, conn);
                                if (response.EntegrasyonDurumu is null or not "Parça devam ediyor" && state == "INWORK")
                                {

                                    await EntegrasyonDurumUpdate(state, partItem.idA2A2);
                                    //await EntegrasyonDurumCheckOut(partItem.idA2A2, state);
                                }

                            }



                        }

                    }
                    catch (Exception)
                    {
                        // Handle the exception
                    }
                }



            }
            catch (Exception ex)
            {
                notificatonSettings("Hata!" + ex.Message);
                MessageBox.Show(ex.Message);
            }



        }





        private async Task ProcessReviseAsync(string state, string catalogValue, SqlConnection conn)
        {

            var sql = "";
            var formattedTarih = DateTime.Today.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
            var formattedTarih2 = DateTime.Today.ToString("yyyy.MM.dd HH:mm:ss.fffffff");

            var ViewKod = $"SELECT  [idA2A2] FROM {catalogValue}.WTView WHERE [name] = 'Design'";

            var resolvedViewKod = await conn.QueryFirstOrDefaultAsync<dynamic>(ViewKod);



            sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = '{state}' and [latestiterationInfo] = 1  and [idA3view] =  {resolvedViewKod.idA2A2}  and (updateStampA2 >= @formattedTarih or updateStampA2 >= @formattedTarih2)";


            var resolvedItems = await conn.QueryAsync<dynamic>(sql, new { formattedTarih, formattedTarih2 });






            try
            {
                WindchillApiService windchillApiService = new WindchillApiService();
                foreach (var partItem in resolvedItems)
                {
                    if (state != "ALTERNATE_RELEASED")
                    {

                        #region WTPART Revizyon iþlemleri
                        try
                        {
                            var sql2 = "";




                            sql2 = $@"
    SELECT
        [idA2A2],
        [idA3masterReference],
        [statestate],
        [updateStampA2],
        [versionIdA2versionInfo],
        [versionLevelA2versionInfo]
    FROM
        {catalogValue}.WTPart
    WHERE
        [statestate] = 'INWORK'
        AND [latestiterationInfo] = 1
        AND [idA3masterReference] = {partItem.idA3masterReference}
        AND [versionIdA2versionInfo] != 'A'
        AND (updateStampA2 >= @formattedTarih or updateStampA2 >= @formattedTarih2)
     
";

                            var revisedItems = await conn.QueryAsync<dynamic>(sql2, new { formattedTarih, formattedTarih2 });


                            if (revisedItems.Count() != 0)
                            {
                                foreach (var partRevisedItem in revisedItems)
                                {

                                    var existingLog2 = await conn.QueryFirstOrDefaultAsync<WTChangeOrder2MasterViewModel>(
                            $"SELECT [idA2A2],[statestate], [ProcessTimestamp], [updateStampA2],[ReviseDate]  FROM [{catalogValue}].[Change_Notice_LogTable] WHERE [idA2A2] = @idA2A2",
                            new { idA2A2 = partRevisedItem.idA2A2 });
                                    if (existingLog2 == null)
                                    {
                                        await EntegrasyonDurumRESET(partRevisedItem.statestate, partRevisedItem.idA2A2, partRevisedItem.updateStampA2);
                                    }

                                    if (existingLog2 != null && partRevisedItem.updateStampA2 != existingLog2.ReviseDate)
                                    {

                                        await EntegrasyonDurumRESET(partRevisedItem.statestate, partRevisedItem.idA2A2, partRevisedItem.updateStampA2);
                                    }

                                }
                            }
                        }
                        catch (Exception)
                        {

                            throw;
                        }
                        #endregion

                    }

                }



            }
            catch (Exception)
            {
            }



        }

        #region Entegrasyon-Durum-Ayarlarý

        private async Task EntegrasyonDurumUpdate(string state, long KodidA2A2)
        {
            try
            {

                WindchillApiService windchillApiService = new WindchillApiService();

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

                WrsToken apiToken = await windchillApiService.GetApiToken(WindchillServerName, BasicUsername, BasicPassword);

                //var sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = 'RELEASED' and [latestiterationInfo] = 1 and statecheckoutInfo = 'wrk'";
                //var resolvedItems = await conn.QueryFirstAsync<dynamic>(sql);


                var IdSeq = $"SELECT [value] FROM {catalogValue}.id_sequence ORDER BY [value] DESC";
                var resolvedIdSeq = await conn.QueryFirstOrDefaultAsync<dynamic>(IdSeq);
                long respIdSeq = Convert.ToInt64(resolvedIdSeq.value) + 100;

                var message = "";
                var existingRecordStringDefinitionSeq = $"SELECT * FROM [{catalogValue}].[StringDefinition] WHERE [displayName] = 'Entegrasyon Durumu'";
                var existingRecordStringDefinition = await conn.QueryFirstOrDefaultAsync<dynamic>(existingRecordStringDefinitionSeq);

                var existingRecordTimestampDefinitionSeq = $"SELECT * FROM [{catalogValue}].[TimestampDefinition] WHERE [displayName] = 'Entegrasyon Tarihi'";
                var existingRecordTimestampDefinition = await conn.QueryFirstOrDefaultAsync<dynamic>(existingRecordTimestampDefinitionSeq);

                if (state == "RELEASED")
                {
                    //var sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = 'RELEASED' and [latestiterationInfo] = 1 and statecheckoutInfo = 'wrk' and idA3E2iterationInfo = {idA2A2}";
                    //var resolvedItems = await conn.QueryFirstOrDefaultAsync<dynamic>(sql);

                    //message = "{\r\n  \"EntegrasyonDurumu\": \"Entegregrasyon gerçekleþtirildi\"\r\n}";


                    //DateTime currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                    var content = $"{{\r\n  \"EntegrasyonDurumu\": \"Parça entegre oldu\",\r\n  \"EntegrasyonTarihi\": \"{currentDate}\"\r\n}}";

                    //var IdSeq = _plm.Query(catalogValue + ".id_sequence").OrderByDesc("value").FirstOrDefault();







                    // Öncelikle, mevcut kaydý kontrol edin
                    var existingRecord = await conn.QueryFirstOrDefaultAsync(
                 $"SELECT * FROM [{catalogValue}].[StringValue] WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordStringDefinition.idA2A2}",
                 new { KodidA2A2 });
                    var existingRecordTimeStamp = await conn.QueryFirstOrDefaultAsync(
                   $"SELECT * FROM [{catalogValue}].[TimestampValue] WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordTimestampDefinition.idA2A2}",
                   new { KodidA2A2 });




                    if (existingRecord != null)
                    {
                        // Mevcut bir kayýt varsa, güncelleme iþlemi yapýn
                        int result = await conn.ExecuteAsync(
                            $"UPDATE [{catalogValue}].[StringValue] " +
                            "SET " +
                            "[hierarchyIDA6] = @hierarchyIDA6, " +
                            "[idA2A2] = @idA2A2, " +
                            "[classnameA2A2] = @classnameA2A2, " +
                            "[idA3A5] = @idA3A5, " +
                            "[idA3A6] = @idA3A6, " +
                            "[markForDeleteA2] = @markForDeleteA2, " +
                            "[modifyStampA2] = @modifyStampA2, " +
                            "[updateCountA2] = @updateCountA2, " +
                            "[updateStampA2] = @updateStampA2, " +
                            "[createStampA2] = @createStampA2, " +
                            "[classnamekeyA4] = @classnamekeyA4, " +
                            "[classnamekeyA6] = @classnamekeyA6, " +
                            "[value] = @value, " +
                            "[value2] = @value2 " +
                            $"WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordStringDefinition.idA2A2}",
                            new
                            {
                                hierarchyIDA6 = "7058085483721066086",
                                idA2A2 = respIdSeq,
                                classnameA2A2 = "wt.iba.value.StringValue",
                                idA3A5 = 0,
                                idA3A6 = Convert.ToInt64(existingRecordStringDefinition.idA2A2),
                                markForDeleteA2 = 0,
                                modifyStampA2 = DateTime.Now.Date,
                                updateCountA2 = 1,
                                updateStampA2 = DateTime.Now.Date,
                                createStampA2 = DateTime.Now.Date,
                                classnamekeyA4 = "wt.part.WTPart",
                                classnamekeyA6 = "wt.iba.definition.StringDefinition",
                                value = "PARÇA ENTEGRE OLDU",
                                value2 = "Parça entegre oldu",
                                KodidA2A2
                            });




                        // Güncelleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }
                    else
                    {
                        // Mevcut bir kayýt yoksa, yeni bir kayýt ekleyin
                        int result = await conn.ExecuteAsync(
                            $"INSERT INTO [{catalogValue}].[StringValue] " +
                            "([hierarchyIDA6], [idA2A2], [idA3A4], [classnameA2A2], [idA3A5], [idA3A6], [markForDeleteA2], [modifyStampA2], [updateCountA2], [updateStampA2], [createStampA2], [classnamekeyA4], [classnamekeyA6], [value], [value2]) " +
                            "VALUES (@hierarchyIDA6, @idA2A2, @idA3A4, @classnameA2A2, @idA3A5, @idA3A6, @markForDeleteA2, @modifyStampA2, @updateCountA2, @updateStampA2, @createStampA2, @classnamekeyA4, @classnamekeyA6, @value, @value2)",
                            new
                            {
                                hierarchyIDA6 = "7058085483721066086",
                                idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
                                idA3A4 = Convert.ToInt64(KodidA2A2),
                                classnameA2A2 = "wt.iba.value.StringValue",
                                idA3A5 = 0,
                                idA3A6 = Convert.ToInt64(existingRecordStringDefinition.idA2A2),
                                markForDeleteA2 = 0,
                                modifyStampA2 = DateTime.Now.Date,
                                updateCountA2 = 1,
                                updateStampA2 = DateTime.Now.Date,
                                createStampA2 = DateTime.Now.Date,
                                classnamekeyA4 = "wt.part.WTPart",
                                classnamekeyA6 = "wt.iba.definition.StringDefinition",
                                value = "PARÇA ENTEGRE OLDU",
                                value2 = "Parça entegre oldu"
                            });





                        // Yeni ekleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }

                    var controlDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                    DateTime controlDate2 = Convert.ToDateTime(controlDate);
                    if (existingRecordTimeStamp != null)
                    {


                        // TimestampValue tablosunu güncelleyin
                        int result = await conn.ExecuteAsync(
                            $"UPDATE [{catalogValue}].[TimestampValue] " +
                            "SET " +
                            "[hierarchyIDA6] = @hierarchyIDA6, " +
                            "[idA2A2] = @idA2A2, " +
                            "[classnameA2A2] = @classnameA2A2, " +
                            "[idA3A5] = @idA3A5, " +
                            "[idA3A6] = @idA3A6, " +
                            "[markForDeleteA2] = @markForDeleteA2, " +
                            "[modifyStampA2] = @modifyStampA2, " +
                            "[updateCountA2] = @updateCountA2, " +
                            "[updateStampA2] = @updateStampA2, " +
                            "[createStampA2] = @createStampA2, " +
                            "[classnamekeyA4] = @classnamekeyA4, " +
                            "[classnamekeyA6] = @classnamekeyA6, " +
                            "[value] = @value " +
                            $"WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordTimestampDefinition.idA2A2}",
                            new
                            {
                                hierarchyIDA6 = "-148878178526147486",
                                idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
                                classnameA2A2 = "wt.iba.value.TimestampValue",
                                idA3A5 = 0,
                                idA3A6 = Convert.ToInt64(existingRecordTimestampDefinition.idA2A2),
                                markForDeleteA2 = 0,
                                modifyStampA2 = DateTime.Now.Date,
                                updateCountA2 = 1,
                                updateStampA2 = DateTime.Now.Date,
                                createStampA2 = DateTime.Now.Date,
                                classnamekeyA4 = "wt.part.WTPart",
                                classnamekeyA6 = "wt.iba.definition.TimestampDefinition",
                                value = controlDate2,
                                KodidA2A2
                            });

                        // Güncelleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }
                    else
                    {


                        int result = await conn.ExecuteAsync(
    $"INSERT INTO [{catalogValue}].[TimestampValue] " +
    "([hierarchyIDA6], [idA2A2], [idA3A4], [classnameA2A2], [idA3A5], [idA3A6], [markForDeleteA2], [modifyStampA2], [updateCountA2], [updateStampA2], [createStampA2], [classnamekeyA4], [classnamekeyA6], [value]) " +
    "VALUES (@hierarchyIDA6, @idA2A2, @idA3A4, @classnameA2A2, @idA3A5, @idA3A6, @markForDeleteA2, @modifyStampA2, @updateCountA2, @updateStampA2, @createStampA2, @classnamekeyA4, @classnamekeyA6, @value)",
    new
    {
        hierarchyIDA6 = "-148878178526147486",
        idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
        idA3A4 = Convert.ToInt64(KodidA2A2),
        classnameA2A2 = "wt.iba.value.TimestampValue",
        idA3A5 = 0,
        idA3A6 = Convert.ToInt64(existingRecordTimestampDefinition.idA2A2),
        markForDeleteA2 = 0,
        modifyStampA2 = DateTime.Now.Date,
        updateCountA2 = 1,
        updateStampA2 = DateTime.Now.Date,
        createStampA2 = DateTime.Now.Date,
        classnamekeyA4 = "wt.part.WTPart",
        classnamekeyA6 = "wt.iba.definition.TimestampDefinition",
        value = controlDate2,

    });


                        // Yeni ekleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }


                    //await windchillApiService.EntegrasyonDurumUpdateAPI(ServerName, "ProdMgmt/Parts('OR:wt.part.WTPart:" + resolvedItems.idA2A2 + "')", BasicUsername, BasicPassword, apiToken.NonceValue, "Parça entegre oldu",currentDate);
                }
                if (state == "CANCELLED")
                {
                    //var sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = 'CANCELLED' and [latestiterationInfo] = 1 and statecheckoutInfo = 'wrk' and idA3E2iterationInfo = {idA2A2}";
                    //var resolvedItems = await conn.QueryFirstOrDefaultAsync<dynamic>(sql);

                    //message = "{\r\n  \"EntegrasyonDurumu\": \"Entegrasyon iptal edildi\"\r\n}";

                    //var currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var currentDate = DateTime.Now.ToString("yyyy-MM-dd");

                    //var IdSeq = _plm.Query(catalogValue + ".id_sequence").OrderByDesc("value").FirstOrDefault();

                    // Öncelikle, mevcut kaydý kontrol edin
                    var existingRecord = await conn.QueryFirstOrDefaultAsync(
                     $"SELECT * FROM [{catalogValue}].[StringValue] WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordStringDefinition.idA2A2}",
                     new { KodidA2A2 });
                    var existingRecordTimeStamp = await conn.QueryFirstOrDefaultAsync(
                   $"SELECT * FROM [{catalogValue}].[TimestampValue] WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordTimestampDefinition.idA2A2}",
                   new { KodidA2A2 });

                    if (existingRecord != null)
                    {
                        // Mevcut bir kayýt varsa, güncelleme iþlemi yapýn
                        int result = await conn.ExecuteAsync(
                            $"UPDATE [{catalogValue}].[StringValue] " +
                            "SET " +
                            "[hierarchyIDA6] = @hierarchyIDA6, " +
                            "[idA2A2] = @idA2A2, " +
                            "[classnameA2A2] = @classnameA2A2, " +
                            "[idA3A5] = @idA3A5, " +
                            "[idA3A6] = @idA3A6, " +
                            "[markForDeleteA2] = @markForDeleteA2, " +
                            "[modifyStampA2] = @modifyStampA2, " +
                            "[updateCountA2] = @updateCountA2, " +
                            "[updateStampA2] = @updateStampA2, " +
                            "[createStampA2] = @createStampA2, " +
                            "[classnamekeyA4] = @classnamekeyA4, " +
                            "[classnamekeyA6] = @classnamekeyA6, " +
                            "[value] = @value, " +
                            "[value2] = @value2 " +
                            $"WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordStringDefinition.idA2A2}",
                            new
                            {
                                hierarchyIDA6 = "7058085483721066086",
                                idA2A2 = respIdSeq,
                                classnameA2A2 = "wt.iba.value.StringValue",
                                idA3A5 = 0,
                                idA3A6 = Convert.ToInt64(existingRecordStringDefinition.idA2A2),
                                markForDeleteA2 = 0,
                                modifyStampA2 = DateTime.Now.Date,
                                updateCountA2 = 1,
                                updateStampA2 = DateTime.Now.Date,
                                createStampA2 = DateTime.Now.Date,
                                classnamekeyA4 = "wt.part.WTPart",
                                classnamekeyA6 = "wt.iba.definition.StringDefinition",
                                value = "PARÇA ÝPTAL OLDU",
                                value2 = "Parça iptal oldu",
                                KodidA2A2
                            });



                        // Güncelleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }
                    else
                    {
                        // Mevcut bir kayýt yoksa, yeni bir kayýt ekleyin
                        int result = await conn.ExecuteAsync(
                            $"INSERT INTO [{catalogValue}].[StringValue] " +
                            "([hierarchyIDA6], [idA2A2], [idA3A4], [classnameA2A2], [idA3A5], [idA3A6], [markForDeleteA2], [modifyStampA2], [updateCountA2], [updateStampA2], [createStampA2], [classnamekeyA4], [classnamekeyA6], [value], [value2]) " +
                            "VALUES (@hierarchyIDA6, @idA2A2, @idA3A4, @classnameA2A2, @idA3A5, @idA3A6, @markForDeleteA2, @modifyStampA2, @updateCountA2, @updateStampA2, @createStampA2, @classnamekeyA4, @classnamekeyA6, @value, @value2)",
                            new
                            {
                                hierarchyIDA6 = "7058085483721066086",
                                idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
                                idA3A4 = Convert.ToInt64(KodidA2A2),
                                classnameA2A2 = "wt.iba.value.StringValue",
                                idA3A5 = 0,
                                idA3A6 = Convert.ToInt64(existingRecordStringDefinition.idA2A2),
                                markForDeleteA2 = 0,
                                modifyStampA2 = DateTime.Now.Date,
                                updateCountA2 = 1,
                                updateStampA2 = DateTime.Now.Date,
                                createStampA2 = DateTime.Now.Date,
                                classnamekeyA4 = "wt.part.WTPart",
                                classnamekeyA6 = "wt.iba.definition.StringDefinition",
                                value = "PARÇA ÝPTAL OLDU",
                                value2 = "Parça iptal oldu"
                            });




                        // Yeni ekleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }

                    var controlDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                    DateTime controlDate2 = Convert.ToDateTime(controlDate);
                    if (existingRecordTimeStamp != null)
                    {


                        // TimestampValue tablosunu güncelleyin
                        int result = await conn.ExecuteAsync(
                            $"UPDATE [{catalogValue}].[TimestampValue] " +
                            "SET " +
                            "[hierarchyIDA6] = @hierarchyIDA6, " +
                            "[idA2A2] = @idA2A2, " +
                            "[classnameA2A2] = @classnameA2A2, " +
                            "[idA3A5] = @idA3A5, " +
                            "[idA3A6] = @idA3A6, " +
                            "[markForDeleteA2] = @markForDeleteA2, " +
                            "[modifyStampA2] = @modifyStampA2, " +
                            "[updateCountA2] = @updateCountA2, " +
                            "[updateStampA2] = @updateStampA2, " +
                            "[createStampA2] = @createStampA2, " +
                            "[classnamekeyA4] = @classnamekeyA4, " +
                            "[classnamekeyA6] = @classnamekeyA6, " +
                            "[value] = @value " +
                            $"WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordTimestampDefinition.idA2A2}",
                            new
                            {
                                hierarchyIDA6 = "-148878178526147486",
                                idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
                                classnameA2A2 = "wt.iba.value.TimestampValue",
                                idA3A5 = 0,
                                idA3A6 = Convert.ToInt64(existingRecordTimestampDefinition.idA2A2),
                                markForDeleteA2 = 0,
                                modifyStampA2 = DateTime.Now.Date,
                                updateCountA2 = 1,
                                updateStampA2 = DateTime.Now.Date,
                                createStampA2 = DateTime.Now.Date,
                                classnamekeyA4 = "wt.part.WTPart",
                                classnamekeyA6 = "wt.iba.definition.TimestampDefinition",
                                value = controlDate2,
                                KodidA2A2
                            });

                        // Güncelleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }
                    else
                    {


                        int result = await conn.ExecuteAsync(
    $"INSERT INTO [{catalogValue}].[TimestampValue] " +
    "([hierarchyIDA6], [idA2A2], [idA3A4], [classnameA2A2], [idA3A5], [idA3A6], [markForDeleteA2], [modifyStampA2], [updateCountA2], [updateStampA2], [createStampA2], [classnamekeyA4], [classnamekeyA6], [value]) " +
    "VALUES (@hierarchyIDA6, @idA2A2, @idA3A4, @classnameA2A2, @idA3A5, @idA3A6, @markForDeleteA2, @modifyStampA2, @updateCountA2, @updateStampA2, @createStampA2, @classnamekeyA4, @classnamekeyA6, @value)",
    new
    {
        hierarchyIDA6 = "-148878178526147486",
        idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
        idA3A4 = Convert.ToInt64(KodidA2A2),
        classnameA2A2 = "wt.iba.value.TimestampValue",
        idA3A5 = 0,
        idA3A6 = Convert.ToInt64(existingRecordTimestampDefinition.idA2A2),
        markForDeleteA2 = 0,
        modifyStampA2 = DateTime.Now.Date,
        updateCountA2 = 1,
        updateStampA2 = DateTime.Now.Date,
        createStampA2 = DateTime.Now.Date,
        classnamekeyA4 = "wt.part.WTPart",
        classnamekeyA6 = "wt.iba.definition.TimestampDefinition",
        value = controlDate2,

    });


                        // Yeni ekleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }


                    //var content = $"{{\r\n  \"EntegrasyonDurumu\": \"Parça iptal oldu\",\r\n  \"EntegrasyonTarihi\": \"{currentDate}\"\r\n}}";
                    //await windchillApiService.EntegrasyonDurumUpdateAPI(ServerName, "ProdMgmt/Parts('OR:wt.part.WTPart:" + resolvedItems.idA2A2 + "')", BasicUsername, BasicPassword, apiToken.NonceValue, "Parça iptal oldu",currentDate);
                }
                if (state == "INWORK")
                {
                    //var sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = 'INWORK' and [latestiterationInfo] = 1 and statecheckoutInfo = 'wrk' and idA3E2iterationInfo = {idA2A2}";
                    //var resolvedItems = await conn.QueryFirstOrDefaultAsync<dynamic>(sql);

                    //var currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var currentDate = DateTime.Now.ToString("yyyy-MM-dd");


                    //var IdSeq = _plm.Query(catalogValue + ".id_sequence").OrderByDesc("value").FirstOrDefault();

                    // Öncelikle, mevcut kaydý kontrol edin
                    var existingRecord = await conn.QueryFirstOrDefaultAsync(
                    $"SELECT * FROM [{catalogValue}].[StringValue] WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordStringDefinition.idA2A2}",
                    new { KodidA2A2 });
                    var existingRecordTimeStamp = await conn.QueryFirstOrDefaultAsync(
                   $"SELECT * FROM [{catalogValue}].[TimestampValue] WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordTimestampDefinition.idA2A2}",
                   new { KodidA2A2 });

                    if (existingRecord != null)
                    {
                        // Mevcut bir kayýt varsa, güncelleme iþlemi yapýn
                        int result = await conn.ExecuteAsync(
                            $"UPDATE [{catalogValue}].[StringValue] " +
                            "SET " +
                            "[hierarchyIDA6] = @hierarchyIDA6, " +
                            "[idA2A2] = @idA2A2, " +
                            "[classnameA2A2] = @classnameA2A2, " +
                            "[idA3A5] = @idA3A5, " +
                            "[idA3A6] = @idA3A6, " +
                            "[markForDeleteA2] = @markForDeleteA2, " +
                            "[modifyStampA2] = @modifyStampA2, " +
                            "[updateCountA2] = @updateCountA2, " +
                            "[updateStampA2] = @updateStampA2, " +
                            "[createStampA2] = @createStampA2, " +
                            "[classnamekeyA4] = @classnamekeyA4, " +
                            "[classnamekeyA6] = @classnamekeyA6, " +
                            "[value] = @value, " +
                            "[value2] = @value2 " +
                            $"WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordStringDefinition.idA2A2}",
                            new
                            {
                                hierarchyIDA6 = "7058085483721066086",
                                idA2A2 = respIdSeq,
                                classnameA2A2 = "wt.iba.value.StringValue",
                                idA3A5 = 0,
                                idA3A6 = Convert.ToInt64(existingRecordStringDefinition.idA2A2),
                                markForDeleteA2 = 0,
                                modifyStampA2 = DateTime.Now.Date,
                                updateCountA2 = 1,
                                updateStampA2 = DateTime.Now.Date,
                                createStampA2 = DateTime.Now.Date,
                                classnamekeyA4 = "wt.part.WTPart",
                                classnamekeyA6 = "wt.iba.definition.StringDefinition",
                                value = "",
                                value2 = "",
                                KodidA2A2
                            });

                        // Güncelleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin


                        // Güncelleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }
                    else
                    {
                        // Mevcut bir kayýt yoksa, yeni bir kayýt ekleyin
                        int result = await conn.ExecuteAsync(
                            $"INSERT INTO [{catalogValue}].[StringValue] " +
                            "([hierarchyIDA6], [idA2A2], [idA3A4], [classnameA2A2], [idA3A5], [idA3A6], [markForDeleteA2], [modifyStampA2], [updateCountA2], [updateStampA2], [createStampA2], [classnamekeyA4], [classnamekeyA6], [value], [value2]) " +
                            "VALUES (@hierarchyIDA6, @idA2A2, @idA3A4, @classnameA2A2, @idA3A5, @idA3A6, @markForDeleteA2, @modifyStampA2, @updateCountA2, @updateStampA2, @createStampA2, @classnamekeyA4, @classnamekeyA6, @value, @value2)",
                            new
                            {
                                hierarchyIDA6 = "7058085483721066086",
                                idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
                                idA3A4 = Convert.ToInt64(KodidA2A2),
                                classnameA2A2 = "wt.iba.value.StringValue",
                                idA3A5 = 0,
                                idA3A6 = Convert.ToInt64(existingRecordStringDefinition.idA2A2),
                                markForDeleteA2 = 0,
                                modifyStampA2 = DateTime.Now.Date,
                                updateCountA2 = 1,
                                updateStampA2 = DateTime.Now.Date,
                                createStampA2 = DateTime.Now.Date,
                                classnamekeyA4 = "wt.part.WTPart",
                                classnamekeyA6 = "wt.iba.definition.StringDefinition",
                                value = "",
                                value2 = ""
                            });


                        // Yeni ekleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }

                    var controlDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                    DateTime controlDate2 = Convert.ToDateTime(controlDate);
                    if (existingRecordTimeStamp != null)
                    {


                        // TimestampValue tablosunu güncelleyin
                        int result = await conn.ExecuteAsync(
                            $"UPDATE [{catalogValue}].[TimestampValue] " +
                            "SET " +
                            "[hierarchyIDA6] = @hierarchyIDA6, " +
                            "[idA2A2] = @idA2A2, " +
                            "[classnameA2A2] = @classnameA2A2, " +
                            "[idA3A5] = @idA3A5, " +
                            "[idA3A6] = @idA3A6, " +
                            "[markForDeleteA2] = @markForDeleteA2, " +
                            "[modifyStampA2] = @modifyStampA2, " +
                            "[updateCountA2] = @updateCountA2, " +
                            "[updateStampA2] = @updateStampA2, " +
                            "[createStampA2] = @createStampA2, " +
                            "[classnamekeyA4] = @classnamekeyA4, " +
                            "[classnamekeyA6] = @classnamekeyA6, " +
                            "[value] = @value " +
                            $"WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordTimestampDefinition.idA2A2}",
                            new
                            {
                                hierarchyIDA6 = "-148878178526147486",
                                idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
                                classnameA2A2 = "wt.iba.value.TimestampValue",
                                idA3A5 = 0,
                                idA3A6 = Convert.ToInt64(existingRecordTimestampDefinition.idA2A2),
                                markForDeleteA2 = 0,
                                modifyStampA2 = DateTime.Now.Date,
                                updateCountA2 = 1,
                                updateStampA2 = DateTime.Now.Date,
                                createStampA2 = DateTime.Now.Date,
                                classnamekeyA4 = "wt.part.WTPart",
                                classnamekeyA6 = "wt.iba.definition.TimestampDefinition",
                                value = "",
                                KodidA2A2
                            });

                        // Güncelleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }
                    else
                    {


                        int result = await conn.ExecuteAsync(
    $"INSERT INTO [{catalogValue}].[TimestampValue] " +
    "([hierarchyIDA6], [idA2A2], [idA3A4], [classnameA2A2], [idA3A5], [idA3A6], [markForDeleteA2], [modifyStampA2], [updateCountA2], [updateStampA2], [createStampA2], [classnamekeyA4], [classnamekeyA6], [value]) " +
    "VALUES (@hierarchyIDA6, @idA2A2, @idA3A4, @classnameA2A2, @idA3A5, @idA3A6, @markForDeleteA2, @modifyStampA2, @updateCountA2, @updateStampA2, @createStampA2, @classnamekeyA4, @classnamekeyA6, @value)",
    new
    {
        hierarchyIDA6 = "-148878178526147486",
        idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
        idA3A4 = Convert.ToInt64(KodidA2A2),
        classnameA2A2 = "wt.iba.value.TimestampValue",
        idA3A5 = 0,
        idA3A6 = Convert.ToInt64(existingRecordTimestampDefinition.idA2A2),
        markForDeleteA2 = 0,
        modifyStampA2 = DateTime.Now.Date,
        updateCountA2 = 1,
        updateStampA2 = DateTime.Now.Date,
        createStampA2 = DateTime.Now.Date,
        classnamekeyA4 = "wt.part.WTPart",
        classnamekeyA6 = "wt.iba.definition.TimestampDefinition",
        value = "",

    });


                        // Yeni ekleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }


                    //var content = $"{{\r\n  \"EntegrasyonDurumu\": \"Parça devam ediyor\",\r\n  \"EntegrasyonTarihi\": \"{currentDate}\"\r\n}}";
                    //await windchillApiService.EntegrasyonDurumUpdateAPI(ServerName, "ProdMgmt/Parts('OR:wt.part.WTPart:" + resolvedItems.idA2A2 + "')", BasicUsername, BasicPassword, apiToken.NonceValue, "Parça devam ediyor",currentDate);
                }

                //await windchillApiService.EntegrasyonDurumUpdateAPI(ServerName, "ProdMgmt/Parts('OR:wt.part.WTPart:" + resolvedItems.idA2A2 + "')/PTC.ProdMgmt.CheckOut", BasicUsername, BasicPassword, apiToken.NonceValue, "{\r\n  \"EntegrasyonDurumu\": \"Entegre oldu1\"\r\n}");



            }
            catch (Exception)
            {

            }
        }

        private async Task EntegrasyonHataDurumUpdate(long KodidA2A2)
        {
            try
            {

                WindchillApiService windchillApiService = new WindchillApiService();

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

                WrsToken apiToken = await windchillApiService.GetApiToken(WindchillServerName, BasicUsername, BasicPassword);

                //var sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = 'RELEASED' and [latestiterationInfo] = 1 and statecheckoutInfo = 'wrk'";
                //var resolvedItems = await conn.QueryFirstAsync<dynamic>(sql);


                var IdSeq = $"SELECT [value] FROM {catalogValue}.id_sequence ORDER BY [value] DESC";
                var resolvedIdSeq = await conn.QueryFirstOrDefaultAsync<dynamic>(IdSeq);
                long respIdSeq = Convert.ToInt64(resolvedIdSeq.value) + 100;

                var message = "";
                var existingRecordStringDefinitionSeq = $"SELECT * FROM [{catalogValue}].[StringDefinition] WHERE [displayName] = 'Entegrasyon Durumu'";
                var existingRecordStringDefinition = await conn.QueryFirstOrDefaultAsync<dynamic>(existingRecordStringDefinitionSeq);

                var existingRecordTimestampDefinitionSeq = $"SELECT * FROM [{catalogValue}].[TimestampDefinition] WHERE [displayName] = 'Entegrasyon Tarihi'";
                var existingRecordTimestampDefinition = await conn.QueryFirstOrDefaultAsync<dynamic>(existingRecordTimestampDefinitionSeq);

             
                   
                    var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                    var content = $"{{\r\n  \"EntegrasyonDurumu\": \"Parça entegre edilemedi\",\r\n  \"EntegrasyonTarihi\": \"{currentDate}\"\r\n}}";








                    // Öncelikle, mevcut kaydý kontrol edin
                    var existingRecord = await conn.QueryFirstOrDefaultAsync(
                 $"SELECT * FROM [{catalogValue}].[StringValue] WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordStringDefinition.idA2A2}",
                 new { KodidA2A2 });
                    var existingRecordTimeStamp = await conn.QueryFirstOrDefaultAsync(
                   $"SELECT * FROM [{catalogValue}].[TimestampValue] WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordTimestampDefinition.idA2A2}",
                   new { KodidA2A2 });




                    if (existingRecord != null)
                    {
                        // Mevcut bir kayýt varsa, güncelleme iþlemi yapýn
                        int result = await conn.ExecuteAsync(
                            $"UPDATE [{catalogValue}].[StringValue] " +
                            "SET " +
                            "[hierarchyIDA6] = @hierarchyIDA6, " +
                            "[idA2A2] = @idA2A2, " +
                            "[classnameA2A2] = @classnameA2A2, " +
                            "[idA3A5] = @idA3A5, " +
                            "[idA3A6] = @idA3A6, " +
                            "[markForDeleteA2] = @markForDeleteA2, " +
                            "[modifyStampA2] = @modifyStampA2, " +
                            "[updateCountA2] = @updateCountA2, " +
                            "[updateStampA2] = @updateStampA2, " +
                            "[createStampA2] = @createStampA2, " +
                            "[classnamekeyA4] = @classnamekeyA4, " +
                            "[classnamekeyA6] = @classnamekeyA6, " +
                            "[value] = @value, " +
                            "[value2] = @value2 " +
                            $"WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordStringDefinition.idA2A2}",
                            new
                            {
                                hierarchyIDA6 = "7058085483721066086",
                                idA2A2 = respIdSeq,
                                classnameA2A2 = "wt.iba.value.StringValue",
                                idA3A5 = 0,
                                idA3A6 = Convert.ToInt64(existingRecordStringDefinition.idA2A2),
                                markForDeleteA2 = 0,
                                modifyStampA2 = DateTime.Now.Date,
                                updateCountA2 = 1,
                                updateStampA2 = DateTime.Now.Date,
                                createStampA2 = DateTime.Now.Date,
                                classnamekeyA4 = "wt.part.WTPart",
                                classnamekeyA6 = "wt.iba.definition.StringDefinition",
                                value = "PARÇA ENTEGRE EDÝLEMEDÝ",
                                value2 = "Parça entegre edilemedi",
                                KodidA2A2
                            });




                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }
                    else
                    {
                        // Mevcut bir kayýt yoksa, yeni bir kayýt ekleyin
                        int result = await conn.ExecuteAsync(
                            $"INSERT INTO [{catalogValue}].[StringValue] " +
                            "([hierarchyIDA6], [idA2A2], [idA3A4], [classnameA2A2], [idA3A5], [idA3A6], [markForDeleteA2], [modifyStampA2], [updateCountA2], [updateStampA2], [createStampA2], [classnamekeyA4], [classnamekeyA6], [value], [value2]) " +
                            "VALUES (@hierarchyIDA6, @idA2A2, @idA3A4, @classnameA2A2, @idA3A5, @idA3A6, @markForDeleteA2, @modifyStampA2, @updateCountA2, @updateStampA2, @createStampA2, @classnamekeyA4, @classnamekeyA6, @value, @value2)",
                            new
                            {
                                hierarchyIDA6 = "7058085483721066086",
                                idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
                                idA3A4 = Convert.ToInt64(KodidA2A2),
                                classnameA2A2 = "wt.iba.value.StringValue",
                                idA3A5 = 0,
                                idA3A6 = Convert.ToInt64(existingRecordStringDefinition.idA2A2),
                                markForDeleteA2 = 0,
                                modifyStampA2 = DateTime.Now.Date,
                                updateCountA2 = 1,
                                updateStampA2 = DateTime.Now.Date,
                                createStampA2 = DateTime.Now.Date,
                                classnamekeyA4 = "wt.part.WTPart",
                                classnamekeyA6 = "wt.iba.definition.StringDefinition",
                                value = "PARÇA ENTEGRE EDÝLEMEDÝ",
                                value2 = "Parça entegre edilemedi"
                            });





                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }

                    var controlDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                    DateTime controlDate2 = Convert.ToDateTime(controlDate);
                    if (existingRecordTimeStamp != null)
                    {


                        // TimestampValue tablosunu güncelleyin
                        int result = await conn.ExecuteAsync(
                            $"UPDATE [{catalogValue}].[TimestampValue] " +
                            "SET " +
                            "[hierarchyIDA6] = @hierarchyIDA6, " +
                            "[idA2A2] = @idA2A2, " +
                            "[classnameA2A2] = @classnameA2A2, " +
                            "[idA3A5] = @idA3A5, " +
                            "[idA3A6] = @idA3A6, " +
                            "[markForDeleteA2] = @markForDeleteA2, " +
                            "[modifyStampA2] = @modifyStampA2, " +
                            "[updateCountA2] = @updateCountA2, " +
                            "[updateStampA2] = @updateStampA2, " +
                            "[createStampA2] = @createStampA2, " +
                            "[classnamekeyA4] = @classnamekeyA4, " +
                            "[classnamekeyA6] = @classnamekeyA6, " +
                            "[value] = @value " +
                            $"WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordTimestampDefinition.idA2A2}",
                            new
                            {
                                hierarchyIDA6 = "-148878178526147486",
                                idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
                                classnameA2A2 = "wt.iba.value.TimestampValue",
                                idA3A5 = 0,
                                idA3A6 = Convert.ToInt64(existingRecordTimestampDefinition.idA2A2),
                                markForDeleteA2 = 0,
                                modifyStampA2 = DateTime.Now.Date,
                                updateCountA2 = 1,
                                updateStampA2 = DateTime.Now.Date,
                                createStampA2 = DateTime.Now.Date,
                                classnamekeyA4 = "wt.part.WTPart",
                                classnamekeyA6 = "wt.iba.definition.TimestampDefinition",
                                value = controlDate2,
                                KodidA2A2
                            });

                        // Güncelleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }
                    else
                    {


                        int result = await conn.ExecuteAsync(
    $"INSERT INTO [{catalogValue}].[TimestampValue] " +
    "([hierarchyIDA6], [idA2A2], [idA3A4], [classnameA2A2], [idA3A5], [idA3A6], [markForDeleteA2], [modifyStampA2], [updateCountA2], [updateStampA2], [createStampA2], [classnamekeyA4], [classnamekeyA6], [value]) " +
    "VALUES (@hierarchyIDA6, @idA2A2, @idA3A4, @classnameA2A2, @idA3A5, @idA3A6, @markForDeleteA2, @modifyStampA2, @updateCountA2, @updateStampA2, @createStampA2, @classnamekeyA4, @classnamekeyA6, @value)",
    new
    {
        hierarchyIDA6 = "-148878178526147486",
        idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
        idA3A4 = Convert.ToInt64(KodidA2A2),
        classnameA2A2 = "wt.iba.value.TimestampValue",
        idA3A5 = 0,
        idA3A6 = Convert.ToInt64(existingRecordTimestampDefinition.idA2A2),
        markForDeleteA2 = 0,
        modifyStampA2 = DateTime.Now.Date,
        updateCountA2 = 1,
        updateStampA2 = DateTime.Now.Date,
        createStampA2 = DateTime.Now.Date,
        classnamekeyA4 = "wt.part.WTPart",
        classnamekeyA6 = "wt.iba.definition.TimestampDefinition",
        value = controlDate2,

    });


                        // Yeni ekleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }


             

            }
            catch (Exception)
            {

            }
        }

        private async Task EntegrasyonDurumRESET(string state, long KodidA2A2, DateTime ReviseDate)
        {
            try
            {

                WindchillApiService windchillApiService = new WindchillApiService();

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

                WrsToken apiToken = await windchillApiService.GetApiToken(WindchillServerName, BasicUsername, BasicPassword);

                //var sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = 'RELEASED' and [latestiterationInfo] = 1 and statecheckoutInfo = 'wrk'";
                //var resolvedItems = await conn.QueryFirstAsync<dynamic>(sql);


                var IdSeq = $"SELECT [value] FROM {catalogValue}.id_sequence ORDER BY [value] DESC";
                var resolvedIdSeq = await conn.QueryFirstOrDefaultAsync<dynamic>(IdSeq);
                long respIdSeq = Convert.ToInt64(resolvedIdSeq.value) + 100;

                var message = "";

                var existingRecordStringDefinitionSeq = $"SELECT * FROM [{catalogValue}].[StringDefinition] WHERE [displayName] = 'Entegrasyon Durumu'";
                var existingRecordStringDefinition = await conn.QueryFirstOrDefaultAsync<dynamic>(existingRecordStringDefinitionSeq);

                var existingRecordTimestampDefinitionSeq = $"SELECT * FROM [{catalogValue}].[TimestampDefinition] WHERE [displayName] = 'Entegrasyon Tarihi'";
                var existingRecordTimestampDefinition = await conn.QueryFirstOrDefaultAsync<dynamic>(existingRecordTimestampDefinitionSeq);

                if (state == "INWORK")
                {
                    //var sql = $"SELECT [idA2A2], [idA3masterReference], [statestate], [updateStampA2] FROM {catalogValue}.WTPart WHERE [statestate] = 'INWORK' and [latestiterationInfo] = 1 and statecheckoutInfo = 'wrk' and idA3E2iterationInfo = {idA2A2}";
                    //var resolvedItems = await conn.QueryFirstOrDefaultAsync<dynamic>(sql);

                    //var currentDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var currentDate = DateTime.Now.ToString("yyyy-MM-dd");


                    //var IdSeq = _plm.Query(catalogValue + ".id_sequence").OrderByDesc("value").FirstOrDefault();

                    // Öncelikle, mevcut kaydý kontrol edin
                    var existingRecord = await conn.QueryFirstOrDefaultAsync(
                    $"SELECT * FROM [{catalogValue}].[StringValue] WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordStringDefinition.idA2A2}",
                    new { KodidA2A2 });
                    var existingRecordTimeStamp = await conn.QueryFirstOrDefaultAsync(
                   $"SELECT * FROM [{catalogValue}].[TimestampValue] WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordTimestampDefinition.idA2A2}",
                   new { KodidA2A2 });

                    if (existingRecord != null)
                    {
                        // Mevcut bir kayýt varsa, güncelleme iþlemi yapýn
                        int result = await conn.ExecuteAsync(
                            $"UPDATE [{catalogValue}].[StringValue] " +
                            "SET " +
                            "[hierarchyIDA6] = @hierarchyIDA6, " +
                            "[idA2A2] = @idA2A2, " +
                            "[classnameA2A2] = @classnameA2A2, " +
                            "[idA3A5] = @idA3A5, " +
                            "[idA3A6] = @idA3A6, " +
                            "[markForDeleteA2] = @markForDeleteA2, " +
                            "[modifyStampA2] = @modifyStampA2, " +
                            "[updateCountA2] = @updateCountA2, " +
                            "[updateStampA2] = @updateStampA2, " +
                            "[createStampA2] = @createStampA2, " +
                            "[classnamekeyA4] = @classnamekeyA4, " +
                            "[classnamekeyA6] = @classnamekeyA6, " +
                            "[value] = @value, " +
                            "[value2] = @value2 " +
                            $"WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordStringDefinition.idA2A2}",
                            new
                            {
                                hierarchyIDA6 = "7058085483721066086",
                                idA2A2 = respIdSeq,
                                classnameA2A2 = "wt.iba.value.StringValue",
                                idA3A5 = 0,
                                idA3A6 = Convert.ToInt64(existingRecordStringDefinition.idA2A2),
                                markForDeleteA2 = 0,
                                modifyStampA2 = DateTime.Now.Date,
                                updateCountA2 = 1,
                                updateStampA2 = DateTime.Now.Date,
                                createStampA2 = DateTime.Now.Date,
                                classnamekeyA4 = "wt.part.WTPart",
                                classnamekeyA6 = "wt.iba.definition.StringDefinition",
                                value = "",
                                value2 = "",
                                KodidA2A2
                            });

                        // Güncelleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin


                        // Güncelleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }
                    else
                    {
                        // Mevcut bir kayýt yoksa, yeni bir kayýt ekleyin
                        int result = await conn.ExecuteAsync(
                            $"INSERT INTO [{catalogValue}].[StringValue] " +
                            "([hierarchyIDA6], [idA2A2], [idA3A4], [classnameA2A2], [idA3A5], [idA3A6], [markForDeleteA2], [modifyStampA2], [updateCountA2], [updateStampA2], [createStampA2], [classnamekeyA4], [classnamekeyA6], [value], [value2]) " +
                            "VALUES (@hierarchyIDA6, @idA2A2, @idA3A4, @classnameA2A2, @idA3A5, @idA3A6, @markForDeleteA2, @modifyStampA2, @updateCountA2, @updateStampA2, @createStampA2, @classnamekeyA4, @classnamekeyA6, @value, @value2)",
                            new
                            {
                                hierarchyIDA6 = "7058085483721066086",
                                idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
                                idA3A4 = Convert.ToInt64(KodidA2A2),
                                classnameA2A2 = "wt.iba.value.StringValue",
                                idA3A5 = 0,
                                idA3A6 = Convert.ToInt64(existingRecordStringDefinition.idA2A2),
                                markForDeleteA2 = 0,
                                modifyStampA2 = DateTime.Now.Date,
                                updateCountA2 = 1,
                                updateStampA2 = DateTime.Now.Date,
                                createStampA2 = DateTime.Now.Date,
                                classnamekeyA4 = "wt.part.WTPart",
                                classnamekeyA6 = "wt.iba.definition.StringDefinition",
                                value = "",
                                value2 = ""
                            });


                        // Yeni ekleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }

                    var controlDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                    DateTime controlDate2 = Convert.ToDateTime(controlDate);
                    if (existingRecordTimeStamp != null)
                    {


                        // TimestampValue tablosunu güncelleyin
                        int result = await conn.ExecuteAsync(
                            $"UPDATE [{catalogValue}].[TimestampValue] " +
                            "SET " +
                            "[hierarchyIDA6] = @hierarchyIDA6, " +
                            "[idA2A2] = @idA2A2, " +
                            "[classnameA2A2] = @classnameA2A2, " +
                            "[idA3A5] = @idA3A5, " +
                            "[idA3A6] = @idA3A6, " +
                            "[markForDeleteA2] = @markForDeleteA2, " +
                            "[modifyStampA2] = @modifyStampA2, " +
                            "[updateCountA2] = @updateCountA2, " +
                            "[updateStampA2] = @updateStampA2, " +
                            "[createStampA2] = @createStampA2, " +
                            "[classnamekeyA4] = @classnamekeyA4, " +
                            "[classnamekeyA6] = @classnamekeyA6, " +
                            "[value] = @value " +
                            $"WHERE [idA3A4] = @KodidA2A2 AND [idA3A6] = {existingRecordTimestampDefinition.idA2A2}",
                            new
                            {
                                hierarchyIDA6 = "-148878178526147486",
                                idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
                                classnameA2A2 = "wt.iba.value.TimestampValue",
                                idA3A5 = 0,
                                idA3A6 = Convert.ToInt64(existingRecordTimestampDefinition.idA2A2),
                                markForDeleteA2 = 0,
                                modifyStampA2 = DateTime.Now.Date,
                                updateCountA2 = 1,
                                updateStampA2 = DateTime.Now.Date,
                                createStampA2 = DateTime.Now.Date,
                                classnamekeyA4 = "wt.part.WTPart",
                                classnamekeyA6 = "wt.iba.definition.TimestampDefinition",
                                value = "",
                                KodidA2A2
                            });

                        // Güncelleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }
                    else
                    {


                        int result = await conn.ExecuteAsync(
    $"INSERT INTO [{catalogValue}].[TimestampValue] " +
    "([hierarchyIDA6], [idA2A2], [idA3A4], [classnameA2A2], [idA3A5], [idA3A6], [markForDeleteA2], [modifyStampA2], [updateCountA2], [updateStampA2], [createStampA2], [classnamekeyA4], [classnamekeyA6], [value]) " +
    "VALUES (@hierarchyIDA6, @idA2A2, @idA3A4, @classnameA2A2, @idA3A5, @idA3A6, @markForDeleteA2, @modifyStampA2, @updateCountA2, @updateStampA2, @createStampA2, @classnamekeyA4, @classnamekeyA6, @value)",
    new
    {
        hierarchyIDA6 = "-148878178526147486",
        idA2A2 = Convert.ToInt64(resolvedIdSeq.value) + 100,
        idA3A4 = Convert.ToInt64(KodidA2A2),
        classnameA2A2 = "wt.iba.value.TimestampValue",
        idA3A5 = 0,
        idA3A6 = Convert.ToInt64(existingRecordTimestampDefinition.idA2A2),
        markForDeleteA2 = 0,
        modifyStampA2 = DateTime.Now.Date,
        updateCountA2 = 1,
        updateStampA2 = DateTime.Now.Date,
        createStampA2 = DateTime.Now.Date,
        classnamekeyA4 = "wt.part.WTPart",
        classnamekeyA6 = "wt.iba.definition.TimestampDefinition",
        value = "",

    });


                        // Yeni ekleme iþlemi baþarýlýysa, id_sequence tablosuna dummy = 'x' þeklinde ekleme iþlemi gerçekleþtirin
                        if (result == 1)
                        {
                            await conn.ExecuteAsync(
                                $"INSERT INTO [{catalogValue}].[id_sequence] (dummy) VALUES ('x')");
                        }
                    }




                    await conn.ExecuteAsync(
         $"UPDATE [{catalogValue}].[Change_Notice_LogTable] SET  [ReviseDate] = @ReviseDate WHERE [idA2A2] = @idA2A2",
                    new { idA2A2 = KodidA2A2, ReviseDate = ReviseDate });

                    //var content = $"{{\r\n  \"EntegrasyonDurumu\": \"Parça devam ediyor\",\r\n  \"EntegrasyonTarihi\": \"{currentDate}\"\r\n}}";
                    //await windchillApiService.EntegrasyonDurumUpdateAPI(ServerName, "ProdMgmt/Parts('OR:wt.part.WTPart:" + resolvedItems.idA2A2 + "')", BasicUsername, BasicPassword, apiToken.NonceValue, "Parça devam ediyor",currentDate);
                }

                //await windchillApiService.EntegrasyonDurumUpdateAPI(ServerName, "ProdMgmt/Parts('OR:wt.part.WTPart:" + resolvedItems.idA2A2 + "')/PTC.ProdMgmt.CheckOut", BasicUsername, BasicPassword, apiToken.NonceValue, "{\r\n  \"EntegrasyonDurumu\": \"Entegre oldu1\"\r\n}");



            }
            catch (Exception)
            {
            }
        }


        #endregion


        #region ProcessInworkAsync-Insert and Update Function
        private async Task InsertLogProcessInworkAsync(Part response, string catalogValue, SqlConnection conn)
        {
            try
            {
                ApiService _apiService = new ApiService();
                var jsonData3 = JsonConvert.SerializeObject(response);
                await conn.ExecuteAsync(
                $"INSERT INTO [{catalogValue}].[Change_Notice_LogTable] ([TransferID],[idA2A2], [ProcessTimestamp], [updateStampA2],[statestate], [name], [WTPartNumber],[Version],[VersionID]) VALUES (@TransferID,@idA2A2, @ProcessTimestamp, @updateStampA2,@statestate, @name, @WTPartNumber,@Version,@VersionID )",
                new { TransferID = response.TransferID, idA2A2 = response.ID.Split(':')[2], ProcessTimestamp = DateTime.UtcNow, updateStampA2 = response.LastModified, statestate = response.State.Value, name = response.Name, WTPartNumber = response.Number, Version = response.Version, VersionID = response.VersionID });
                LogService logService = new LogService(_configuration);
                if (response.State.Value == "INWORK")
                {
                    logService.CreateJsonFileLog(jsonData3, "Parçaya devam ediliyor.");
                }

            }
            catch (Exception)
            {

            }


        }

        private async Task UpdateLogProcessInworkAsync(Part response, string catalogValue, SqlConnection conn)
        {
            try
            {


                ApiService _apiService = new ApiService();
                var jsonData3 = JsonConvert.SerializeObject(response);


                await conn.ExecuteAsync(
                    $"UPDATE [{catalogValue}].[Change_Notice_LogTable] SET [TransferID] = @TransferID, [ProcessTimestamp] = @ProcessTimestamp, [updateStampA2] = @updateStampA2, [statestate] = @statestate,[name] = @name , [WTPartNumber] = @WTPartNumber, [Version] = @Version, [VersionID] = @VersionID WHERE [idA2A2] = @idA2A2",
                    new { TransferID = response.TransferID, idA2A2 = response.ID.Split(':')[2], ProcessTimestamp = DateTime.UtcNow, updateStampA2 = response.LastModified, statestate = response.State.Value, name = response.Name, WTPartNumber = response.Number, Version = response.Version, VersionID = response.VersionID });

                LogService logService = new LogService(_configuration);
                if (response.State.Value == "INWORK")
                {
                    logService.CreateJsonFileLog(jsonData3, "Parçaya devam ediliyor.");
                }


            }
            catch (Exception)
            {

            }

        }

        #endregion


        #region PDF Download Settings


        private async Task SendPdfToCustomerFunctionAsync(string pdfUrl, string pdfFileName, string apiFullUrl, string apiURL, string endPoint, long EPMDocID, string catalogValue, SqlConnection conn, RootObject CADResponse, string stateType,string partCode)
        {
            try
            {
                if (stateType == "CADSTOK")
                {
                    var CADViewResponse = new TeknikResimViewModel
                    {
                        Number = CADResponse.Number,
                        Revizyon = CADResponse.Revision,
                        DocumentType = "TR",
                        Description = CADResponse.Description,
                        ModifiedOn = CADResponse.LastModified,
                        AuthorizationDate = "",
                        ModifiedBy = CADResponse.ModifiedBy,
                        State = CADResponse.State,
                    };

                    CADViewResponse.State.Value = "50";

                    ApiService _apiService = new ApiService();



                    //var jsonData3 = JsonConvert.SerializeObject(anaPart);
                    var LogJsonData = JsonConvert.SerializeObject(CADViewResponse);
                    await _apiService.PostDataAsync(apiFullUrl, apiURL, endPoint, LogJsonData, LogJsonData);

                }
                if (stateType == "CAD_WTPART_ILISKI")
                {

                }
                if (stateType == "SEND_FILE")
                {

                    var partName = "";
                    var partNumber = "";

                    if (partCode != null || partCode != "")
                    {

                    var SQL_WTPart = $"SELECT [idA3masterReference] FROM {catalogValue}.WTPart WHERE [idA2A2] = '{partCode}'";
                    var resolvedItems_SQL_WTPart = await conn.QuerySingleAsync<dynamic>(SQL_WTPart);
                    var SQL_WTPartMaster = $"SELECT [name],[WTPartNumber] FROM {catalogValue}.WTPartMaster WHERE [idA2A2] = '{resolvedItems_SQL_WTPart.idA3masterReference}'";
                    var resolvedItems_SQL_WTPartMaster = await conn.QuerySingleAsync<dynamic>(SQL_WTPartMaster);


                        partName = resolvedItems_SQL_WTPartMaster.name;
                        partNumber = resolvedItems_SQL_WTPartMaster.WTPartNumber;
                    }




                    //if (resolvedItems_SQL_EPMBuildHistory != null || resolvedItems_SQL_EPMBuildHistory != "")
                    //{
                    //    partName = resolvedItems_SQL_WTPartMaster.name;
                    //    partNumber = resolvedItems_SQL_WTPartMaster.WTPartNumber;
                    //}
                    //else
                    //{
                    //    partName = "Name deðer bulunamadý";
                    //    partNumber = "Number deðeri bulunamadý";
                    //}

                    var CADViewResponseContentInfo = new TeknikResimViewModel
                    {
                        Number = CADResponse.Number,
                        Revizyon = CADResponse.Revision,
                        DocumentType = "TR",
                        Description = CADResponse.Description,
                        ModifiedOn = CADResponse.LastModified,
                        AuthorizationDate = "",
                        ModifiedBy = CADResponse.ModifiedBy,
                        State = CADResponse.State,
                        cADContent = new CADContent
                        {
                            //FileData = await DownloadPdfAsync(pdfUrl),
                            FileData = await DownloadPdfAsync(pdfUrl),
                            Name = pdfFileName
                        },
                        dAD_WTPART_Iliþki = new CAD_WTPART_Iliþki
                        {

                            RelatedPartName = partName,
                            RelatedPartNumber = partNumber
                        }

                    };
                    CADViewResponseContentInfo.State.Value = "50";

                    // PDF dosyasýný indir
                    //byte[] pdfBytes = await DownloadPdfAsync(pdfUrl);

                    //  Api Endpoint
                    //string customerApiEndpoint = apiFullUrl + "/" + endPoint;
                    //string customerApiEndpoint = "http://localhost:7217/api/Designtech/SENDFILE";

                    // PDF dosyasýný müþteri API'sine gönder

                    ApiService _apiService = new ApiService();



                    //var jsonData3 = JsonConvert.SerializeObject(anaPart);
                    var LogJsonData = JsonConvert.SerializeObject(CADViewResponseContentInfo);
                    await _apiService.PostDataAsync(apiFullUrl, apiURL, endPoint, LogJsonData, LogJsonData);
                    LogService logService = new LogService(_configuration);

                    logService.CreateJsonFileLog(LogJsonData, "CAD Döküman bilgileri gönderildi.");
                    //await SendPdfToCustomerApiAsync(pdfBytes, pdfFileName, customerApiEndpoint, CADViewResponseContentInfo);
                }



                try
                {


                    await conn.ExecuteAsync($@"
        DELETE FROM [{catalogValue}].[Ent_EPMDocState]
        WHERE EPMDocID = @Ids", new { Ids = EPMDocID });

                }
                catch (Exception ex)
                {
                    // Hata mesajýný veya hata günlüðünü kaydedin
                    //Console.WriteLine($"Hata: {ex.Message}");
                    var CADViewResponseContentInfo = new TeknikResimViewModel
                    {
                        Number = CADResponse.Number,
                        Revizyon = CADResponse.Revision,
                        DocumentType = "TR",
                        Description = CADResponse.Description,
                        ModifiedOn = CADResponse.LastModified,
                        AuthorizationDate = "",
                        ModifiedBy = CADResponse.ModifiedBy,
                        State = CADResponse.State,
                        cADContent = new CADContent
                        {
                            FileData = await DownloadPdfAsync(pdfUrl),
                            Name = pdfFileName
                        }

                    };
                    CADViewResponseContentInfo.State.Value = "50";
                    var LogJsonData = JsonConvert.SerializeObject(CADViewResponseContentInfo);
                    LogService logService = new LogService(_configuration);

                    logService.CreateJsonFileLog(LogJsonData, "CAD Döküman bilgileri gönderildi.");
                }
                finally
                {
                    // Baðlantýyý kapatýn
                    conn.Close();
                }

                //MessageBox.Show($"PDF dosyasý ({pdfFileName}) gönderildi.");
            }
            catch (Exception)
            {
                //MessageBox.Show($"Hata: {ex.Message}");
            }
        }

        private async Task DownloadAndSavePdfAsync(string pdfUrl)
        {
            try
            {
                // HttpClient oluþtur
                using var httpClient = new HttpClient();

                // PDF'yi indir
                using (var response = await httpClient.GetAsync(pdfUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        // Dosya adýný URL'den al
                        var fileName = Path.GetFileName(pdfUrl);

                        // PDF dosyasýný belirtilen dizine kaydet
                        string saveDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "PDF");
                        string savePath = Path.Combine(saveDirectory, fileName);

                        // Klasör yoksa oluþtur
                        if (!Directory.Exists(saveDirectory))
                        {
                            Directory.CreateDirectory(saveDirectory);
                        }

                        using (var fileStream = new FileStream(savePath, FileMode.Create))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }
                    }
                    else
                    {
                        // Hata durumunu ele al
                        throw new Exception($"PDF indirme baþarýsýz. StatusCode: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunu ele al 
                throw new Exception($"PDF indirme hatasý: {ex.Message}");
            }
        }


        private async Task<byte[]> DownloadPdfAsync(string pdfUrl)
        {
            try
            {

                string directoryPath = "Configuration";
                string fileName2 = "appsettings.json";
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName2);   
                

                //string directoryPath2 = "Configuration";
                //string fileName2 = "ApiSendDataSettings.json";

                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
                {
                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
                }



                // (Önceki kodlar burada)
                string jsonData = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;

                JObject jsonObject = JObject.Parse(jsonData);

                var CSRF_NONCE = jsonObject["APIConnectionINFO"]["CSRF_NONCE"].ToString();
                var BasicUsername = jsonObject["APIConnectionINFO"]["Username"].ToString();
                var BasicPassword = jsonObject["APIConnectionINFO"]["Password"].ToString();


                var _httpClient1 = new HttpClient();
                _httpClient1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{BasicUsername}:{BasicPassword}")));

                _httpClient1.DefaultRequestHeaders.Add("CSRF-NONCE", CSRF_NONCE);
                using (var response = await _httpClient1.GetAsync(pdfUrl))
                {
                    //if (response.IsSuccessStatusCode)
                    //{
                    //    var dosyaAdi = Path.GetFileName(new Uri(pdfUrl).LocalPath);

                    //    // PDF dosyasýný belirtilen dizine kaydet
                    //    string saveDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "PDF");
                    //    string savePath = Path.Combine(saveDirectory, dosyaAdi);

                    //    // Klasör yoksa oluþtur
                    //    if (!Directory.Exists(saveDirectory))
                    //    {
                    //        Directory.CreateDirectory(saveDirectory);
                    //    }






                    //    var Ocr = new IronTesseract();
                    //    using (var Input = new OcrInput(savePath)) 
                    //    {

                    //        var Result = Ocr.Read(Input);
                    //        var pageContent = Result.Text;

                    //        // Sayfa numarasýný ayýkla
                    //        Match pageMatch = Regex.Match(pageContent, @"SHEET\s+(\d+)\s+OF\s+(\d+)");
                    //        if (pageMatch.Success)
                    //        {
                    //            int sheetNumber = int.Parse(pageMatch.Groups[1].Value);
                    //            int totalSheets = int.Parse(pageMatch.Groups[2].Value);

                    //            Console.WriteLine($"Sheet {sheetNumber} of {totalSheets}");
                    //        }
                    //    }



                    //    return await response.Content.ReadAsByteArrayAsync();
                    //}


                    if (response.IsSuccessStatusCode)
                    {
                        var dosyaAdi = Path.GetFileName(new Uri(pdfUrl).LocalPath);

                        // PDF dosyasýný belirtilen dizine kaydet
                        string saveDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration", "PDF");
                        string savePath = Path.Combine(saveDirectory, dosyaAdi);

                        // Klasör yoksa oluþtur
                        if (!Directory.Exists(saveDirectory))
                        {
                            Directory.CreateDirectory(saveDirectory);
                        }

                        // PDF dosyasýný kaydet
                        await using (var fileStream = new FileStream(savePath, FileMode.Create, FileAccess.Write))
                        {
                            await response.Content.CopyToAsync(fileStream);
                        }


                        byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                        var stream = new MemoryStream(bytes);
                        //PdfiumViewer.PdfDocument pdfDocument = PdfiumViewer.PdfDocument.Load(stream);
                        // PDF dosyasýný oku
                        using (var pdfDocument = PdfiumViewer.PdfDocument.Load(stream))
                        {
                            // Tüm sayfalar için OCR iþlemi yap
                            for (int i = 0; i < pdfDocument.PageCount; i++)
                            {
                                using (var page = pdfDocument.Render(i, 300, 300, true))
                                {
                                    using (var bitmap = new Bitmap(page))
                                    {
                                        // OCR iþlemi
                                        using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
                                        {
                                            using (var img = BitmapToPixConverter.Convert(bitmap))
                                            {
                                                using (var tesseractPage = engine.Process(img))
                                                {
                                                    var text = tesseractPage.GetText();

                                                    // Sayfa numarasýný ayýkla

                                                    Match pageMatch = Regex.Match(text, @"SHEET\s+([0-9]+)\s+OF\s+([0-9]+)", RegexOptions.IgnoreCase);
                                                    if (pageMatch.Success)
                                                    {
                                                        int sheetNumber = int.Parse(pageMatch.Groups[1].Value);
                                                        int totalSheets = int.Parse(pageMatch.Groups[2].Value);

                                                        Console.WriteLine($"Sheet {sheetNumber} of {totalSheets}");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        return await response.Content.ReadAsByteArrayAsync();
                    }
                    else
                    {
                        // Hata durumunu ele al
                        throw new Exception($"PDF indirme baþarýsýz. StatusCode: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunu ele al 
                throw new Exception($"PDF indirme hatasý: {ex.Message}");
            }
        }


    

    public static class BitmapToPixConverter
    {
        public static Pix Convert(Bitmap bitmap)
        {
            var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".bmp");
            bitmap.Save(tempFile);
            var pix = Pix.LoadFromFile(tempFile);
            File.Delete(tempFile);
            return pix;
        }
    }

    private async Task SendPdfToCustomerApiAsync(byte[] pdfBytes, string pdfFileName, string customerApiEndpoint, CADContent CADViewResponseContentInfo)
        {
            try
            {


                using (var httpClient = new HttpClient())
                {
                    using (var content = new MultipartFormDataContent())
                    {
                        content.Add(new ByteArrayContent(pdfBytes), "file", pdfFileName);

                        var response = await httpClient.PostAsync(customerApiEndpoint, content);


                    }
                }
            }
            catch (Exception)
            {

            }
        }

        #endregion







        private async Task InsertLogAndPostDataAsync(Part response, string catalogValue, SqlConnection conn, string apiFullUrl, string apiURL, string apiEndpoint, string state, long partItemIdA2A2)
        {
            try
            {
                var anaPart = new AnaPart();
                var anaPartCancelled = new AnaPartCancelled();
                var jsonData3 = "";
                if (response.CLASSIFICATION == null)
                {
                    response.CLASSIFICATION = new CLASSIFICATION
                    {
                        ClfNodeHierarchyDisplayName = "NULL"
                    };
                }


                if (response.State.Value == "RELEASED")
                {
                    response.State.Value = "A";
                    response.State.Display = "Aktif";
                    anaPart = new AnaPart
                    {
                        Number = response.Number,
                        Name = response.Name,
                        Fai = "H",
                        MuhasebeKodu = "0000000",
                        PlanlamaTipiKodu = "P",
                        PLM = "E",
                        State = response.State,
                        TransferID = response.TransferID,
                        Description = response.Description,
                        BirimKodu = response.BirimKodu,
                        CLASSIFICATION = response.CLASSIFICATION
                    };
                    jsonData3 = JsonConvert.SerializeObject(anaPart);
                }
                else if (response.State.Value == "CANCELLED")
                {
                    response.State.Value = "P";
                    response.State.Display = "Pasif";
                    anaPartCancelled = new AnaPartCancelled
                    {
                        Number = response.Number,
                        State = response.State,

                    };
                    jsonData3 = JsonConvert.SerializeObject(anaPartCancelled);
                }
                else if (response.State.Value == "INWORK")
                {
                    response.State.Value = "";
                    response.State.Display = "";
                    anaPart = new AnaPart
                    {
                        Number = response.Number,
                        Name = response.Name,
                        Fai = "H",
                        MuhasebeKodu = "0000000",
                        PlanlamaTipiKodu = "P",
                        PLM = "E",
                        State = response.State,
                        TransferID = response.TransferID,
                        Description = response.Description,
                        BirimKodu = response.BirimKodu,
                        CLASSIFICATION = response.CLASSIFICATION
                    };
                    jsonData3 = JsonConvert.SerializeObject(anaPart);
                }


                ApiService _apiService = new ApiService();



                //var jsonData3 = JsonConvert.SerializeObject(anaPart);
                var LogJsonData = JsonConvert.SerializeObject(response);
                await _apiService.PostDataAsync(apiFullUrl, apiURL, apiEndpoint, jsonData3, LogJsonData);



                if (response.EntegrasyonDurumu is null or not "Parça entegre oldu" && state == "RELEASED")
                {
                    await EntegrasyonDurumUpdate(state, partItemIdA2A2);
                    //await EntegrasyonDurumCheckOut(partItem.idA2A2, state);

                }

                if (response.EntegrasyonDurumu is null or not "Parça iptal oldu" && state == "CANCELLED")
                {
                    await EntegrasyonDurumUpdate(state, partItemIdA2A2);
                    //await EntegrasyonDurumCheckOut(partItem.idA2A2, state);
                }

              


                if (response.State.Value == "A")
                {
                    response.State.Value = "RELEASED";
                    response.State.Display = "Released";
                }
                if (response.State.Value == "P")
                {
                    response.State.Value = "CANCELLED";
                    response.State.Display = "Cancelled";
                }

                await conn.ExecuteAsync(
                    $"INSERT INTO [{catalogValue}].[Change_Notice_LogTable] ([TransferID],[idA2A2], [ProcessTimestamp], [updateStampA2],[statestate], [name], [WTPartNumber],[Version],[VersionID]) VALUES (@TransferID,@idA2A2, @ProcessTimestamp, @updateStampA2,@statestate, @name, @WTPartNumber,@Version,@VersionID )",
                    new { TransferID = response.TransferID, idA2A2 = response.ID.Split(':')[2], ProcessTimestamp = DateTime.UtcNow, updateStampA2 = response.LastModified, statestate = response.State.Value, name = response.Name, WTPartNumber = response.Number, Version = response.Version, VersionID = response.VersionID });

                LogService logService = new LogService(_configuration);
                if (response.State.Value == "RELEASED")
                {
                    logService.CreateJsonFileLog(LogJsonData, "Parça gönderildi.");

                }
                else if (response.State.Value == "CANCELLED")
                {
                    response.State.Value = "P";
                    response.State.Display = "Pasif";
                    var anaPartCancelledLOG = new AnaPartCancelledLOG
                    {
                        Name = response.Name,
                        Number = response.Number,
                        State = response.State,

                    };
                    var jsonData4 = JsonConvert.SerializeObject(anaPartCancelledLOG);
                    logService.CreateJsonFileLog(LogJsonData, "Parça iptal edildi.");

                }
                else
                {
                    logService.CreateJsonFileLog(LogJsonData, "Parçaya devam ediliyor.");

                }





            }
            catch (Exception)
            {
                if (response.EntegrasyonDurumu is null or not "Parça entegre edilemedi")
                {
                    await EntegrasyonHataDurumUpdate(partItemIdA2A2);
                    //await EntegrasyonDurumCheckOut(partItem.idA2A2, state);
                }
            }


        }

        private async Task UpdateLogAndPostDataAsync(Part response, string catalogValue, SqlConnection conn, string apiFullUrl, string apiURL, string apiEndpoint, string state, long partItemIdA2A2)
        {
            try
            {
                var anaPart = new AnaPart();
                var anaPartLOG = new AnaPart();
                var anaPartCancelled = new AnaPartCancelled();
                var jsonData3 = "";
                 //string faiValue = string.IsNullOrEmpty(response.Fai) ? "H" : response.Fai;

                if(response.CLASSIFICATION == null)
                {
                    response.CLASSIFICATION = new CLASSIFICATION
                    {
                        ClfNodeHierarchyDisplayName = "NULL"
                    };
                }
              

                if (response.State.Value == "RELEASED")
                {
                    response.State.Value = "A";
                    response.State.Display = "Aktif";
                    anaPart = new AnaPart
                    {
                        Number = response.Number,
                        Name = response.Name,
                        Fai = "H",
                        MuhasebeKodu = "0000000",
                        PlanlamaTipiKodu = "P",
                        PLM = "E",
                        State = response.State,
                        TransferID = response.TransferID,
                        Description = response.Description,
                        BirimKodu = response.BirimKodu,
                        CLASSIFICATION = response.CLASSIFICATION,
                    };
                    jsonData3 = JsonConvert.SerializeObject(anaPart);
                }
                else if (response.State.Value == "CANCELLED")
                {
                    response.State.Value = "P";
                    response.State.Display = "Pasif";
                    anaPartCancelled = new AnaPartCancelled
                    {
                        Number = response.Number,
                        State = response.State,

                    };
                    jsonData3 = JsonConvert.SerializeObject(anaPartCancelled);
                }
                else if (response.State.Value == "INWORK")
                {
                    response.State.Value = "";
                    response.State.Display = "";
                    anaPart = new AnaPart
                    {
                        Number = response.Number,
                        Name = response.Name,
                        Fai = "H",
                        MuhasebeKodu = "0000000",
                        PlanlamaTipiKodu = "P",
                        PLM = "E",
                        State = response.State,
                        TransferID = response.TransferID,
                        Description = response.Description,
                        BirimKodu = response.BirimKodu,
                        CLASSIFICATION = response.CLASSIFICATION
                    };
                    jsonData3 = JsonConvert.SerializeObject(anaPart);
                }

                ApiService _apiService = new ApiService();

                //if (response.State.Value == "RELEASED")
                //{
                //    response.State.Value = "A";
                //    response.State.Display = "Aktif";
                //}
                //if (response.State.Value == "CANCELLED")
                //{
                //    response.State.Value = "P";
                //    response.State.Display = "Pasif";
                //}
                //var jsonData3 = JsonConvert.SerializeObject(anaPart);
                var LogJsonData = JsonConvert.SerializeObject(response);
                await _apiService.PostDataAsync(apiFullUrl, apiURL, apiEndpoint, jsonData3, LogJsonData);


                if (response.EntegrasyonDurumu is null or not "Parça entegre oldu" && state == "RELEASED")
                {
                    await EntegrasyonDurumUpdate(state, partItemIdA2A2);

                }

                if (response.EntegrasyonDurumu is null or not "Parça iptal oldu" && state == "CANCELLED")
                {
                    await EntegrasyonDurumUpdate(state, partItemIdA2A2);
                }

                #region released parts

                #endregion

                if (response.State.Value == "A")
                {
                    response.State.Value = "RELEASED";
                    response.State.Display = "Released";
                }
                if (response.State.Value == "P")
                {
                    response.State.Value = "CANCELLED";
                    response.State.Display = "Cancelled";
                }

                await conn.ExecuteAsync(
                    $"UPDATE [{catalogValue}].[Change_Notice_LogTable] SET [TransferID] = @TransferID, [ProcessTimestamp] = @ProcessTimestamp, [updateStampA2] = @updateStampA2, [statestate] = @statestate,[name] = @name , [WTPartNumber] = @WTPartNumber, [Version] = @Version, [VersionID] = @VersionID WHERE [idA2A2] = @idA2A2",
                    new { TransferID = response.TransferID, idA2A2 = response.ID.Split(':')[2], ProcessTimestamp = DateTime.UtcNow, updateStampA2 = response.LastModified, statestate = response.State.Value, name = response.Name, WTPartNumber = response.Number, Version = response.Version, VersionID = response.VersionID });




                var jsonDataLog = JsonConvert.SerializeObject(anaPart);


                LogService logService = new LogService(_configuration);
                if (response.State.Value == "RELEASED")
                {
                    logService.CreateJsonFileLog(LogJsonData, "Parça gönderildi.");

                }
                else if (response.State.Value == "CANCELLED")
                {
                    response.State.Value = "P";
                    response.State.Display = "Pasif";
                    var anaPartCancelledLOG = new AnaPartCancelledLOG
                    {
                        Name = response.Name,
                        Number = response.Number,
                        State = response.State,

                    };
                    var jsonData4 = JsonConvert.SerializeObject(anaPartCancelledLOG);
                    logService.CreateJsonFileLog(LogJsonData, "Parça iptal edildi.");
                }
                else
                {
                    logService.CreateJsonFileLog(LogJsonData, "Parçaya devam ediliyor.");

                }




            }
            catch (Exception)
            {
                if (response.EntegrasyonDurumu is null or not "Parça entegre edilemedi")
                {
                    await EntegrasyonHataDurumUpdate(partItemIdA2A2);
                }
            }

        }

        #region Alternates Functions

        private async Task AlternatesInsertLogAndPostDataAsync(Part response, Alternates item, string catalogValue, SqlConnection conn, string apiFullUrl, string apiURL, string apiEndpoint)
        {
            try
            {

                var muadilPart = new MuadilPart
                {
                    Number = response.Number,
                    Alternates = response.Alternates.Select(alternate => new Alternates2
                    {
                        AlternatePart = new AlternatePart2
                        {
                            TransferID = alternate.AlternatePart.TransferID,
                            Number = alternate.AlternatePart.Number,
                        }
                    }).ToList()
                };


                ApiService _apiService = new ApiService();

                if (response.State.Value == "RELEASED")
                {
                    response.State.Value = "A";
                    response.State.Display = "Aktif";
                }
                if (response.State.Value == "CANCELLED")
                {
                    response.State.Value = "P";
                    response.State.Display = "Pasif";
                }
                var jsonData3 = JsonConvert.SerializeObject(response);
                var jsonData4 = JsonConvert.SerializeObject(muadilPart);
                var LogJsonData = JsonConvert.SerializeObject(response);
                await _apiService.PostDataAsync(apiFullUrl, apiURL, apiEndpoint, jsonData4, LogJsonData);

                if (response.State.Value == "A")
                {
                    response.State.Value = "RELEASED";
                    response.State.Display = "Released";
                }
                if (response.State.Value == "P")
                {
                    response.State.Value = "CANCELLED";
                    response.State.Display = "Cancelled";
                }

                await conn.ExecuteAsync(
$"INSERT INTO [{catalogValue}].[WTPartAlternateLink_LOG] ([AnaParcaTransferID],[AnaParcaID],[AnaParcaNumber],[AnaParcaName],[TransferID],[ID],[ObjectType],[Name], [Number],[updateStampA2], [modifyStampA2], [ProcessTimestamp], [state]) VALUES (@AnaParcaTransferID,@AnaParcaID,@AnaParcaNumber,@AnaParcaName,@TransferID,@ID,@ObjectType,@Name, @Number,@modifyStampA2, @modifyStampA2, @ProcessTimestamp,@state)",
new { AnaParcaTransferID = response.TransferID, AnaParcaID = response.ID, AnaParcaNumber = response.Number, AnaParcaName = response.Name, TransferID = item.AlternatePart.TransferID, ID = item.ID.Split(':')[2], ObjectType = item.ObjectType, Name = item.AlternatePart.Name, Number = item.AlternatePart.Number, updateStampA2 = item.LastModified, modifyStampA2 = item.LastModified, ProcessTimestamp = DateTime.UtcNow, state = item.AlternatePart.State });




                LogService logService = new LogService(_configuration);
                logService.CreateJsonFileLog(jsonData3);

            }
            catch (Exception)
            {

            }


        }

        private async Task AlternatesUpdateLogAndPostDataAsync(Part response, Alternates item, string catalogValue, SqlConnection conn, string apiFullUrl, string apiURL, string apiEndpoint)
        {
            try
            {

                var muadilPart = new MuadilPart
                {
                    Number = response.Number,
                    Alternates = response.Alternates.Select(alternate => new Alternates2
                    {
                        AlternatePart = new AlternatePart2
                        {
                            TransferID = alternate.AlternatePart.TransferID,
                            Number = alternate.AlternatePart.Number,
                        }
                    }).ToList()
                };


                ApiService _apiService = new ApiService();

                if (response.State.Value == "RELEASED")
                {
                    response.State.Value = "A";
                    response.State.Display = "Aktif";
                }
                if (response.State.Value == "CANCELLED")
                {
                    response.State.Value = "P";
                    response.State.Display = "Pasif";
                }
                var jsonData3 = JsonConvert.SerializeObject(response);
                var jsonData4 = JsonConvert.SerializeObject(muadilPart);
                var LogJsonData = JsonConvert.SerializeObject(response);
                await _apiService.PostDataAsync(apiFullUrl, apiURL, apiEndpoint, jsonData4, LogJsonData);
                if (response.State.Value == "A")
                {
                    response.State.Value = "RELEASED";
                    response.State.Display = "Released";
                }
                if (response.State.Value == "P")
                {
                    response.State.Value = "CANCELLED";
                    response.State.Display = "Cancelled";
                }

                await conn.ExecuteAsync(
$"UPDATE [{catalogValue}].[WTPartAlternateLink_LOG] SET  [AnaParcaTransferID] = @AnaParcaTransferID,[AnaParcaID] = @AnaParcaID,[AnaParcaNumber] = @AnaParcaNumber,[AnaParcaName] = @AnaParcaName, [TransferID] = @TransferID, [ID] = @ID,[ObjectType] = @ObjectType,[Name] = @Name, [Number] = @Number,[updateStampA2] = @updateStampA2, [modifyStampA2] = @modifyStampA2, [ProcessTimestamp] = @ProcessTimestamp , [state] = @state",
new { AnaParcaTransferID = response.TransferID, AnaParcaID = response.ID, AnaParcaNumber = response.Number, AnaParcaName = response.Name, TransferID = item.AlternatePart.TransferID, ID = item.ID.Split(':')[2], ObjectType = item.ObjectType, Name = item.AlternatePart.Name, Number = item.AlternatePart.Number, updateStampA2 = item.LastModified, modifyStampA2 = item.LastModified, ProcessTimestamp = DateTime.UtcNow, state = item.AlternatePart.State.Value });

                LogService logService = new LogService(_configuration);
                logService.CreateJsonFileLog(jsonData3);

            }
            catch (Exception)
            {

            }

        }

        #region released parts

        private async Task RELEASED_AlternatesInsertLogAndPostDataAsync(Part response, Alternates item, string catalogValue, SqlConnection conn, string apiFullUrl, string apiURL, string apiEndpoint)
        {
            try
            {


                var muadilPart = new MuadilPart
                {
                    Number = response.Number,
                    Alternates = response.Alternates.Select(alternate => new Alternates2
                    {
                        AlternatePart = new AlternatePart2
                        {
                            TransferID = alternate.AlternatePart.TransferID,
                            Number = alternate.AlternatePart.Number,
                            isCancel = false
                        }
                    }).ToList()
                };


                ApiService _apiService = new ApiService();


                response.Alternates = response.Alternates
                    .Where(x => x.AlternatePart.State.Value == "RELEASED")
                    .ToList();


                if (response.Alternates.SingleOrDefault().AlternatePart.State.Value == "RELEASED")
                {
                    response.Alternates.SingleOrDefault().AlternatePart.State.Value = "A";
                    response.Alternates.SingleOrDefault().AlternatePart.State.Display = "Aktif";
                }
                if (response.Alternates.SingleOrDefault().AlternatePart.State.Value == "CANCELLED")
                {
                    response.Alternates.SingleOrDefault().AlternatePart.State.Value = "P";
                    response.Alternates.SingleOrDefault().AlternatePart.State.Display = "Pasif";
                }
                var jsonData = JsonConvert.SerializeObject(response);
                var jsonData2 = JsonConvert.SerializeObject(muadilPart);






                #region KontrolFonskiyon

                var existingRecord = await conn.QueryFirstOrDefaultAsync<WTPartAlternateLink_LOG>(
    $@"
    SELECT *
    FROM [{catalogValue}].[WTPartAlternateLink_LOG]
    WHERE  [AnaParcaNumber] = @AnaParcaNumber
    AND [Number] = @Number",
    new
    {
        AnaParcaNumber = response.Number,
        Number = item.AlternatePart.Number
    });





                //        if (existingRecord != null)
                //        {
                //            // Kayýt güncellemesi
                //            await conn.ExecuteAsync(
                //                $@"
                //UPDATE [{catalogValue}].[WTPartAlternateLink_LOG]
                //SET [updateStampA2] = @updateStampA2,
                //    [modifyStampA2] = @modifyStampA2,
                //    [ProcessTimestamp] = @ProcessTimestamp,
                //    [state] = @state
                //WHERE [AnaParcaTransferID] = @AnaParcaTransferID
                //AND [AnaParcaID] = @AnaParcaID
                //AND [AnaParcaNumber] = @AnaParcaNumber
                //AND [AnaParcaName] = @AnaParcaName
                //AND [TransferID] = @TransferID
                //AND [ID] = @ID
                //AND [ObjectType] = @ObjectType
                //AND [Name] = @Name
                //AND [Number] = @Number",
                //                new
                //                {
                //                    updateStampA2 = item.LastModified,
                //                    modifyStampA2 = item.LastModified,
                //                    ProcessTimestamp = item.LastModified,
                //                    state = item.AlternatePart.State.Value,
                //                    AnaParcaTransferID = response.TransferID,
                //                    AnaParcaID = response.ID,
                //                    AnaParcaNumber = response.Number,
                //                    AnaParcaName = response.Name,
                //                    TransferID = item.AlternatePart.TransferID,
                //                    ID = item.AlternatePart.ID.Split(':')[2],
                //                    ObjectType = item.ObjectType,
                //                    Name = item.AlternatePart.Name,
                //                    Number = item.AlternatePart.Number
                //                });
                //        }
                if (existingRecord == null)
                {

                    var LogJsonData = JsonConvert.SerializeObject(response);
                    await _apiService.PostDataAsync(apiFullUrl, apiURL, apiEndpoint, jsonData2, LogJsonData);

                    if (response.Alternates.SingleOrDefault().AlternatePart.State.Value == "A")
                    {
                        response.Alternates.SingleOrDefault().AlternatePart.State.Value = "RELEASED";
                        response.Alternates.SingleOrDefault().AlternatePart.State.Display = "Released";
                    }
                    if (response.Alternates.SingleOrDefault().AlternatePart.State.Value == "P")
                    {
                        response.Alternates.SingleOrDefault().AlternatePart.State.Value = "CANCELLED";
                        response.Alternates.SingleOrDefault().AlternatePart.State.Display = "Cancelled";
                    }
                    var roundedLastModified = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                    DateTime controlTime = Convert.ToDateTime(roundedLastModified);


                    // Yeni bir kayýt ekleme
                    await conn.ExecuteAsync(
                        $@"
        INSERT INTO [{catalogValue}].[WTPartAlternateLink_LOG] ([AnaParcaTransferID],[AnaParcaID],[AnaParcaNumber],[AnaParcaName],[TransferID],[ID],[ObjectType],[Name], [Number],[updateStampA2], [modifyStampA2], [ProcessTimestamp], [state])
        VALUES (@AnaParcaTransferID,@AnaParcaID,@AnaParcaNumber,@AnaParcaName,@TransferID,@ID,@ObjectType,@Name, @Number,@updateStampA2, @modifyStampA2, @ProcessTimestamp,@state)",
                        new
                        {
                            AnaParcaTransferID = response.TransferID,
                            AnaParcaID = response.ID,
                            AnaParcaNumber = response.Number,
                            AnaParcaName = response.Name,
                            TransferID = item.AlternatePart.TransferID,
                            ID = item.AlternatePart.ID.Split(':')[2],
                            ObjectType = item.ObjectType,
                            Name = item.AlternatePart.Name,
                            Number = item.AlternatePart.Number,
                            updateStampA2 = item.LastModified,
                            modifyStampA2 = item.LastModified,
                            ProcessTimestamp = item.LastModified,
                            state = item.AlternatePart.State.Value
                        });
                    LogService logService = new LogService(_configuration);
                    logService.CreateJsonFileLog(jsonData, $"Ana parça: {response.Number} - Muadil parça: {item.AlternatePart.Number} ile iliþkilendirildi. ");
                }

                #endregion





                //                await conn.ExecuteAsync(
                //$"INSERT INTO [{catalogValue}].[WTPartAlternateLink_LOG] ([AnaParcaTransferID],[AnaParcaID],[AnaParcaNumber],[AnaParcaName],[TransferID],[ID],[ObjectType],[Name], [Number],[updateStampA2], [modifyStampA2], [ProcessTimestamp], [state]) VALUES (@AnaParcaTransferID,@AnaParcaID,@AnaParcaNumber,@AnaParcaName,@TransferID,@ID,@ObjectType,@Name, @Number,@modifyStampA2, @modifyStampA2, @ProcessTimestamp,@state)",
                //new { AnaParcaTransferID = response.TransferID, AnaParcaID = response.ID, AnaParcaNumber = response.Number, AnaParcaName = response.Name, TransferID = item.AlternatePart.TransferID, ID = item.AlternatePart.ID.Split(':')[2], ObjectType = item.ObjectType, Name = item.AlternatePart.Name, Number = item.AlternatePart.Number, updateStampA2 = item.LastModified, modifyStampA2 = item.LastModified, ProcessTimestamp = item.LastModified, state = item.AlternatePart.State.Value });


                //                await conn.ExecuteAsync(
                //$"UPDATE [{catalogValue}].[WTPartAlternateLink] SET [modifyStampA2] = @modifyStampA2 WHERE idA2A2 = {item.ID.Split(':')[2]} ",
                //new { modifyStampA2 = controlTime.AddHours(3) });




                //                await conn.ExecuteAsync(
                //$"UPDATE [{catalogValue}].[WTPart] SET [modifyStampA2] = @modifyStampA2 WHERE idA2A2 = {item.AlternatePart.ID.Split(':')[2]} ",
                //new { modifyStampA2 = controlTime });




            }
            catch (Exception)
            {

            }

        }
        private async Task RELEASED_AlternatesUpdateLogAndPostDataAsync(Part response, Alternates item, string catalogValue, SqlConnection conn, string apiFullUrl, string apiURL, string apiEndpoint)
        {
            try
            {

                var muadilPart = new MuadilPart
                {
                    Number = response.Number,
                    Alternates = response.Alternates.Select(alternate => new Alternates2
                    {
                        AlternatePart = new AlternatePart2
                        {
                            TransferID = alternate.AlternatePart.TransferID,
                            Number = alternate.AlternatePart.Number,
                            isCancel = false
                        }
                    }).ToList()
                };


                ApiService _apiService = new ApiService();
                response.Alternates = response.Alternates
                    .Where(x => x.AlternatePart.State.Value == "RELEASED")
                    .ToList();


                if (response.Alternates.SingleOrDefault().AlternatePart.State.Value == "RELEASED")
                {
                    response.Alternates.SingleOrDefault().AlternatePart.State.Value = "A";
                    response.Alternates.SingleOrDefault().AlternatePart.State.Display = "Aktif";
                }
                if (response.Alternates.SingleOrDefault().AlternatePart.State.Value == "CANCELLED")
                {
                    response.Alternates.SingleOrDefault().AlternatePart.State.Value = "P";
                    response.Alternates.SingleOrDefault().AlternatePart.State.Display = "Pasif";
                }

                var jsonData = JsonConvert.SerializeObject(response);
                var jsonData2 = JsonConvert.SerializeObject(muadilPart);
                var LogJsonData = JsonConvert.SerializeObject(response);
                await _apiService.PostDataAsync(apiFullUrl, apiURL, apiEndpoint, jsonData2, LogJsonData);

                if (response.Alternates.SingleOrDefault().AlternatePart.State.Value == "A")
                {
                    response.Alternates.SingleOrDefault().AlternatePart.State.Value = "RELEASED";
                    response.Alternates.SingleOrDefault().AlternatePart.State.Display = "Released";
                }
                if (response.Alternates.SingleOrDefault().AlternatePart.State.Value == "P")
                {
                    response.Alternates.SingleOrDefault().AlternatePart.State.Value = "CANCELLED";
                    response.Alternates.SingleOrDefault().AlternatePart.State.Display = "Cancelled";
                }


                var roundedLastModified = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fffffff");


                DateTime controlTime = Convert.ToDateTime(roundedLastModified);



                await conn.ExecuteAsync(
$"UPDATE [{catalogValue}].[WTPartAlternateLink_LOG] SET [AnaParcaTransferID] = @AnaParcaTransferID,[AnaParcaID] = @AnaParcaID,[AnaParcaNumber] = @AnaParcaNumber,[AnaParcaName] = @AnaParcaName, [TransferID] = @TransferID, [ID] = @ID,[ObjectType] = @ObjectType,[Name] = @Name, [Number] = @Number,[updateStampA2] = @updateStampA2, [modifyStampA2] = @modifyStampA2, [ProcessTimestamp] = @ProcessTimestamp , [state] = @state WHERE ID = {item.AlternatePart.ID.Split(':')[2]} ",
new
{
    AnaParcaTransferID = response.TransferID,
    AnaParcaID = response.ID,
    AnaParcaNumber = response.Number,
    AnaParcaName = response.Name,
    TransferID = item.AlternatePart.TransferID,
    ID = item.AlternatePart.ID.Split(':')[2],
    ObjectType = item.ObjectType,
    Name = item.AlternatePart.Name,
    Number = item.AlternatePart.Number,
    updateStampA2 = item.LastModified,
    modifyStampA2 = item.LastModified,
    ProcessTimestamp = item.LastModified
,
    state = item.AlternatePart.State.Value
});



                //                await conn.ExecuteAsync(
                //$"UPDATE [{catalogValue}].[WTPartAlternateLink] SET [modifyStampA2] = @modifyStampA2 WHERE idA2A2 = {item.ID.Split(':')[2]} ",
                //new { modifyStampA2 = controlTime.AddHours(3) });

                //                await conn.ExecuteAsync(
                //$"UPDATE [{catalogValue}].[WTPart] SET [modifyStampA2] = @modifyStampA2 WHERE idA2A2 = {item.AlternatePart.ID.Split(':')[2]} ",
                //new { modifyStampA2 = controlTime });

                LogService logService = new LogService(_configuration);
                logService.CreateJsonFileLog(jsonData, $"Ana parça: {response.Number} - Muadil parça: {item.AlternatePart.Number} ile iliþkilendirildi. ");

            }
            catch (Exception)
            {

            }

        }
        #endregion

        #endregion


        #region RemovedParts
        private async Task RemovedLogAndPostDataAsync(Part response, string catalogValue, SqlConnection conn, string apiFullUrl, string apiURL, string apiEndpoint)
        {
            try
            {

                var removePart = new MuadilPart
                {
                    Number = response.Number,
                    Alternates = response.Alternates.Select(alternate => new Alternates2
                    {
                        AlternatePart = new AlternatePart2
                        {
                            TransferID = alternate.AlternatePart.TransferID,
                            Number = alternate.AlternatePart.Number,
                            isCancel = true
                        }
                    }).ToList()
                };
             
                ApiService _apiService = new ApiService();
                var jsonData3 = JsonConvert.SerializeObject(response);
                var jsonData4 = JsonConvert.SerializeObject(removePart);
                var LogJsonData = JsonConvert.SerializeObject(response);
                await _apiService.PostDataAsync(apiFullUrl, apiURL, apiEndpoint, jsonData4, LogJsonData);


                LogService logService = new LogService(_configuration);
                logService.CreateJsonFileLog(jsonData3, $"Ana parça: {response.Number} - Muadil parça: {removePart.Alternates.FirstOrDefault().AlternatePart.Number} muadil iliþkisi kaldýrýldý.");
                

            }
            catch (Exception)
            {
            }


        }
        #endregion




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
                notificatonSettings("Hata! JSON dosyasý oluþturulamadý:" + ex.Message);
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
                                if(displayStringSTATE == " (Aktif)")
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
                                displayString = displayString.Replace(dataObject["State"]["Display"].ToString(), null);
                            }
                            if (dataObject.ContainsKey("Mesaj") && !string.IsNullOrEmpty(dataObject["Mesaj"].ToString()) && dataObject["Mesaj"].ToString().Contains("Muadil parça"))
                            {
                                //displayString += $" - {dataObject["Mesaj"]}";
                                displayString = displayString.Replace(dataObject["Number"].ToString()+" - ", null);
                                displayString = displayString.Replace(dataObject["Name"].ToString(), null);
                                displayString = displayString.Replace(dataObject["Version"].ToString()+" - ", null);
                              
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
    }
}
#endregion