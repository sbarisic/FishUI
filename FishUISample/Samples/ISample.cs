using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishUISample.Samples
{
    internal interface ISample
    {
        public FishUI.FishUI CreateUI();

        public void Init();
    }
}
