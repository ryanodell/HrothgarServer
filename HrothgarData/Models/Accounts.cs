using System;
using System.Collections.Generic;

namespace HrothgarData
{
    public partial class Accounts
    {
        public Accounts()
        {
            Characters = new HashSet<Characters>();
        }

        public int AccountId { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public int Plvl { get; set; }
        public int MaxPlvl { get; set; }

        public virtual ICollection<Characters> Characters { get; set; }
    }
}
