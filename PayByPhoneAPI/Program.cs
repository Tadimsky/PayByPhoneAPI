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
        static void Main(string[] args)
        {
            Program p = new Program();
            p.Login();
            Console.ReadLine();
        }

        private void Login()
        {
            bool val;
            PayByPhoneAPI api = new PayByPhoneAPI();
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
    }
}
