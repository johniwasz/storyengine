using System;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Serialization;

namespace Whetstone.StoryEngine.Reporting.Models
{

    public enum OutputDataType
    {
        String = 1,
        DateTime = 2,
        Guid = 3,
        Integer = 4,
        Boolean = 5

    }

    public enum ValueCasing
    {
        Upper =1,
        Lower =2

    }

    public class CsvColumnDefinition
    {
        /// <summary>
        /// Name of the parameter to map to CSV. If Alias is set, then the name of the column is the alias. Otherwise,
        /// it is the name of the mapped parameter.
        /// </summary>
        [YamlMember(Alias = "name")]
        public string Name { get; set; }

        [YamlMember(Alias = "regularExpressionFilter")]
        public RegexReplacement RegularExpressionFilter { get; set; }

        [YamlMember(Alias = "format")]
        public string Format { get; set; }


        [YamlMember(Alias = "isQuoted")]
        public bool? IsQuoted { get; set; }


        [YamlMember(Alias = "dataType")]
        public OutputDataType DataType { get; set; }

        [YamlMember(Alias = "casing")]
        public ValueCasing? Casing { get; set; }

    }

    public class RegexReplacement
    {

        [YamlMember(Alias = "regularExpression")]
        public string RegularExpression { get; set; }

        [YamlMember(Alias = "replacementText")]
        public string ReplacementText { get; set; }

    }
}
