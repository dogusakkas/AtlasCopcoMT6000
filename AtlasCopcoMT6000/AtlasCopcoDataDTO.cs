using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlasCopcoMT6000
{
    public class AtlasCopcoDataDTO
    {
        
        public string ControllerName { get; set; }

        public string TighteningId { get; set; }
        public string Torque { get; set; }
        public string TorqueSts { get; set; }
        public string Angle { get; set; }
        public string AngleSts { get; set; }
        public string Time { get; set; }

    }
}
