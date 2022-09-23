namespace CalpineAzureDashboard.Data.Repository
{
    public class ResourceGroupRepository<T> : BaseRepository<T>
    {
        public ResourceGroupRepository(string conn) : base(conn)
        {
        }
    }
}