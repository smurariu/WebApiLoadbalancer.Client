﻿using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace WebApiApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                            .UseKestrel()
                            .UseStartup<Startup>()
                            .Build();

            host.Run();
        }
    }
}
