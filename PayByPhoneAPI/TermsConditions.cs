using PayByPhoneAPI.Items;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayByPhoneAPI
{
    class TermsConditions : ApiSection
    {
       public TermsConditions(PayByPhoneApi api) : base(api)
        {
        }        

        public async Task<Items.TermsConditions> GetTermsConditions()
        {
            var doc = await this.LoadOptions(Sections.Button.TermsConditions);
            
            // process this doc
            return new Items.TermsConditions(doc.DocumentNode);
        }
    }
    namespace Items
    {

        class TermsConditions
        {
            public string InfoHtml { get; private set; }
            public string InfoText { get; private set; }


            public TermsConditions(HtmlAgilityPack.HtmlNode tc)
            {
                // process the data to get the security settings                                
                var node = tc.SelectSingleNode("//userlog");
                InfoHtml = node.OuterHtml;
                InfoText = node.InnerText;

            }
            public override string ToString()
            {
                return InfoText;
            }
        }
    }
}
