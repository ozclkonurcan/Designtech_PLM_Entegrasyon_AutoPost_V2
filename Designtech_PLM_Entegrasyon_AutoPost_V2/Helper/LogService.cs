using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;

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
