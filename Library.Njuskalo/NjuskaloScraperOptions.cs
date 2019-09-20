using System.Collections.Generic;

namespace Library.Njuskalo
{
    public class NjuskaloScraperOptions
    {
        public string Name { get; set; }
        public string BaseUrl { get; set; }
        public string PathSegment { get; set; }
        public bool ScrapeAllPages { get; set; }
        public IDictionary<string, string> QueryParams { get; set; }
    }
}
