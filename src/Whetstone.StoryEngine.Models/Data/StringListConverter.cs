using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Whetstone.StoryEngine.Models.Data
{
    public class StringListConverter : TypeConverter
    {

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }
            else if (sourceType == typeof(string[]))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }
            else if (destinationType == typeof(string[]))
            {
                return true;
            }

            return base.CanConvertFrom(context, destinationType);

        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            List<string> sourceList = (List<string>)value;



            if (destinationType == typeof(string))
            {
                string retVal = string.Join(",", sourceList);
                return retVal;
            }
            else if (destinationType == typeof(string[]))
            {
                string[] destArray = sourceList.ToArray<string>();
                return destArray;

            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            string s = value as string;



            if (!string.IsNullOrEmpty(s))
            {
                return ((string)value).Split(',').ToList<string>();
            }
            if (value is string[])
            {
                List<string> retList = null;
                if (value != null)
                {
                    retList = ((string[])value).ToList<string>();
                }

                return retList;
            }


            return base.ConvertFrom(context, culture, value);
        }
    }
}
