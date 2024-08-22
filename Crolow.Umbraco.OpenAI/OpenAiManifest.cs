using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.Manifest;

namespace Umbraco.Crolow.OpenAi;
public class OpenAiManifest : IManifestFilter
{
    public void Filter(List<PackageManifest> manifests)
    {
        manifests.Add(new PackageManifest
        {
            PackageName = "Umbraco OpenAI",
            Version = "1.0.6",
            ContentApps = new ManifestContentAppDefinition[]
            {
                new ManifestContentAppDefinition
                {
                  Name = "AI Generator",
                  Alias = "AIGenerator",
                  Weight= 0,
                  Icon= "icon-calculator",
                  View= "/App_Plugins/CrolowAI/views/AIContentApp.html",
                  Show= new [] { "+content/*" }
                }
            },
            Dashboards = new ManifestDashboard[]
            {
                new ManifestDashboard
                {
                    Alias ="crolowAIDashboard",
                    View = "/App_Plugins/CrolowAI/Views/AIDashboard.html",
                    Sections = new [] { "content" },
                    Weight =  -10,
                    AccessRules = new IAccessRule[]
                    {
                       new AccessRule() { Type = AccessRuleType.Grant,  Value = "admin" }
                    }
                }
            },
            Stylesheets = new[]
            {
                "/App_Plugins/CrolowAI/css/crolowai.css"
            },
            Scripts = new[]
            {
                "/App_Plugins/CrolowAI/js/CrolowAI.ContentApp.Controller.js",
                "/App_Plugins/CrolowAI/js/CrolowAI.Controller.js",
                "/App_Plugins/CrolowAI/js/CrolowAI.Resources.js"
            }
        });
    }
}