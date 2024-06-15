using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;
using Designtech_PLM_Entegrasyon_AutoPost.Model.WindchillApiModel;

namespace Designtech_PLM_Entegrasyon_AutoPost.Helper
{
    public class LogService
    {
        private readonly IConfiguration _configuration;

        public LogService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        string currentMonthFolder;
        string logFileName;
        public void AddNewLogEntry(string message, string fileName, string operation, string kullaniciAdi)
        {
            try
            {

                //string directoryPath = "ListData";
                //string fileName = "relasedData.json";
                //string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

                currentMonthFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "TakvimFile");
                string dateFormatted = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                logFileName = Path.Combine(currentMonthFolder, dateFormatted + ".json");
                var logObject = new
                {
                    ExcelDosya = fileName,
                    Text = message,
                    Operation = operation,
                    KullaniciAdi = kullaniciAdi,
                    Durum = true,

                    Properties = new { }
                };

                string json = JsonConvert.SerializeObject(logObject);
                Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(new CustomJsonFormatter(), logFileName, shared: true)
                .CreateLogger();



                Log.Information(json);
                Log.CloseAndFlush();
            }
            catch (Exception ex)
            {
                Log.Error("Log oluşturulurken hata oluştu: " + ex.Message);
            }
        }


        public void CreateJsonFileLog(dynamic dataModel, string message = null)
        {
            try
            {
                // Log dosyasının adını ve yolunu oluştur
                currentMonthFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration\\logs", "TakvimFile");
                string dateFormatted = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                string islemTarihi = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                logFileName = Path.Combine(currentMonthFolder, dateFormatted + ".json");

                JArray dataArray;
                if (File.Exists(logFileName))
                {
                    string jsonData = File.ReadAllText(logFileName);
                    dataArray = JArray.Parse(jsonData); // JArray'e dönüştür
                }
                else
                {
                    dataArray = new JArray();
                }

                // Yeni veriyi oluştur
                JObject jsonDataObject = JObject.Parse(dataModel);

                // Mesaj girildiyse ekle
                if (!string.IsNullOrEmpty(message))
                {
                    jsonDataObject.Add("Mesaj", message);
                }

                jsonDataObject.Add("islemTarihi", islemTarihi);

                // Diziye ekle
                dataArray.Add(jsonDataObject);

                // JSON dosyasına yaz
                File.WriteAllText(logFileName, JsonConvert.SerializeObject(dataArray, Formatting.Indented));
            }
            catch (Exception ex)
            {
                // Hata durumunda log oluşturma işlemi
                Log.Error("Log oluşturulurken hata oluştu: " + ex.Message);
            }
        }


        public void CreateJsonFileLogError(dynamic dataModel, string message = null)
        {
            try
            {
                // Log dosyasının adını ve yolunu oluştur
                currentMonthFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration\\logs", "TakvimFile");
                string dateFormatted = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
                string islemTarihi = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                logFileName = Path.Combine(currentMonthFolder, dateFormatted + ".json");
                DataModel responseDataArray = JsonConvert.DeserializeObject<DataModel>(dataModel);

                JArray dataArray;
                if (File.Exists(logFileName))
                {
                    string jsonData = File.ReadAllText(logFileName);
                    dataArray = JArray.Parse(jsonData); // JArray'e dönüştür

                }
                else
                {
                    dataArray = new JArray();
                }

                // Yeni veriyi oluştur
                JObject jsonDataObject = JObject.Parse(dataModel);

                // Mesaj girildiyse ekle
                if (!string.IsNullOrEmpty(message))
                {
                    jsonDataObject.Add("Mesaj", message);
                }

                jsonDataObject.Add("islemTarihi", islemTarihi);

                // Hata mesajı varsa ve daha önce eklenmişse eski logu bul ve kaldır
                if (!string.IsNullOrEmpty(message))
                {
                    var existingErrorLog = dataArray.FirstOrDefault(log => log["Mesaj"] != null && log["Mesaj"].ToString() == message && log["ID"].ToString() == responseDataArray.ID && log["Number"].ToString() == responseDataArray.Number && log["Version"].ToString() == responseDataArray.Version);
                    if (existingErrorLog != null)
                    {
                        dataArray.Remove(existingErrorLog);
                    }


                    //var existingErrorLog = dataArray.FirstOrDefault(log => log["Mesaj"] != null && log["Mesaj"].ToString() == message && log["ID"].ToString() == responseDataArray.ID && log["Number"].ToString() == responseDataArray.Number && log["Version"].ToString() == responseDataArray.Version);
                    //if (existingErrorLog != null)
                    //{
                    //    dataArray.Remove(existingErrorLog);
                    //}
                }

                // Diziye ekle
                dataArray.Add(jsonDataObject);

                // JSON dosyasına yaz
                File.WriteAllText(logFileName, JsonConvert.SerializeObject(dataArray, Formatting.Indented));
                HataBildirimiGonder(dataModel,message);

            }
            catch (Exception ex)
            {
                // Hata durumunda log oluşturma işlemi
                Log.Error("Log oluşturulurken hata oluştu: " + ex.Message);
            }
        }


        //MaillService

        public class DataModel
        {
            public string Number { get; set; }
            public string ID { get; set; }
            public string Version { get; set; }
            public string Message { get; set; }
        }

        private static HashSet<string> gonderilenHataImzalari = new HashSet<string>();
        private static System.Threading.Timer resetTimer;

        static LogService()
        {
            // Timer'ı 2 saatlik aralıklarla tetikleyecek şekilde yapılandır
            resetTimer = new System.Threading.Timer(ResetHashSet, null, TimeSpan.Zero, TimeSpan.FromHours(1));
        }

        private static void ResetHashSet(object state)
        {
            gonderilenHataImzalari = new HashSet<string>();
        }
        public void HataBildirimiGonder(dynamic jsonData, string message)
        {
            string directoryPath = "Configuration";
            string fileName = "EmailController.json";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath, fileName);

            // Klasör yoksa oluştur
            if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath)))
            {
                Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, directoryPath));
            }

            // Dosya varsa oku




            JObject jsonObjectFile;
            if (System.IO.File.Exists(filePath))
            {
                var jsonDataForEmail = File.ReadAllText(filePath);
                jsonObjectFile = JObject.Parse(jsonDataForEmail);
            }
            else
            {
                jsonObjectFile = new JObject
                {
                    ["FromEmail"] = "",
                    ["FromEmailPassword"] = "",
                    ["PortNumber"] = "",
                    ["SmtpClient"] = "",
                    ["EmailClearSettings"] = new JObject
                    {
                        ["ClearHours"] = 1
                    },
                    ["EmailList"] = new JArray()
                };
            }
            JObject jsonObject;
            jsonObject = JObject.Parse(jsonData);
            JArray emailList = (JArray)jsonObjectFile["EmailList"];
            DataModel dataModel = JsonConvert.DeserializeObject<DataModel>(jsonData);
            string hataImzasi = $"{dataModel.Number}_{dataModel.ID}_{dataModel.Version}_{message}";

            if (!gonderilenHataImzalari.Contains(hataImzasi))
            {
                try
                {
                    var portNumber = jsonObjectFile["PortNumber"].ToString();
                    // SMTP ayarlarını yapılandır
                    var smtpClient = new SmtpClient(jsonObjectFile["SmtpClient"].ToString())
                    {
                        Port = Convert.ToInt32(portNumber),
                        Credentials = new NetworkCredential(jsonObjectFile["FromEmail"].ToString(), jsonObjectFile["FromEmailPassword"].ToString()),
                        EnableSsl = true,
                    };

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(jsonObjectFile["FromEmail"].ToString()),
                        Subject = $"Hata Bildirimi - {dataModel.Number} {dataModel.Version}",
                        Body = $@"
                        <h2>Hata Bildirimi</h2>
                        <p><strong>Parça Numarası:</strong> {dataModel.Number}</p>
                        <p><strong>ID:</strong> {dataModel.ID.Split(':')[2]}</p>
                        <p><strong>Versiyon:</strong> {dataModel.Version}</p>
                        <p><strong>Mesaj:</strong> {message}</p>",
                        IsBodyHtml = true,
                    };

                    foreach (var item in emailList)
                    {
                        if (!string.IsNullOrEmpty(item["EMail"].ToString()) &&  Convert.ToBoolean(item["Durum"].ToString()) == true)
                        {
                        mailMessage.To.Add(item["EMail"].ToString());
                        }

                    }
                    smtpClient.Send(mailMessage);

                    gonderilenHataImzalari.Add(hataImzasi);
                    Console.WriteLine($"E-posta gönderildi! Hata: {hataImzasi}");
                }
                catch (Exception)
                {
                    //Log.Error("E-posta gönderimi sırasında hata oluştu: " + ex.Message);
                }
            }
    
        }

        //private void SendEmailNotification(dynamic dataModel,string message)
        //{
        //    // E-posta gönderme işlemleri burada yapılır
        //    // Örneğin, SMTP kullanarak e-posta gönderme işlemi
        //    try
        //    {
        //        // SMTP ayarlarını yapılandır
        //        var smtpClient = new SmtpClient("smtp.gmail.com")
        //        {
        //            Port = 587,
        //            Credentials = new NetworkCredential("tr.ozclkonur@gmail.com", "qsfq dtne jrin xknf"),
        //            EnableSsl = true,
        //        };

        //        var mailMessage = new MailMessage
        //        {
        //            From = new MailAddress("tr.ozclkonur@gmail.com"),
        //            Subject = "Hata Bildirimi",
        //            Body = message,
        //            IsBodyHtml = true,
        //        };

        //        mailMessage.To.Add("o.ozcelik@designtech.com.tr");

        //        smtpClient.Send(mailMessage);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error("E-posta gönderimi sırasında hata oluştu: " + ex.Message);
        //    }
        //}
        //MaillService



        //Aynı log kaydı var güncelle böylelikle üst üste aynı hata logunu eklemyeccek yada diğer logu direkt güncelleyecek
        //public void CreateJsonFileLog(dynamic dataModel, string message = null)
        //{
        //    try
        //    {
        //        // Log dosyasının adını ve yolunu oluştur
        //        currentMonthFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration\\logs", "TakvimFile");
        //        string dateFormatted = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
        //        string islemTarihi = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
        //        logFileName = Path.Combine(currentMonthFolder, dateFormatted + ".json");

        //        JArray dataArray;
        //        if (File.Exists(logFileName))
        //        {
        //            string jsonData = File.ReadAllText(logFileName);
        //            dataArray = JArray.Parse(jsonData); // JArray'e dönüştür
        //        }
        //        else
        //        {
        //            dataArray = new JArray();
        //        }

        //        // Yeni veriyi oluştur
        //        JObject jsonDataObject = JObject.Parse(dataModel);

        //        // Mesaj girildiyse ekle
        //        if (!string.IsNullOrEmpty(message))
        //        {
        //            jsonDataObject.Add("Mesaj", message);
        //        }

        //        jsonDataObject.Add("islemTarihi", islemTarihi);

        //        // Aynı mesajlı bir kayıt varsa kaldır
        //        for (int i = 0; i < dataArray.Count; i++)
        //        {
        //            if (dataArray[i]["Mesaj"]?.ToString() == message)
        //            {
        //                dataArray.RemoveAt(i);
        //                break; // Sadece bir tane sil, aynı mesajlı birden fazla kayıt olabilir
        //            }
        //        }

        //        // Diziye ekle
        //        dataArray.Add(jsonDataObject);

        //        // JSON dosyasına yaz
        //        File.WriteAllText(logFileName, JsonConvert.SerializeObject(dataArray, Formatting.Indented));
        //    }
        //    catch (Exception ex)
        //    {
        //        // Hata durumunda log oluşturma işlemi
        //        Log.Error("Log oluşturulurken hata oluştu: " + ex.Message);
        //    }
        //}


        //public void CreateJsonFileLog(dynamic dataModel,string message)
        //{
        //    try
        //    {
        //        // Log dosyasının adını ve yolu oluştur
        //        currentMonthFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configuration\\logs", "TakvimFile");
        //        string dateFormatted = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
        //        logFileName = Path.Combine(currentMonthFolder, dateFormatted + ".json");

        //        JArray dataArray;
        //        if (File.Exists(logFileName))
        //        {
        //            string jsonData = File.ReadAllText(logFileName);
        //            dataArray = JArray.Parse(jsonData);  // JArray'e dönüştür
        //        }
        //        else
        //        {
        //            dataArray = new JArray();
        //        }

        //        // Yeni veriyi diziye ekle
        //        dataArray.Add(JObject.Parse(dataModel));


        //        // JSON dosyasına yaz
        //        File.WriteAllText(logFileName, JsonConvert.SerializeObject(dataArray, Formatting.Indented));
        //    }
        //    catch (Exception ex)
        //    {
        //        // Hata durumunda log oluşturma işlemi
        //        Log.Error("Log oluşturulurken hata oluştu: " + ex.Message);
        //    }
        //}



        //public void CreateJsonFileLog(dynamic dataModel)
        //{
        //    try
        //    {


        //        // Log dosyasının adını ve yolu oluştur
        //        currentMonthFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "TakvimFile");
        //        string dateFormatted = DateTime.Now.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
        //        logFileName = Path.Combine(currentMonthFolder, dateFormatted + ".json");

        //        string jsonData = "";
        //        if (File.Exists(logFileName))
        //        {
        //            jsonData = File.ReadAllText(logFileName);
        //        }

        //        JObject jsonObject;
        //        if (string.IsNullOrWhiteSpace(jsonData))
        //        {
        //            // JSON verisi boşsa yeni bir nesne oluştur
        //            jsonObject = new JObject
        //            {
        //                ["data"] = new JArray()
        //            };
        //        }
        //        else
        //        {
        //            // JSON verisi mevcutsa, onu bir nesneye çevir
        //            jsonObject = JObject.Parse(jsonData);
        //        }

        //        // Yeni veriyi JSON nesnesine ekle veya güncelle
        //        JArray dataArray = (JArray)jsonObject["data"];
        //        dataArray.Add(JObject.FromObject(dataModel));


        //        // JSON dosyasına yaz
        //        File.WriteAllText(logFileName, JsonConvert.SerializeObject(jsonObject, Formatting.Indented));
        //        // Log dosyasına yazma işlemi
        //        //Log.Logger = new LoggerConfiguration()
        //        //    .MinimumLevel.Information()
        //        //    .WriteTo.File(new CustomJsonFormatter(), logFileName, shared: true)
        //        //    .CreateLogger();

        //        //// JSON verisini log'a yazma işlemi
        //        //var jsonText = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
        //        //Log.Information(jsonText);
        //        //Log.CloseAndFlush();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Hata durumunda log oluşturma işlemi
        //        Log.Error("Log oluşturulurken hata oluştu: " + ex.Message);
        //    }
        //}


    }
}
