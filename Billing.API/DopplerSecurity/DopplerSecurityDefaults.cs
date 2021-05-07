namespace Billing.API.DopplerSecurity
{
    public static class DopplerSecurityDefaults
    {
        public const string DEFAULT_OR_SIGNED_PATHS_POLICY = "DefaultOrSignedPathsPolicy";
        public const string SIGNED_PATH_SCHEME = "SignedPath";
        public const string SIGNED_PATH_CLAIM_TYPE = "SignedPath";
        public const string SIGNED_PATH_QUERY_STRING_PARAMETER_NAME = "_s";
        public const string PUBLIC_KEYS_FOLDER_CONFIG_KEY = "PublicKeysFolder";
        public const string PUBLIC_KEYS_FOLDER_DEFAULT_CONFIG_VALUE = "public-keys";
        public const string SUPERUSER_JWT_KEY = "isSU";
    }
}
