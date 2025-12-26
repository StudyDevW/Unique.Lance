using AccountService.Domain.Entities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Domain.Entities
{
    public class ProfileTable : IId
    {
        public Guid user_id { get; set; }

        public string? header_url { get; set; }

        public string bio { get; set; }

        public string country { get; set; }

        public int rating { get; set; }

        public int completed_projects { get; set; }

        public DateTime? updated_at { get; set; }
    }
}
