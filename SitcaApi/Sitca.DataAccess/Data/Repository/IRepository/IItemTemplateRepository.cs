using Sitca.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;//para async

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface IItemTemplateRepository: IRepository<ItemTemplate>
    {
        IEnumerable<SelectListItem> GetITemForDropDown();


        //https://www.youtube.com/watch?v=QU2QzPDh3-I async
        Task<List<ItemTemplate>> GetAllAsync();
    }
}
