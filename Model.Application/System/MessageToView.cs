using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Application.System
{
    public class MessageToView
    {
        public string Title { get; set; }
        public MessageInfo Type { get; set; }
        public List<string> Messages { get; set; }
        public bool ShowInNexPage { get; set; }
    }
}