using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Text.RegularExpressions;
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

        public override string ToString()
        {
            return Name;
        }
    }

    class ResultLocation : Location
    {
        // no difference between them, just want a super class

        public string Authority { get; private set; }

        public ResultLocation()
        {
            
        }

        public ResultLocation(HtmlNode html)
        {
            // parse html
            var table = html.SelectSingleNode($"//table[@id='{FormInputNames.LocationSearch.SingleLocationLabelTable}']");
            Name = table?.SelectSingleNode($".//span[@id='{FormInputNames.LocationSearch.SingleLocationNameBox}']")?
                    .InnerText;

            Authority = table?.SelectSingleNode($".//span[@id='{FormInputNames.LocationSearch.SingleLocationAuthorityNameBox}']")?
                    .InnerText;
        }
    }

    class DifferentiateResultLocation : ResultLocation
    {
        public int Index { get; private set; }

        public DifferentiateResultLocation(HtmlNode html)
        {
            // parse the html result
            Name = html.InnerText;
            var href = html.SelectSingleNode(".//a").GetAttributeValue("href", "");
            href = WebUtility.HtmlDecode(href);
            var regex = new Regex(@"\((.+)\',\'(\d+)");
            var matches = regex.Match(href);
            Index = int.Parse(matches.Groups[2].Value);
        }

        /// <summary>
        /// Parses the locations that were provided by the web site when a parking location was entered that has 
        /// more than one result.
        /// </summary>
        /// <param name="html">The html document node</param>
        /// <returns></returns>
        public static List<DifferentiateResultLocation> ParseLocations(HtmlNode html)
        {
            var list = new List<DifferentiateResultLocation>();
            // iterate through the list and create new items
            var ul = html.SelectSingleNode($"//ul[@id='{Constants.ChooseLocation.OverlappedLocationsListId}']");
            foreach (var node in ul.SelectNodes(".//li"))
            {
                list.Add(new DifferentiateResultLocation(node));
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
        private PayByPhoneApi _api;

        public MultipleLocationResult(PayByPhoneApi api)
        {
            _api = api;
        }

        public LocationResult RefineSelection(DifferentiateResultLocation location)
        {
            // update with the new location 
            // Constants.ChooseLocation.OverlappedLocationTarget

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
            public const string SingleLocationLabelTable = "LabelTable";
            public const string SingleLocationNameBox = "ctl00_ContentPlaceHolder1_LocationNameBox_LabelText";
            public const string SingleLocationAuthorityNameBox = "ctl00_ContentPlaceHolder1_LocationNameBox_LabelMidText";
        }
    }

}
