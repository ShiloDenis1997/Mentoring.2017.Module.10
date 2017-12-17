using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteDownloader
{
    public class Settings
    {
        public int MaxDeepLevel { get; set; }
        public BetweenDomainTransition BetweenDomainTransition { get; set; }
        public List<string> AvailableExtensions { get; set; }
    }
}
