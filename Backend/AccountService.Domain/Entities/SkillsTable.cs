using AccountService.Domain.Entities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Domain.Entities
{
    public class SkillsTable : IId
    {
        public Guid user_id { get; set; }

        public string name { get; set; }

        public string level { get; set; }

        public bool verified { get; set; }

        public int years_of_expirience { get; set; }
    }
}
