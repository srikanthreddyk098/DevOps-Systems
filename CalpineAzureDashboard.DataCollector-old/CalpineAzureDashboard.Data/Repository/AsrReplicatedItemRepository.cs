namespace CalpineAzureDashboard.Data.Repository
{
    public class AsrReplicatedItemRepository<T> : BaseRepository<T>
    {
        public AsrReplicatedItemRepository(string conn) : base(conn)
        {
        }
    }
}