using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelingSalesman
{
    public class PopulationMember
    {
        public List<float[]> Path { get; set; }

        public float TotalDistance { get; set; }

        public int ReproductionWeight { get; set; }

        public int DeathWeight { get; set; }
    }
}
