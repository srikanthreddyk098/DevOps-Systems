using System;

namespace CalpineAzureDashboard.Models
{
    public class BaseModel
    {
        public int? Id { get; set; }

        public bool IsDateEqual(DateTime? date1, DateTime? date2)
        {
            if (date1.HasValue && date2.HasValue)
            {
                return date1 == date2 || date1 - date2 < TimeSpan.FromSeconds(1);
            }

            return !date1.HasValue && !date2.HasValue;
        }

        public bool IsDoubleEqual(double? d1, double? d2)
        {
            if (d1.HasValue && d2.HasValue)
            {
                return Math.Abs(d1.Value - d2.Value) < 0.01;
            }

            return !d1.HasValue && !d2.HasValue;
        }
    }
}