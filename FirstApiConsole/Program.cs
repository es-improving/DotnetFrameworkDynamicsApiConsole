using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http.Headers;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using FirstApiConsole.Entities;
using System.Configuration;

namespace FirstApiConsole
{
    class Program
    {
        private static DynamicsConnector _connector;

        static void Main(string[] args)
        {
            string userEmail = ConfigurationManager.AppSettings["userEmail"];
            string userPassword = ConfigurationManager.AppSettings["userPassword"];
            string clientId = ConfigurationManager.AppSettings["clientId"];
            string tenantUrl = ConfigurationManager.AppSettings["tenantUrl"];

            _connector = new DynamicsConnector(userEmail, userPassword, clientId, tenantUrl);

            Console.WriteLine("Welcome to my oh so groovy console app to demonstrate calling Dynamics APIs from C#.");
            Console.WriteLine("First of all, would you like for me to write API response bodies to the console for you to see them? Y or N?");
            var showResponsesResponse = Console.ReadKey();
            Console.WriteLine("");

            if (showResponsesResponse.KeyChar == 'Y')
            {
                Console.WriteLine("Output display enabled.");
                _connector.PrettyPrintJson = true;
            }
            else
            {
                Console.WriteLine("Output display disabled.");
            }

            while(true)
            {
                Console.WriteLine("\r\nWhich flow do you want to try out? Choose a number.");
                Console.WriteLine("  1  - Try the get account list and update account description flow.");
                Console.WriteLine("  2  - Try the system user and roles join flow.");
                Console.WriteLine("  3  - Get accounts by name using JObject.");
                Console.WriteLine("  4  - Get accounts by name using C# generics.");
                Console.WriteLine("  5  - Run a fetch xml query.");
                Console.WriteLine("  X  - Running this app was a HUGE mistake, so get me outta here!");
                Console.WriteLine("");

                var flowChoice = Console.ReadKey();
                Console.WriteLine("");

                switch (flowChoice.KeyChar)
                {
                    case '1':
                        GetAndUpdateAccountFlow();
                        break;
                    case '2':
                        JoinAndMetadataBrowserDemo();
                        break;
                    case '3':
                        GetAccountsWithJObject();
                        break;
                    case '4':
                        GetAccountsWithGenerics();
                        break;
                    case '5':
                        FetchXmlExample();
                        break;
                    default:
                        Console.WriteLine("Goodbye!");
                        return;
                }

            }

            Console.WriteLine("Complete!!!!");
            Console.ReadLine();            
        }


        private static void GetAndUpdateAccountFlow()
        {
            string accountUriTop = "accounts";
            var result = _connector.Get<Account>(accountUriTop);

            Console.WriteLine("Accounts: ");
            foreach (var account in result.Value)
            {
                Console.WriteLine($"  {account.Name} ({account.AccountId}) - {account.Description}");
            }

            Console.WriteLine("\r\nWhat account do you want to update?");
            string accountId = Console.ReadLine();

            Console.WriteLine("\r\nPlease enter the new description.");
            string description = Console.ReadLine();

            Console.WriteLine($"{accountId} {description}");


            string updateAccountUri = $"accounts({accountId})";

            var payload = new
            {
                description = description
            };
            _connector.Patch(updateAccountUri, payload);
        }


        private static void JoinAndMetadataBrowserDemo()
        {
            string systemusersjoin = "systemusers?$select=title,firstname,lastname&$expand=systemuserroles_association($select=name)&$filter=contains(firstname, 'Sanjay') and contains(lastname, 'Shah')";

            var users = _connector.GetJObject(systemusersjoin);
        }



        private static void GetAccountsWithJObject()
        {
            Console.WriteLine("What query would you like to use to find accounts? This will search the `name` and `description` fields.");
            var query = Console.ReadLine();
            
            string accountFilterUri = $"accounts?$select=name,description&$filter=contains(name,'{query}') and contains(description,'{query}')";

            var accounts = _connector.GetJObject(accountFilterUri);

            Console.WriteLine("The following accounts were found:");
            foreach (var account in accounts["value"])
            {
                Console.WriteLine($"  - {account["name"]} ({account["accountid"]}) - {account["description"]}");
            }
            
        }

        private static void GetAccountsWithGenerics()
        {
            Console.WriteLine("What query would you like to use to find accounts? This will search the `name` and `description` fields.");
            var query = Console.ReadLine();

            string accountFilterUri = $"accounts?$select=name,description&$filter=contains(name,'{query}') and contains(description,'{query}')";

            var accounts = _connector.Get<Account>(accountFilterUri);

            Console.WriteLine("The following accounts were found:");
            foreach (var account in accounts.Value)
            {
                Console.WriteLine($"  - {account.Name} ({account.AccountId}) - {account.Description}");
            }
        }

        private static void FetchXmlExample()
        {
            string encodedXml = "%3Cfetch%20mapping%3D%27logical%27%3E%20%20%20%0A%20%20%20%3Centity%20name%3D%27account%27%3E%20%20%0A%20%20%20%20%20%20%3Cattribute%20name%3D%27accountid%27%2F%3E%20%20%20%0A%20%20%20%20%20%20%3Cattribute%20name%3D%27name%27%20%2F%3E%20%20%20%0A%20%20%20%3C%2Fentity%3E%20%20%0A%3C%2Ffetch%3E%20";
            //string encodedXml = "<fetch mapping='logical'><entity name='account'><attribute name='accountid'/><attribute name='name' /></ entity></ fetch>";
            string uri = $"accounts?fetchXml={encodedXml}";

            var accounts = _connector.GetWithFetchXml(uri);

            Console.WriteLine("The following accounts were found:");
            foreach (var account in accounts["value"])
            {
                Console.WriteLine($"  - {account["name"]} ({account["accountid"]}) - {account["description"]}");
            }
        }
    }
}