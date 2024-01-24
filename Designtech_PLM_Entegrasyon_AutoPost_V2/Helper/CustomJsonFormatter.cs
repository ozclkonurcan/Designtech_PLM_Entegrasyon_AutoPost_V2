using Serilog.Events;
using Serilog.Formatting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost.Helper
{
    public class CustomJsonFormatter : ITextFormatter
    {
        public void Format(LogEvent logEvent, TextWriter output)
        {
            output.Write("{");

            output.Write($"\"Timestamp\":\"{logEvent.Timestamp:dd/MM/yyyy - HH:mm:ss}\",");
            output.Write($"\"Message\":{logEvent.MessageTemplate}");
            output.Write(",\"Properties\": {");

            bool precedingElement = false;
            foreach (var property in logEvent.Properties)
            {
                if (precedingElement)
                {
                    output.Write(",");
                }

                output.Write($"\"{property.Key}\":\"{property.Value}");
                precedingElement = true;
            }

            output.Write("}},");
            output.WriteLine();
        }
    }
}
