using Nop.Core.Configuration;

namespace RG.Plugin.eBayCommander
{
    public class eBayCommanderSettings : ISettings
    {
        public string eBayToken { get; set; }
        public int eBayDefaultStoreId { get; set; }
        public int eBayDefaultProductId { get; set; }
    }
}