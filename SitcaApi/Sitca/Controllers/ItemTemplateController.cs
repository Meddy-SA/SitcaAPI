using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Models;

namespace Sitca.Controllers
{
    public class ItemTemplateController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public ItemTemplateController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Json(await _unitOfWork.ItemTemplate.GetAll());
        }

        [HttpGet]
        public async Task<IActionResult> AsyncGetAll()
        {
            return Json(new { data = await _unitOfWork.ItemTemplate.GetAllAsync() });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var objFromDb = await _unitOfWork.ItemTemplate.Get(id);

            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error" });
            }
            _unitOfWork.ItemTemplate.Remove(objFromDb);
            _unitOfWork.SaveChanges();
            return Json(new { success = true, message = "delete success" });
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(ItemTemplate itemTemplate)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.ItemTemplate.Add(itemTemplate);
                _unitOfWork.SaveChanges();
            }
            return null;
        }
    }
}
