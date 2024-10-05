using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Sitca.Models.ViewModels
{
    [NotMapped]
    public class CommonVm
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool isSelected { get; set; }
    }


    [NotMapped]
    public class CommonUserVm
    {
        public string id { get; set; }
        public string email { get; set; }

        public string fullName { get; set; }
        public string phone { get; set; }

        public string codigo { get; set; }

    }
}
