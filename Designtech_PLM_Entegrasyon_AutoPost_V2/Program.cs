using Designtech_PLM_Entegrasyon_AutoPost.ApiServices;
using Designtech_PLM_Entegrasyon_AutoPost.Helper;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EmailSettings;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.EntegrasyonAyar.EntegrasyonDurum;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.EPMDocument.Attachment;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.Equivalence;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.Revise;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.WTPart.Alternate;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModulu.WTPart.State;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModuluError.EPMDocument.Attachment;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModuluError.Equivalence;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModuluError.WTPart.Alternate;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.EntegrasyonModuluError.WTPart.State;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.SqlSettigns;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Interfaces.WindchillApiSettings;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EmailSettings;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModulu.EntegrasyonAyar.EntegrasyonDurum;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModulu.EPMDocument.Attachment;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModulu.Equivalence;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModulu.Revise;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModulu.WTPart.Alternate;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModulu.WTPart.State;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModuluError.EPMDocument.Attachment;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModuluError.Equivalence;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModuluError.WTPart.Alternate;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.EntegrasyonModuluError.WTPart.State;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.SqlSettigns;
using Designtech_PLM_Entegrasyon_AutoPost_V2.Repositories.WindchillApiSettings;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
   //     [STAThread]
   //     static void Main()
   //     {
			//// To customize application configuration such as set high DPI settings or default font,
			//// see https://aka.ms/applicationconfiguration.


			//ApplicationConfiguration.Initialize();
   //         Application.Run(new Form1());
   //     }
        [STAThread]
        static void Main()
        {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			// DI konteynerini oluþturun
			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection);
			var serviceProvider = serviceCollection.BuildServiceProvider();

			// Form1'i baþlatýrken baðýmlýlýðý geçirin
			var form = serviceProvider.GetRequiredService<Form1>();
			Application.Run(form);
		}
		private static void ConfigureServices(IServiceCollection services)
		{
			// Servislerinizi burada kaydedin
			services.AddScoped<IEquivalenceService, EquivalenceRepository>();
			services.AddScoped<IAlternateService, AlternateRepository>();
			services.AddScoped<IStateService, StateRepository>();
			services.AddScoped<IEntegrasyonDurumService, EntegrasyonDurumRepository>();
			services.AddScoped<IWTPartReviseService, WTPartReviseRepository>();
			services.AddScoped<IAttachmentsService, AttachmentsRepository>();
			services.AddScoped<IClosedEnterationAttachmentsSerivce, ClosedEnterationAttachmentsRepository>();
			services.AddScoped<IEmailService, EmailRepository>();
			services.AddScoped<ISqlTriggerAndTableManagerService, SqlTriggerAndTableManagerRepository>();
			services.AddScoped<IGetWindchillApiServices, GetWindchillApiRepository>();

			//Error
			services.AddScoped<IErrorStateService, ErrorStateRepository>();
			services.AddScoped<IErrorAlternateService, ErrorAlternateRepository>();
			services.AddScoped<IErrorEquivalenceService, ErrorEquivalenceRepository>();
			services.AddScoped<IErrorAttachmentsService, ErrorAttachmentsRepository>();
			services.AddScoped<IErrorClosedEnterationAttachmentsSerivce, ErrorClosedEnterationAttachmentsRepository>();

			//Error
			services.AddScoped<Form1>(); // Formu da kaydedin
		}


	}
}