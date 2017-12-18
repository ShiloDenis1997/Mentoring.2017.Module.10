using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CUI.Constraints
{
    public class FileTypesConstraint : IConstraint
    {
        private readonly IEnumerable<string> _acceptableExtensions;

        public FileTypesConstraint(IEnumerable<string> acceptableExtensions)
        {
            _acceptableExtensions = acceptableExtensions;
        }

        public ConstraintType ConstraintType => ConstraintType.FileConstraint;

        public bool IsAcceptable(Uri uri)
        {
            string lastSegment = uri.Segments.Last();
            return _acceptableExtensions.Any(e => lastSegment.EndsWith(e));
        }
    }
}
