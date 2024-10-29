using Equifax.Api.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Equifax.Api.Domain.Models
{
    public class RequestMaster
    {
        [Key]
        public int RequestId { get; set; }
        public string user_name { get; set; } = string.Empty;
        public string user_password { get; set; } = string.Empty;
        public int client_id { get; set; }
        public string dispute_type { get; set; } = string.Empty;
        public int credit_repair_id { get; set; }
        public string creditor_name { get; set; }
        public string account_number { get; set; }
        public string credit_balance { get; set; }
        public string open_date { get; set; }
        public string creditor { get; set; }
        public string ownership { get; set; }
        public string accuracy { get; set; }
        public string comment { get; set; }
        public string file_number { get; set; }
        public DateTime estimated_completion_date { get; set; }
        public DateTime submitted_date { get; set; }
        public string request_status { get; set; } = "InProgress";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }
    }
}
