using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SqlKata.Compilers;
using SqlKata.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Model
{
    public class PlmDatabase
    {
        private readonly IConfiguration _configuration;

        public PlmDatabase(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public QueryFactory Connect()
        {
            string connectionString = _configuration.GetConnectionString("Plm");
            var connection = new SqlConnection(connectionString);
            var compiler = new SqlServerCompiler();

            var db = new QueryFactory(connection, compiler);
            return db;
        }
    }
}
