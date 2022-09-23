namespace CalpineAzureDashboard.Data.Repository
{
    public class AvailabilitySetRepository<T> : BaseRepository<T>
    {
        public AvailabilitySetRepository(string conn) : base(conn)
        {
        }
    }
}