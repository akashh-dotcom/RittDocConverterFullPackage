#region

using System.Web.Mvc;
using System.Web.Script.Serialization;
using R2V2.Core.Tabers;
using R2V2.Web.Controllers.SuperTypes;

#endregion

namespace R2V2.Web.Controllers
{
    public class TabersDictionaryController : R2BaseController
    {
        private readonly ITabersDictionaryService _tabersDictionaryService;

        public TabersDictionaryController(ITabersDictionaryService tabersDictionaryService)
        {
            _tabersDictionaryService = tabersDictionaryService;
        }

        public ActionResult Entry(string term)
        {
            return ContentResult(_tabersDictionaryService.GetMainEntry(term));
        }

        public ActionResult TermContent(int? termId)
        {
            var termContent = _tabersDictionaryService.GetTermContent(termId ?? 0);
            return ContentResult(termContent);
        }

        private static ContentResult ContentResult(object obj)
        {
            return
                new ContentResult
                {
                    ContentType = "application/json",
                    Content = new JavaScriptSerializer { MaxJsonLength = int.MaxValue, RecursionLimit = 100 }
                        .Serialize(obj)
                };
        }
    }
}