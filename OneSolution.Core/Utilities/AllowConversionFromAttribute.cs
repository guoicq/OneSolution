using System;
using System.Collections.Generic;
using System.Text;

namespace OneSolution.Core.Utilities
{
    public class AllowConversionFromAttribute : Attribute
    {
        public AllowConversionFromAttribute(Type sourceType) { }
    }

    public class ConvertToAttribute : Attribute
    {
        public ConvertToAttribute(Type targetType) { }
    }
}
