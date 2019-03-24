using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HrothgarData.Repositories
{
    public class AccountRepository
    {
        public Accounts GetAccountByUsernameAndPassword(string username, string password)
        {
            using (var ctx = new hrothgarContext())
            {
                var results = ctx.Accounts.Where(x => x.Name == username && x.Password == password).ToList();
                if(results.Any())
                {
                    return results.First();
                }
                return null;
            }
        }
    }
}
