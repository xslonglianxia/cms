using System;
using System.Collections.Specialized;
using System.Text;
using SiteServer.Utils;

namespace SiteServer.CMS.StlParser.Model
{
    public static partial class Html
    {
        public abstract class HtmlBase
        {
            private StringBuilder _sb;

            protected HtmlBase(StringBuilder sb)
            {
                _sb = sb;
            }

            public StringBuilder GetBuilder()
            {
                return _sb;
            }

            protected void Append(string toAppend)
            {
                _sb.Append(toAppend);
            }

            protected void AddOptionalAttributes(NameValueCollection attributes)
            {
                if (attributes != null && attributes.Count > 0)
                {
                    _sb.Append(" ");
                    _sb.Append(TranslateUtils.ToAttributesString(attributes));
                }
                _sb.Append(">");
            }
        }
    }
}