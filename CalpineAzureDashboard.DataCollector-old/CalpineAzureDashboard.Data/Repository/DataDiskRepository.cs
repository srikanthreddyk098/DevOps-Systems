namespace CalpineAzureDashboard.Data.Repository
{
    public class DataDiskRepository<T> : BaseRepository<T>
    {
        public DataDiskRepository(string conn) : base(conn)
        {
        }
    }
}