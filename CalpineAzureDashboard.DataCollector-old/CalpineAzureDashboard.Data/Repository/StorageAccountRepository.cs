namespace CalpineAzureDashboard.Data.Repository
{
    public class StorageAccountRepository<T> : BaseRepository<T>
    {
        public StorageAccountRepository(string conn) : base(conn)
        {
        }
    }
}