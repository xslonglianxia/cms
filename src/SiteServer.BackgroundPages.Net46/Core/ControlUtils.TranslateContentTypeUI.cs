using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.Model;
using SiteServer.CMS.Model.Enumerations;
using SiteServer.CMS.Plugin.Impl;
using SiteServer.Utils;

namespace SiteServer.BackgroundPages.Core
{
    public static partial class ControlUtils
    {
        public static class TranslateContentTypeUI
        {
            public static ListItem GetListItem(ETranslateContentType type, bool selected)
            {
                var item = new ListItem(ETranslateContentTypeUtils.GetText(type), ETranslateContentTypeUtils.GetValue(type));
                if (selected)
                {
                    item.Selected = true;
                }
                return item;
            }

            public static void AddListItems(ListControl listControl, bool isCut)
            {
                if (listControl != null)
                {
                    listControl.Items.Add(GetListItem(ETranslateContentType.Copy, false));
                    if (isCut)
                    {
                        listControl.Items.Add(GetListItem(ETranslateContentType.Cut, false));
                    }
                    listControl.Items.Add(GetListItem(ETranslateContentType.Reference, false));
                    listControl.Items.Add(GetListItem(ETranslateContentType.ReferenceContent, false));
                }
            }
        }
    }
}

