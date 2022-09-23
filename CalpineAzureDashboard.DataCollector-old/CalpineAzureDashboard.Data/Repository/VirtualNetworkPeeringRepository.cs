namespace CalpineAzureDashboard.Data.Repository
{
    public class VirtualNetworkPeeringRepository<T> : BaseRepository<T>
    {
        public VirtualNetworkPeeringRepository(string conn) : base(conn)
        {
        }
    }
}