using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace IO.Swagger
{
    public static class Companies
    {
        public static bool IsCompanyExists(string passedCompany)
        {
            /*List<string> existingCompanies = new List<string>() {
                    "Derendinger-Switzerland",
                    "Technomag-Switzerland",
                    "Klaus-Switzerland",
                    "Matik-Switzerland",
                    "Walchli-Bollier-Bulach",
                    "Matik-Austria",
                    "Derendinger-Austria",
                    "Remco-Belgium",
                    "Cross-Company-Austria",
                    "Cross-Company-Switzerland",
                    "Wint-Serbia"
            };*/
            //return GetCompanies().Contains(passedCompany);
            return true;
        }
        public static List<string> GetCompanies()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var configuation = builder.Build();
            return configuation.GetSection("Companies").Value.Split(',').ToList();
        }
    }
}
