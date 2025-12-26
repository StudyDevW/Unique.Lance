using AccountService.Domain.Entities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Domain.Entities
{
    public class UsersTable : IId
    {
        public string first_name { get; set; }

        public string? last_name { get; set; }

        public string? photo_url { get; set; }

        public string[] roles { get; set; }

        public string telegram_id { get; set; }

        public string username { get; set; }

        public string status { get; set; }

        public bool is_verified { get; set; }

        public DateTime created_at { get; set; }

        public DateTime? last_login { get; set; }
    }
}
