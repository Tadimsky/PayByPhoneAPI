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
                    case "Add Vehicle":
                        p.AddVehicle();
                        break;
                    case "Edit Vehicle":
                        p.EditVehicle();
                        break;
                    case "Delete Vehicle":
                        p.DeleteVehicle();
                        break;
                    case "Edit Card":
                        p.EditCard();
                        break;
                }
            }
        }

        private async void Login()
        {   
            bool val;
            api = new PayByPhoneAPI();
            await api.Login("7202564696", "2343");
            return;
            do
            {
                Console.WriteLine("Login to PayByPhone");
                Console.Write("\tUsername: ");
                var username = Console.ReadLine();

                Console.Write("\tPassword: ");
                var password = Console.ReadLine();

                val = await api.Login(username, password);
                if (!val)
                {
                    Console.WriteLine("Error Logging In");
                }
            }
            while (!val);
            Console.WriteLine("Logged in");
        }

        private async void GetVehicles()
        {
            foreach (var v in await api.GetVehicles())
            {
                Console.WriteLine("\t{0}\t{1}", v.LicensePlate, v.Type.ToString());
            }
        }

        private void Logout()
        {
            api.Logout();
        }

        private async void AddVehicle()
        {
            Items.Vehicle v = new Items.Vehicle();
            v.LicensePlate = Guid.NewGuid().ToString().Substring(0, 5);
            Console.WriteLine("\tAdding New Vehicle: {0}", v.LicensePlate);
            await api.CreateVehicle(v);            
        }

        private async void EditVehicle()
        {
            var v = await api.GetVehicles();
            var vehicle = v.First();

            var old = vehicle.LicensePlate;

            vehicle.LicensePlate = Guid.NewGuid().ToString().Substring(0, 5);

            Console.WriteLine("\tEditing Vehicle: {0} -> {1}", old, vehicle.LicensePlate);
            await api.UpdateVehicle(vehicle);
        }

        private async void DeleteVehicle()
        {
            var v = await api.GetVehicles();
            var vehicle = v.First();
            if (vehicle != null)
            {
                Console.WriteLine("\tDeleting Vehicle: {0}", vehicle.LicensePlate);

                await api.DeleteVehicle(vehicle);
            }
            else
            {
                Console.WriteLine("\tNo More Vehicles");
            }            
        }

        private async void EditCard()
        {
            Items.CreditCard cc = new Items.CreditCard();
            cc.ExpiryMonth = "01";
            cc.ExpiryYear = "2017";
            cc.Name = "Ja Schmidt";
            cc.Number = "52210083633010048";

            await api.UploadCard(cc);
        }

    }
}
