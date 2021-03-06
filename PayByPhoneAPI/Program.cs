﻿using HtmlAgilityPack;
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
        PayByPhoneApi _api;

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
                    case "Get Email":
                        p.GetEmail();
                        break;
                    case "Edit Email":
                        p.EditEmail();
                        break;
                    case "Get Security":
                        p.GetSecuritySettings();
                        break;
                    case "Edit Security":
                        p.EditSecuritySettings();
                        break;
                    case "Get TC":
                        p.GetTc();
                        break;
                    case "Test":
                        p.Test();
                        break;
                    case "Recent Locations":
                        p.GetRecentLocations();
                        break;
                    case "Select Recent Location":
                        p.SelectRecentLocation();
                        break;
                    case "Select Location":
                        p.SelectLocation();
                        break;
                }
            }
        }

        private async void Login()
        {   
            bool val;
            _api = new PayByPhoneApi();
            await _api.Login("7202564696", "2343");
            return;
            do
            {
                Console.WriteLine("Login to PayByPhone");
                Console.Write("\tUsername: ");
                var username = Console.ReadLine();

                Console.Write("\tPassword: ");
                var password = Console.ReadLine();

                val = await _api.Login(username, password);
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
            foreach (var v in await _api.GetVehicles())
            {
                Console.WriteLine("\t{0}\t{1}", v.LicensePlate, v.Type.ToString());
            }
        }

        private void Logout()
        {
            _api.Logout();
        }

        private async void AddVehicle()
        {
            Items.Vehicle v = new Items.Vehicle();
            v.LicensePlate = Guid.NewGuid().ToString().Substring(0, 5);
            Console.WriteLine("\tAdding New Vehicle: {0}", v.LicensePlate);
            await _api.CreateVehicle(v);            
        }

        private async void EditVehicle()
        {
            var v = await _api.GetVehicles();
            var vehicle = v.First();

            var old = vehicle.LicensePlate;

            vehicle.LicensePlate = Guid.NewGuid().ToString().Substring(0, 5);

            Console.WriteLine("\tEditing Vehicle: {0} -> {1}", old, vehicle.LicensePlate);
            await _api.UpdateVehicle(vehicle);
        }

        private async void DeleteVehicle()
        {
            var v = await _api.GetVehicles();
            var vehicle = v.First();
            if (vehicle != null)
            {
                Console.WriteLine("\tDeleting Vehicle: {0}", vehicle.LicensePlate);

                await _api.DeleteVehicle(vehicle);
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

            await _api.UploadCard(cc);
        }

        private async void GetEmail()
        {
            Console.WriteLine((await _api.GetEmailSetting()).ToString());
        }

        private async void EditEmail()
        {
            Items.EmailSetting email = new Items.EmailSetting();
            email.EmailAddress = "jonno@schmidtfam.us";
            email.EmailReceipts = true;
            email.TextReminders = true;

            await _api.UpdateEmailSetting(email);
        }

        private async void GetSecuritySettings()
        {
            Console.WriteLine((await _api.GetSecuritySetting()).ToString());
        }

        private async void EditSecuritySettings()
        {
            Items.SecuritySetting security = new Items.SecuritySetting();
            security.RememberPin = false;
            security.SkipVoicePin = true;
            security.ChangePin("2343", "2343", "2343");
            await _api.UpdateSecuritySetting(security);
        }

        private async void GetTc()
        {
            Console.WriteLine((await _api.GetTermsAndConditions()).InfoHtml);
        }

        private void Test()
        {
            _api.Test();
        }

        private async void GetRecentLocations()
        {
            foreach (var recentLocation in await _api.GetRecentLocations())
            {
                Console.WriteLine($"\t{recentLocation}");
            }
        }

        private async void SelectRecentLocation()
        {
            var location = await _api.GetRecentLocations();
            var res = await _api.SelectLocation(location.First());
        }

        private async void SelectLocation()
        {
            var location = new SearchLocation();
            location.LocationId = "123";

            var res = await _api.SelectLocation(location);
            if (res is SingleLocationResult)
            {
                // selected location
            }
            else
            {
                MultipleLocationResult mlr = (MultipleLocationResult) res;
                res = await mlr.RefineSelection(mlr.Locations.First());
            }
        }
    }
}
