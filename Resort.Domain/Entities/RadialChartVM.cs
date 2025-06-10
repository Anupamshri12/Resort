using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Resort.Domain.Entities
{
    public class RadialChartVM
    {
        public decimal TotalCount { get; set; }
        public decimal IncreaseDecreaseAmount { get; set; }
        public bool HasRationIncreased { get; set; }
        public int series { get; set; }

        public int[] Serial { get; set; }
        public string []Labels{get;set ;}
        public List<LineChart> seriesal { get; set; }
        public string[] Collections { get; set; }
    }
    public class LineChart
    {
        public string Name { get; set; }
        public int[] data { get; set; }
    }
}
