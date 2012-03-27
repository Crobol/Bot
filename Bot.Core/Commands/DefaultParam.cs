using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bot.Core.Commands
{
    class DefaultParam
    {
        [DefaultOption]
        [OptionFullName("query")]
        public string Query { get; set; }
    }
}
