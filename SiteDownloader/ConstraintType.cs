using System;

namespace SiteDownloader
{
    [Flags]
    public enum ConstraintType
    {
        FileConstraint = 1,
        UrlConstraint = 2
    }
}
