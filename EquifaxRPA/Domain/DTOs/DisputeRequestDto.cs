﻿using System;
using System.Collections.Generic;

namespace Equifax.Api.Domain.DTOs
{
    public class DisputeRequestDto
    {
        public string user_name { get; set; }
        public string user_password { get; set; }
        public int client_id { get; set; }
        public string dispute_type { get; set; }
        public EquifaxDataDto equifax_data { get; set; }
    }

    public class EquifaxDataDto
    {
        public List<AccountDto> account { get; set; }
        public List<CollectionDto> collection { get; set; }
        public List<InquiryDto> inquiries { get; set; }
    }

    public class AccountDto
    {
        public int credit_repair_id { get; set; }
        public string creditor_name { get; set; }
        public string account_number { get; set; }
        public string credit_balance { get; set; }
        public string open_date { get; set; }
        public string creditor { get; set; }
        public string ownership { get; set; }
        public List<string> accuracy { get; set; }
        public string comment { get; set; }
        public string file_number { get; set; }
        public DateTime estimated_completion_date { get; set; }
        public DateTime submitted_date { get; set; }
    }

    public class CollectionDto
    {
        public int credit_repair_id { get; set; }
        public string creditor_name { get; set; }
        public string account_number { get; set; }
        public string credit_balance { get; set; }
        public string open_date { get; set; }
        public string creditor { get; set; }
        public string ownership { get; set; }
        public string issueOption { get; set; }
        public string accuracy { get; set; }
        public string comment { get; set; }
        public string file_number { get; set; }
    }

    public class InquiryDto
    {
        // Add properties when necessary
    }
}
