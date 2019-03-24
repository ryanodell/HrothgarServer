using System;
using System.Collections.Generic;

namespace HrothgarData
{
    public partial class Characters
    {
        public int CharacterId { get; set; }
        public int AccountId { get; set; }
        public string Name { get; set; }

        public virtual Accounts Account { get; set; }
    }
}
