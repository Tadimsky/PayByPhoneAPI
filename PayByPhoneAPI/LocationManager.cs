using System;
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
            await parseLocationPage(doc);

            return true;
        }

        public void Test()
        {
            loadLocationPage();
        }

        private async Task<bool> parseLocationPage(HtmlDocument doc)
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

    class RecentLocation
    {
        public string Name { get; set; }
        public string LocationID { get; private set; }

        public RecentLocation(HtmlNode node)
        {
            LocationID = node.GetAttributeValue("value", "");
            if (node.NextSibling != null)
            {
                Name = node.NextSibling.InnerText;
            }
        }

        public static List<RecentLocation> ParseLocations(HtmlNode node)
        {
            var list = new List<RecentLocation>();
            if (node != null)
            {
                var locations = node.SelectNodes("option").Select(option => new RecentLocation(option));
                list = new List<RecentLocation>(locations);
            }
            return list;
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
