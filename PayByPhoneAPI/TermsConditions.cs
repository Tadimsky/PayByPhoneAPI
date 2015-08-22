using PayByPhoneAPI.Items;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PayByPhoneAPI
{
    class TermsConditions : APISection
    {
       public TermsConditions(PayByPhoneAPI api) : base(api)
        {
        }        

        public async Task<Items.TermsConditions> GetTermsConditions()
        {
            var doc = await this.loadOptions(Sections.Button.TermsConditions);
            
            // process this doc
            return new Items.TermsConditions(doc.DocumentNode);
        }
    }
    namespace Items
    {

        class TermsConditions
        {
            public string InfoHTML { get; private set; }
            public string InfoText { get; private set; }


            public TermsConditions(HtmlAgilityPack.HtmlNode tc)
            {
                // process the data to get the security settings                                
                var node = tc.SelectSingleNode("//userlog");
                InfoHTML = node.OuterHtml;
                InfoText = node.InnerText;

            }
            public override string ToString()
            {
                return InfoText;
            }
        }
    }
}
