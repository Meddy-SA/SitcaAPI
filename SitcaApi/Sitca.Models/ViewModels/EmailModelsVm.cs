using System;
using System.Collections.Generic;
using System.Text;

namespace Sitca.Models.ViewModels
{
    class EmailModelsVm
    {
    }

    public class LoginMailVm
    {
        public string Url { get; set; }
        public RegisterVm UserData { get; set; }
    }
}
