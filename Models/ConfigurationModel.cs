using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;

namespace RG.Plugin.eBayCommander.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.eBayCommander.eBayToken")]
        public string eBayToken { get; set; }

        [NopResourceDisplayName("Plugins.eBayCommander.eBayDefaultStoreId")]
        public int eBayDefaultStoreId { get; set; }

        [NopResourceDisplayName("Plugins.eBayCommander.eBayDefaultProductId")]
        public int eBayDefaultProductId { get; set; }

    }
}