namespace Ziggio.Identity.Domain;

public static class Constants
{
    public static class Resources
    {
        public static class Ziggio
        {
            public static class Gazoond
            {
                public const string BlazorClient = "gazoond_blazor_client";
                public const string NutritionApi = "gazoond_nutrition_api";
            }

            public static class SiteBuilder
            {
                public const string ApplicationsApi = "ziggio_sitebuilder_applications_api";
                public const string BlazorClient = "ziggio_sitebuilder_blazor_client";
                public const string SitesApi = "ziggio_sitebuilder_sites_api";
            }

            public static class Testing
            {
                public const string AuthorizationCodeTest = "ziggio_authorization_code_testing";
                public const string ClientCredentialsTest = "ziggio_client_credentials_testing";
            }

            public const string PostmanClient = "ziggio_postman_client";
        }
    }

    public static class Roles
    {
        public static class Applications
        {
            public const string GroupName = "Application";
            public const string Administrator = $"{GroupName} Administrator";
            public const string Manager = $"{GroupName} Manager";
            public const string User = $"{GroupName} User";
        }

        public static class Sites
        {
            public const string GroupName = "Site";
            public const string Administrator = $"{GroupName} Administrator";
            public const string Manager = $"{GroupName} Manager";
            public const string User = $"{GroupName} User";
        }
    }
}
