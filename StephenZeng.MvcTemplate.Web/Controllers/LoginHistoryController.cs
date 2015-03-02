using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using StephenZeng.MvcTemplate.Web.Helpers;

namespace StephenZeng.MvcTemplate.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LoginHistoryController : BaseController
    {
        public async Task<ActionResult> Index(int? page)
        {
            var pageNumber = page ?? 1;

            var list = await DbContext.LoginHistories
                .OrderByDescending(l => l.AttemptTime)
                .ToPagedListAsync(pageNumber, PageSize);

            return View(list);
        }
    }
}