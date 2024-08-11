using Dapper;
using MergeCommand.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MergeCommand
{
    internal class Program
    {
        private static IConfigurationRoot config;
        static void Main(string[] args)
        {
            try
            {
                Initialize();

                var sourceProduct = new SourceProducts()
                {

                    ProductID = 11,
                    Price = 12.33,
                    ProductName = "test product",     
                };

                using (var db = new SqlConnection(config.GetConnectionString("DefaultConnection")))
                //using (var db = new SqlConnection(config["ConnectionStrings:DefaultConnection"]))
                {
                    var sql = "MERGE SourceProducts AS Target " +
                                $"USING (select {sourceProduct.ProductID} as ProductID, {sourceProduct.Price} as Price, '{sourceProduct.ProductName}' as ProductName) AS Source " +
                                "ON Source.ProductID = Target.ProductID " +
                                "WHEN NOT MATCHED BY Target THEN " +
                                    "INSERT (ProductID,ProductName, Price) " +
                                    "VALUES (Source.ProductID,Source.ProductName, Source.Price) " +
                                "WHEN MATCHED THEN " +
                                "UPDATE SET Target.ProductName = Source.ProductName, " +
                                            "Target.Price = Source.Price;";
                    var rowCount = db.Execute(sql);
                    var query = db.Query<SourceProducts>("SELECT * FROM SourceProducts").ToList();
                    rowCount.Output();
                    query.Output();
                }

                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }

        private static void Initialize()
        {
            var builder = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            config = builder.Build();
        }
    }
}
