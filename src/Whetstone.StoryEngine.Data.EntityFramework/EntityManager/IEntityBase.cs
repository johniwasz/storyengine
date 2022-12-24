namespace Whetstone.StoryEngine.Data.EntityFramework.EntityManager
{
    public interface IEntityBase<TID>
    {
        TID Id { get; set; }


    }
}
