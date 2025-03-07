using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_ConsoleApp.ViewModels
{
    public class AiModelViewModel
    {
        public string Model { get; set; }
        public List<MessageViewModel> Messages { get; set; }
        public bool Stream { get; set; }
    }
}
