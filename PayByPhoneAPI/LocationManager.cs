﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace PayByPhoneAPI
{
    class LocationManager : APISection
    {

        public List<ActiveParkingSession> ActiveParkingSessions { get; private set; } 
        public List<RecentLocation> RecentLocations { get; private set; } 
        public LocationManager(PayByPhoneAPI api) : base(api)
        {
            
        }
        private async Task<bool> loadLocationPage()
        {
            var doc = await myAPI.CallAPI("ChooseLocation.aspx");
            // after loading the page, parse whatever we can from the page
            parseLocationPage(doc);

            return true;
        }

        public LocationResult SelectLocation(SearchLocation location)
        {
            // use this location to search for a location on the site
            // if a recent location, set that as the id
            // else search for the number

            // returns select location result
            return null;
        }

        public void Test()
        {
            await loadLocationPage();
        }

        private bool parseLocationPage(HtmlDocument doc)
        {
            // parse recent locations
            RecentLocations = new List<RecentLocation>(
                RecentLocation.ParseLocations(
                    doc.DocumentNode.SelectSingleNode(
                        $"//select[@name='{Constants.ChooseLocation.PreviousLocationsDropDown}']"))
                );
            // parse current parking spots
            ActiveParkingSessions = new List<ActiveParkingSession>(
                ActiveParkingSession.ParseSessions(
                    doc.DocumentNode.SelectSingleNode($"//table[id='{Constants.ChooseLocation.ActiveParkingTable}']"))
                );
            
            // parse other things?

            return true;
        }
    }

    class ActiveParkingSession
    {
        private WebFormData myWebData
        {
            get;
            set;
        }

        public string ExtendAllowed { get; private set; }
        public string LotUid { get; private set; }
        public string StallUid { get; private set; }

        public string TimeLeft { get; private set; }
        public string LocationName { get; private set; }

        

        public ActiveParkingSession(HtmlNode parkingDetails, List<HtmlNode> hiddenFields)
        {
            TimeLeft = parkingDetails.SelectSingleNode(".//span[class='activetimeleft']")?.InnerText;
            LocationName = parkingDetails.SelectSingleNode(".//span[class='activelotdescription']")?.InnerText;

            // parse hidden fields
            
            myWebData = new WebFormData();
            foreach (var input in hiddenFields)
            {
                var name = input.GetAttributeValue("name", "");
                var value = input.GetAttributeValue("value", "");
                if (name.Contains("ExtendAllowedHiddenField"))
                {
                    myWebData.ExtendAllowed = name;
                    ExtendAllowed = value;
                }
                else
                {
                    if (name.Contains("LotUidHiddenField"))
                    {
                        myWebData.LotUid = name;
                        LotUid = value;
                    }
                    else
                    {
                        if (name.Contains("StallHiddenField"))
                        {
                            myWebData.StallUid = name;
                            StallUid = value;
                        }
                    }

                }
            }
        }

        public static List<ActiveParkingSession> ParseSessions(HtmlNode node)
        {
            var sessions = new List<ActiveParkingSession>();
            if (node != null)
            {
                // have the table
                var parkingRows = node.SelectNodes("tbody/tr[td[@class]]");
                var inputs = node.SelectNodes("input");
                // each parking row has 3 inputs
                foreach (var parkingRow in parkingRows)
                {
                    List<HtmlNode> hiddenFields = new List<HtmlNode>(inputs.Take(3));
                    // ghetto hacks
                    inputs.Remove(0);
                    inputs.Remove(0);
                    inputs.Remove(0);
                    sessions.Add(new ActiveParkingSession(parkingRow, hiddenFields));
                }
            }
            return sessions;
        }

        internal class WebFormData
        {
            public string ExtendAllowed { get; set; }
            public string LotUid { get; set; }
            public string StallUid { get; set; }
        }
    }

    namespace Constants
    {
        static class ChooseLocation
        {
            public const string PreviousLocationsDropDown = "ctl00$ContentPlaceHolder1$PreviousLocationDropDownList";
            public const string ActiveParkingTable = "ctl00_ContentPlaceHolder1_ActiveParkingGridView";
        }

    }
}
