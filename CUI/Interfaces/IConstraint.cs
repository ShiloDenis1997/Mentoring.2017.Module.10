using System;
using CUI.Enums;

namespace CUI.Interfaces
{
    public interface IConstraint
    {
        ConstraintType ConstraintType { get; }
        bool IsAcceptable(Uri uri);
    }
}
