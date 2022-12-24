namespace Whetstone.StoryEngine.Reporting.Models
{


    public enum ReportDataSourceType
    {
        DatabaseFunction = 1,
        DatabaseQuery = 2
    }
    /// <summary>
    /// This class defines where to retrieve the data, including parameter mappings
    /// </summary>
    public abstract class ReportDataSourceBase
    {
        public abstract ReportDataSourceType DataSourceType { get; set; }
    }
}
