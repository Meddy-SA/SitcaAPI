using Sitca.DataAccess.Data.Repository.IRepository;
using Sitca.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
    public IActionResult GetAll()
    {
      //return Json(new { data = _unitOfWork.ItemTemplate.GetAll() });
      return Json(_unitOfWork.ItemTemplate.GetAll());
    }


    [HttpGet]
    public async Task<IActionResult> AsyncGetAll()
    {

      return Json(new { data = await _unitOfWork.ItemTemplate.GetAllAsync() });
    }

    [HttpDelete]
    public IActionResult Delete(int id)
    {
      var objFromDb = _unitOfWork.ItemTemplate.Get(id);

      if (objFromDb == null)
      {
        return Json(new { success = false, message = "Error" });


      }
      _unitOfWork.ItemTemplate.Remove(objFromDb);
      _unitOfWork.Save();
      return Json(new { success = true, message = "delete success" });
    }

    [HttpPost]
    public IActionResult Upsert(ItemTemplate itemTemplate)
    {
      if (ModelState.IsValid)
      {
        _unitOfWork.ItemTemplate.Add(itemTemplate);
        _unitOfWork.Save();
      }
      return null;
    }

  }
}
