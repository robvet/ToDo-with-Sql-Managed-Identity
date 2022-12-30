using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using static System.Net.Mime.MediaTypeNames;

namespace DotNetCoreSqlDb.Models
{
    public class MyDatabaseContext : DbContext
    {
        public MyDatabaseContext()
        {
        }

        public MyDatabaseContext (DbContextOptions<MyDatabaseContext> options)
            : base(options)
        {
            // TODO: Move this logic into a static constructor so that it executes once
            
            var connection = (Microsoft.Data.SqlClient.SqlConnection)Database.GetDbConnection();

            // mechansim for providing database access for local traffic (without managed identity) 
            // https://briancaos.wordpress.com/2022/09/06/c-sql-connection-using-azure-managed-identity/
            // A User Id value means we are running locally - use database using username/password. Connect the old fashioned way
            if (connection.ConnectionString.Contains("User ID"))
                return;

            // Provides a default TokenCredential authentication flow for applications that will be deployed to Azure.
            // https://learn.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet
            // When running in App Service, DefaultAzureCredential() uses app's system-assigned managed identity.
            // When running locally, it gets a token using the logged-in identity of VS, VS Code, or Azure CLI.
            var credential = new DefaultAzureCredential();

            // Get token from AAD
            try
            {
                var token = credential
                    .GetToken(new Azure.Core.TokenRequestContext(
                        new[] { "https://database.windows.net/.default" }));

            // Add token to the database connection
                connection.AccessToken = token.Token;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
            }
        }
       
        
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    var connection = (Microsoft.Data.SqlClient.SqlConnection)Database.GetDbConnection();
        //    var credential = new DefaultAzureCredential();
        //    var token = credential
        //            .GetToken(new Azure.Core.TokenRequestContext(
        //                new[] { "https://database.windows.net/.default" }));
        //    connection.AccessToken = token.Token;

        //    optionsBuilder.UseSqlServer(connection);
        //}

        public DbSet<DotNetCoreSqlDb.Models.Todo> Todo { get; set; }
    }
}
