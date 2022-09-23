namespace CalpineAzureDashboard.Data.Repository
{
    public class VirtualMachineRepository<T> : BaseRepository<T>
    {
        public VirtualMachineRepository(string conn) : base(conn)
        {
        }
    }
}