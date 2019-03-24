using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HrothgarData.Repositories
{
    public class CharacterRepository
    {
        public List<Characters> GetCharactersForAccount(int accountId)
        {
            using (var ctx = new hrothgarContext())
            {
                return ctx.Characters.Where(x => x.AccountId == accountId).ToList();
            }
        }
    }
}
