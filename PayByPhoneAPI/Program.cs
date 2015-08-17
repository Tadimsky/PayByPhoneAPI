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
                }
            }
        }

        private void Login()
        {
            bool val;
            api = new PayByPhoneAPI();
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

        private void Logout()
        {
            api.Logout();
        }
    }
}
