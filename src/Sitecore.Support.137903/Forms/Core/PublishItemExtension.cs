using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Events;
using Sitecore.Form.Core.Configuration;
using Sitecore.Publishing;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sitecore.Support.Forms.Core
{
    using Sitecore.Data.Fields;

    public class PublishItemExtension
    {
        private const string GuidPattern = "(\\{){0,1}[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}(\\}){0,1}";

        /// <summary>
        /// Modified method for patch, added check for field being null
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void PublishFormChildItems(object sender, EventArgs args)
        {
            string value = IDs.FormInterpreterID.ToString();
            string value2 = IDs.FormMvcInterpreterID.ToString();
            Sitecore.Events.SitecoreEventArgs sitecoreEventArgs = args as Sitecore.Events.SitecoreEventArgs;
            Sitecore.Publishing.Publisher publisher = sitecoreEventArgs.Parameters.FirstOrDefault<object>() as Sitecore.Publishing.Publisher;
            if (publisher.Options.PublishRelatedItems && publisher.Options.Mode == Sitecore.Publishing.PublishMode.SingleItem)
            {
                Sitecore.Data.Items.Item rootItem = publisher.Options.RootItem;
                Field finalRenderings = rootItem.Fields["__Final renderings"];
                if (finalRenderings != null)
                {
                    string value3 = finalRenderings.Value;
                    if (value3.Contains(value) || value3.Contains(value2))
                    {
                        Sitecore.Data.Database sourceDatabase = publisher.Options.SourceDatabase;
                        MatchCollection matchCollection = Regex.Matches(value3, "(\\{){0,1}[0-9a-fA-F]{8}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{4}\\-[0-9a-fA-F]{12}(\\}){0,1}");
                        foreach (object current in matchCollection)
                        {
                            Sitecore.Data.Items.Item item = sourceDatabase.GetItem(current.ToString());
                            if (item != null && item.TemplateID == IDs.FormTemplateID)
                            {
                                Sitecore.Data.Database database = Sitecore.Data.Database.GetDatabase("web");
                                this.PublishItem(item, sourceDatabase, database, Sitecore.Publishing.PublishMode.SingleItem);
                            }
                        }
                    }
                }
            }
        }

        private void PublishItem(Sitecore.Data.Items.Item item, Sitecore.Data.Database sourceDB, Sitecore.Data.Database targetDB, Sitecore.Publishing.PublishMode mode)
        {
            Sitecore.Publishing.PublishOptions options = new Sitecore.Publishing.PublishOptions(sourceDB, targetDB, mode, item.Language, DateTime.Now)
            {
                RootItem = item,
                Deep = true
            };
            Sitecore.Publishing.Publisher publisher = new Sitecore.Publishing.Publisher(options);
            publisher.Publish();
        }
    }
}