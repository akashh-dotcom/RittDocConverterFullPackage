#region

using System;
using System.Threading;
using System.Web.Mvc;
using R2V2.Web.Exceptions;
using R2V2.Web.Models;

#endregion

namespace R2V2.Web.Controllers
{
    public class TestErrorController : Controller
    {
        //
        // GET: /TestError/

        public ActionResult Forbidden()
        {
            throw new HttpForbiddenException("Forbidden Exception thrown");
        }

        public ActionResult NotFound()
        {
            throw new NotFoundHttpException("Not found Exception");
        }

        public ActionResult Generic500()
        {
            throw new Exception("500 exception");
        }

        public ActionResult LogTester(int delayTime)
        {
            var model = new LogTesterViewModel
            {
                DelayTime = delayTime
            };
            if (delayTime > 0)
            {
                if (delayTime <= 20000)
                {
                    Thread.Sleep(delayTime);
                    model.Messages.Add($"Page thread slept for {delayTime:#,###} ms");
                }
                else
                {
                    model.Messages.Add($"DelayTime too high: {delayTime:#,###} ms");
                }
            }

            return View(model);
        }
    }
}