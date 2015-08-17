using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PayByPhoneAPI
{
    class Program
    {
        PayByPhoneAPI api;

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Login();
            string message = "";
            while (!message.Equals("Exit"))
            {
                message = Console.ReadLine();
                switch (message)
                {
                    case "Logout":
                        p.Logout();
                        break;
                    case "Login":
                        p.Login();
                        break;
                    case "Vehicles":
                        p.GetVehicles();
                        break;
                }
            }
        }

        private void Login()
        {   
            bool val;
            api = new PayByPhoneAPI();
            api.Login("7202564696", "2343");
            return;
            do
            {
                Console.WriteLine("Login to PayByPhone");
                Console.Write("\tUsername: ");
                var username = Console.ReadLine();

                Console.Write("\tPassword: ");
                var password = Console.ReadLine();

                val = api.Login(username, password);
                if (!val)
                {
                    Console.WriteLine("Error Logging In");
                }
            }
            while (!val);
            Console.WriteLine("Logged in");
        }

        private void GetVehicles()
        {
            foreach (var v in api.GetVehicles())
            {
                Console.WriteLine("\t{0}\t{1}", v.LicensePlate, v.Type.ToString());
            }
        }

        private void Logout()
        {
            api.Logout();
        }
    }
}
