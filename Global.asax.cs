using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using WAPP.Utils; // Added to access SystemLogService and LogLevel

namespace WAPP
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // --- GLOBAL CRASH SAFETY NET ---
        void Application_Error(object sender, EventArgs e)
        {
            // 1. Grab the exception that just crashed the page
            Exception ex = Server.GetLastError();

            if (ex != null)
            {
                // 2. Ignore 404s (Page Not Found) from triggering massive CRITICAL alerts.
                // We'll log them as WARNINGS instead, so you can fix broken links later.
                if (ex is HttpException httpEx && httpEx.GetHttpCode() == 404)
                {
                    SystemLogService.Write("HTTP_404_NOT_FOUND", $"Missing page requested: {Request.Url}", LogLevel.WARNING);
                    return;
                }

                // 3. Drill down to the actual root cause of the crash
                Exception rootEx = ex.GetBaseException();

                // 4. Safely try to figure out WHO experienced the crash
                int? currentUserId = null;
                try
                {
                    if (Context != null && Context.Session != null && Context.Session["UserId"] != null)
                    {
                        currentUserId = Convert.ToInt32(Context.Session["UserId"]);
                    }
                }
                catch
                {
                    // If the session itself is what crashed, just ignore and log it as an anonymous error
                }

                // 5. ---> LOGGING ADDED: CRITICAL (The Safety Net)
                // This logs the exact URL that broke and the specific error message.
                string errorMessage = $"Unhandled App Crash on {Request.Path}: {rootEx.Message}";

                SystemLogService.Write("APP_CRASH_CRITICAL", errorMessage, LogLevel.CRITICAL, currentUserId);

                // Note: We are NOT calling Server.ClearError() here. 
                // We want ASP.NET to continue its normal process so it can redirect the user 
                // to your custom error page (if you have one set up in Web.config).
            }
        }
    }

}