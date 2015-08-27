using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace PayByPhoneAPI
{
    class Location
    {
        public string Name { get; set; }

        public Location()
        {
            Name = "";
        }
    }

    class SearchLocation : Location
    {
        public string LocationID { get; set; }

        public SearchLocation()
        {
            LocationID = "";
        }

        public SearchLocation(string location)
        {
            LocationID = location;
        }

        public virtual NameValueCollection GetWebFormData()
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add(FormInputNames.LocationSearch.LocationSearchBox, LocationID);
            return nvc;
        }
    }

    internal class ResultLocation : Location
    {
        // no difference between them, just want a super class

        public string Authority { get; private set; }

        private ResultLocation(HtmlNode html)
        {
            // parse html
        }
    }

    internal class DifferentiateResultLocation : Location
    {
        public int Index { get; private set; }

        private DifferentiateResultLocation(HtmlNode html)
        {
            // parse the html result
        }

        public static List<DifferentiateResultLocation> ParseLocations(HtmlNode html)
        {
            var list = new List<DifferentiateResultLocation>();
            // iterate through the list and create new items

            return list;
        }
    }

    class RecentLocation : SearchLocation
    {
        public RecentLocation(HtmlNode node)
        {
            LocationID = node.GetAttributeValue("value", "");
            if (node.NextSibling != null)
            {
                Name = node.NextSibling.InnerText;
            }
        }

        public override NameValueCollection GetWebFormData()
        {
            return null;
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

    abstract class LocationResult
    {
    }

    internal class SingleLocationResult : LocationResult
    {
        public ResultLocation Location { get; set; }
    }

    internal class MultipleLocationResult : LocationResult
    {
        private PayByPhoneAPI _api;

        public MultipleLocationResult(PayByPhoneAPI api)
        {
            _api = api;
        }

        public LocationResult RefineSelection(DifferentiateResultLocation location)
        {
            // update with the new location 

            // return the result - should be a singlelocation
            // error? shitt
            return null;
        }

        public List<DifferentiateResultLocation> Locations { get; set; }
    }

    namespace FormInputNames
    {
        internal static class LocationSearch
        {
            public const string LocationSearchBox = "ctl00$ContentPlaceHolder1$LocationNumberTextBox";
            public const string PreviousLocationSearch = "ctl00$ContentPlaceHolder1$PreviousLocationDropDownList";
        }
    }

}
