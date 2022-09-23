using System.Web.Optimization;

namespace CalpineAzureDashboard.Web
{
    public static class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/portalBundle")
                   .Include("~/content/styles/portal.css")
                   .Include("~/content/styles/bootstrap.css")
                   .Include("~/content/styles/font-awesome.css")
                   .Include("~/content/styles/animate.css")
                   .Include("~/content/styles/bootstrap-switch.css")
                   .Include("~/content/styles/checkbox3.css")
                   .Include("~/content/styles/jquery.datatables.css")
                   .Include("~/content/styles/bootstrap.datatables.css")
                   .Include("~/content/styles/select2.css")
                   .Include("~/content/styles/select2-bootstrap.css")
                   .Include("~/content/styles/flat-blue.css")
                   .Include("~/content/styles/style.css")
                   .Include("~/content/styles/jquery-ui.css"));

            bundles.Add(new ScriptBundle("~/portalScriptBundle")
                .Include("~/scripts/jquery-{version}.js")
                .Include("~/scripts/jquery-ui.js")
                .Include("~/scripts/jquery.validate.js")
                .Include("~/scripts/jquery.validate.unobtrusive.js")
                .Include("~/scripts/bootstrap.js")
                .Include("~/scripts/jquery.matchHeight.js")
                .Include("~/scripts/jquery.datatables.js")
                .Include("~/scripts/bootstrap-switch.js")
                .Include("~/scripts/bootstrap.datatables.js")
                .Include("~/scripts/select2.js")
                .Include("~/scripts/es6-promise.js")
                .Include("~/scripts/chart.js")
                .Include("~/scripts/ace/ace.js")
                .Include("~/scripts/ace/mode-html.js")
                .Include("~/scripts/ace/theme-github.js")
                .Include("~/scripts/app.js")
                .Include("~/scripts/custom.js")
                .Include("~/scripts/powerbi.js"));

            bundles.Add(new StyleBundle("~/mainBundle")
                   .Include("~/content/styles/bootstrap.css")
                   .Include("~/content/styles/font-awesome.css")
                   .Include("~/content/styles/flat-blue.css")
                   .Include("~/content/styles/site.css"));

            bundles.Add(new ScriptBundle("~/mainScriptBundle")
                .Include("~/scripts/jquery-{version}.js")
                .Include("~/scripts/jquery-ui.js")
                .Include("~/scripts/jquery.validate.js")
                .Include("~/scripts/jquery.validate.unobtrusive.js")
                .Include("~/scripts/bootstrap.js"));

            BundleTable.EnableOptimizations = true;
        }
    }
}