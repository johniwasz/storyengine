namespace Whetstone.StoryEngine.Models.Data
{
    public interface IJoinEntity<TEntity>
    {
        TEntity Navigation { get; set; }
    }
}
