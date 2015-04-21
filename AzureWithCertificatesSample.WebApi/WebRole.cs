using System;
using System.Diagnostics;
using Microsoft.Web.Administration;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace AzureWithCertificatesSample.WebApi
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            try
            {
                using (var server = new ServerManager())
                {
                    const string site = "Web";

                    var siteName = string.Format("{0}_{1}", RoleEnvironment.CurrentRoleInstance.Id, site);
                    var config = server.GetApplicationHostConfiguration();
                    ConfigureAccessSection(config, siteName);

                    var iisClientCertificateMappingAuthenticationSection = EnableIisClientCertificateMappingAuthentication(config, siteName);
                    ConfigureManyToOneMappings(iisClientCertificateMappingAuthenticationSection);
                    ConfigureOneToOneMappings(iisClientCertificateMappingAuthenticationSection);

                    server.CommitChanges();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                // handle error here
            }

            return base.OnStart();
        }

        private static void ConfigureAccessSection(Configuration config, string siteName)
        {
            var accessSection = config.GetSection("system.webServer/security/access", siteName);
            accessSection["sslFlags"] = @"Ssl,SslNegotiateCert,SslRequireCert";
        }

        private static void ConfigureOneToOneMappings(ConfigurationSection iisClientCertificateMappingAuthenticationSection)
        {
            var oneToOneMappings = iisClientCertificateMappingAuthenticationSection.GetCollection("oneToOneMappings");

            var oneToOneElement = oneToOneMappings.CreateElement("add");
            oneToOneElement["enabled"] = true;
            oneToOneElement["userName"] = @"ValidUsernameOfServerUser";
            oneToOneElement["password"] = @"PasswordForTheValidUser";
            oneToOneElement["certificate"] = @"Base64-Encoded-Certificate-Data";

            oneToOneMappings.Add(oneToOneElement);
        }

        private static ConfigurationSection EnableIisClientCertificateMappingAuthentication(Configuration config, string siteName)
        {
            var iisClientCertificateMappingAuthenticationSection = config.GetSection("system.webServer/security/authentication/iisClientCertificateMappingAuthentication", siteName);
            iisClientCertificateMappingAuthenticationSection["enabled"] = true;
            return iisClientCertificateMappingAuthenticationSection;
        }

        private static void ConfigureManyToOneMappings(ConfigurationSection iisClientCertificateMappingAuthenticationSection)
        {
            var manyToOneMappings = iisClientCertificateMappingAuthenticationSection.GetCollection("manyToOneMappings");

            var element = manyToOneMappings.CreateElement("add");
            element["name"] = @"Allowed Clients";
            element["enabled"] = true;
            element["permissionMode"] = @"Allow";
            element["userName"] = @"ValidUsernameOfServerUser";
            element["password"] = @"PasswordForTheValidUser";

            manyToOneMappings.Add(element);

            ConfigureCertificateRules(element);
        }

        private static void ConfigureCertificateRules(ConfigurationElement element)
        {
            var rulesCollection = element.GetCollection("rules");

            var issuerRule = rulesCollection.CreateElement("add");
            issuerRule["certificateField"] = @"Issuer";
            issuerRule["certificateSubField"] = @"CN";
            issuerRule["matchCriteria"] = @"CARoot";
            issuerRule["compareCaseSensitive"] = true;

            var subjectRule = rulesCollection.CreateElement("add");
            subjectRule["certificateField"] = @"Subject";
            subjectRule["certificateSubField"] = @"CN";
            subjectRule["matchCriteria"] = @"ClientCert";
            subjectRule["compareCaseSensitive"] = true;

            rulesCollection.Add(issuerRule);
            rulesCollection.Add(subjectRule);
        }
    }
}