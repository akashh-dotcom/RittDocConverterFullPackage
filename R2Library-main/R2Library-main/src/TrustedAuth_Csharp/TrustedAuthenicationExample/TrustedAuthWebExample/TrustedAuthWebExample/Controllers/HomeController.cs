using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using TrustedAuthWebExample.Helper;

namespace TrustedAuthWebExample.Controllers
{
    public class HomeController : Controller
    {
        private static readonly List<InstitutionModel> Institutions = new List<InstitutionModel>
        {
            new InstitutionModel {AccountNumber = "022021", Name = "CHESTER COUNTY HOSPITAL", TrustedKey = "uh7mANFBDZUFXcbD"},
            new InstitutionModel {AccountNumber = "018752", Name = "HUSSON UNIVERSITY", TrustedKey = "BwfQ4UIXdulyjEXT"},
            new InstitutionModel {AccountNumber = "058872", Name = "LA ROCHE COLLEGE", TrustedKey = "WRUkApUza2u5ac7b"},
            new InstitutionModel {AccountNumber = "025154", Name = "MADIGAN ARMY MEDICAL CENTER", TrustedKey = "fosJic7VowVkh8Bf"},
            new InstitutionModel {AccountNumber = "005033", Name = "NEW RITTENHOUSE.COM BMD", TrustedKey = "wMHDflpGLE9ivsju"},
            new InstitutionModel {AccountNumber = "005034", Name = "NEW RITTENHOUSE.COM LHN", TrustedKey = "gu4tPPnyojPFpXdN"},
            new InstitutionModel {AccountNumber = "014296", Name = "NORTHERN VIRGINIA COMMUNITY COLLEGE", TrustedKey = "ecKdkk47bCELgUsk"},
            new InstitutionModel {AccountNumber = "001037", Name = "ROWAN-CABARRUS COMMUNITY COLLLEGE LIBRARY", TrustedKey = "ERmLuFpkPTejGXzw"},
            new InstitutionModel {AccountNumber = "058866", Name = "SALEM REGIONAL MEDICAL CENTER", TrustedKey = "Sal3mCommun!ty11"},
            new InstitutionModel {AccountNumber = "088349", Name = "THOMAS NELSON COMMUNITY COLLEGE LIBRARY", TrustedKey = "9StpfzbHsJpDCmcs"},
            new InstitutionModel {AccountNumber = "092586", Name = "UNIONTOWN HOSPITAL", TrustedKey = "9sT4zLx2EhcuxSm0"}
        };


        // GET: Home
        public ActionResult Index()
        {
            var items = new List<SelectListItem>();
            foreach (InstitutionModel institutionModel in Institutions)
            {
                items.Add(new SelectListItem {Text = $"{institutionModel.AccountNumber} - {institutionModel.Name} - {institutionModel.TrustedKey}", Value = institutionModel.AccountNumber});
            }

            IndexModel model = new IndexModel {Institutions = new SelectList(items, "Value", "Text"), R2Url = "https://local-dev.r2library.com/Browse" };

            return View(model);
        }

        public ActionResult SamplePage(string accountNumber, string r2Url)
        {
            R2LibraryTrustedAuth trustedAuth = new R2LibraryTrustedAuth();
            string timestamp = trustedAuth.GetTimeStamp();

            InstitutionModel institutionModel = Institutions.FirstOrDefault(x => x.AccountNumber == accountNumber);

            string hash = trustedAuth.GetHashKey(accountNumber, institutionModel.TrustedKey);
            string r2Link = $"{r2Url}?acctno={accountNumber}&timestamp={timestamp}&hash={hash}";

            SamplePageModel model = new SamplePageModel
            {
                Url = r2Link
            };

            return View(model);
        }

        /// <summary>
        /// Test page for testing referrer
        /// </summary>
        public ActionResult ReferrerAuthTest()
        {
            return View();
        }
    }

    public class IndexModel
    {
        public SelectList Institutions { get; set; }
        public string AccountNumber { get; set; }
        public string R2Url { get; set; }
    }

    public class InstitutionModel
    {
        public string AccountNumber { get; set; }
        public string Name { get; set; }
        public string TrustedKey { get; set; }
    }

    public class SamplePageModel
    {
        public string Url { get; set; }
    }
}