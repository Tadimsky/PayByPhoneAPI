using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using HtmlAgilityPack;

namespace PayByPhoneAPI
{
    class SearchLocation : Location
    {
        public string LocationId { get; set; }

        public SearchLocation()
        {
            LocationId = "";
        }

        public SearchLocation(string location)
        {
            LocationId = location;
        }

        public virtual NameValueCollection GetWebFormData()
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add(FormInputNames.LocationSearch.LocationSearchBox, LocationId);
            return nvc;
        }
    }

    class RecentLocation : SearchLocation
    {
        public RecentLocation(HtmlNode node)
        {
            LocationId = node.GetAttributeValue("value", "");
            if (node.NextSibling != null)
            {
                Name = node.NextSibling.InnerText;
            }
        }

        public override NameValueCollection GetWebFormData()
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add(FormInputNames.LocationSearch.PreviousLocationSearch, LocationId);
            return nvc;
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
}