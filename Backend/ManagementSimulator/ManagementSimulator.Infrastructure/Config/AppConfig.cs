using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagementSimulator.Infrastructure.Config
{
    public static class AppConfig
    {
        public static bool ConsoleLogQueries { get; set; } = true;
        public static ConnectionStrings? ConnectionStrings { get; set; }

        public static void Init(IConfiguration configuration)
        {
            ConnectionStrings = configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();
        }
    }
}
