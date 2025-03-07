using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI_ConsoleApp.ViewModels
{
    public class ResponseViewModel
    {
        public string Model { get; set; }
        public string Done_reason { get; set; }
        public bool Done { get; set; }
        public long  Total_duration { get; set; }
        public long Load_duration { get; set; }
        public long Prompt_eval_count { get; set; }
        public long Prompt_eval_duration { get; set; }
        public long Eval_count { get; set; }
        public long Eval_duration { get; set; }
        public MessageViewModel Message { get; set; }
    }
}
