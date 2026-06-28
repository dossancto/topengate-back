namespace Anv;

public static partial class AppEnv
{
    public static partial class DATABASES
    {
        public static partial class POSTGRES
        {
            public static partial class MAIN_DB
            {
                public static readonly AnvEnv CONNECTION_STRING = new("DATABASES__POSTGRES__MAIN_DB__CONNECTION_STRING");
            }
        }
    }
    public static readonly AnvEnv OTEL_SERVICE_NAME = new("OTEL_SERVICE_NAME");
    public static readonly AnvEnv OTEL_EXPORTER_OTLP_ENDPOINT = new("OTEL_EXPORTER_OTLP_ENDPOINT");
    public static readonly AnvEnv OTEL_EXPORTER_OTLP_HEADERS = new("OTEL_EXPORTER_OTLP_HEADERS");
    public static readonly AnvEnv OTEL_ATTRIBUTE_VALUE_LENGTH_LIMIT = new("OTEL_ATTRIBUTE_VALUE_LENGTH_LIMIT");
    public static readonly AnvEnv OTEL_EXPORTER_OTLP_COMPRESSION = new("OTEL_EXPORTER_OTLP_COMPRESSION");
    public static readonly AnvEnv OTEL_EXPORTER_OTLP_PROTOCOL = new("OTEL_EXPORTER_OTLP_PROTOCOL");
    public static readonly AnvEnv OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE = new("OTEL_EXPORTER_OTLP_METRICS_TEMPORALITY_PREFERENCE");
    public static partial class APP
    {
        public static partial class AUTHENTICATION
        {
            public static partial class JWT
            {
                public static readonly AnvEnv KEY = new("APP__AUTHENTICATION__JWT__KEY");
                public static readonly AnvEnv ISSUER = new("APP__AUTHENTICATION__JWT__ISSUER");
                public static readonly AnvEnv AUDIENCE = new("APP__AUTHENTICATION__JWT__AUDIENCE");
            }
        }
        public static partial class INTEGRATIONS
        {
            public static partial class PROVIDERS
            {
                public static partial class EMAIL_SENDERS
                {
                    public static partial class LOGGER
                    {
                        /// <summary>
                        /// Represents if should be enabled the logger email sender. Disable this option in production since user cant see logs
                        /// </summary>
                        public static readonly AnvEnv ENABLED = new("APP__INTEGRATIONS__PROVIDERS__EMAIL_SENDERS__LOGGER__ENABLED");
                    }
                    public static partial class BREVO
                    {
                        public static readonly AnvEnv BASEURL = new("APP__INTEGRATIONS__PROVIDERS__EMAIL_SENDERS__BREVO__BASEURL");
                        public static readonly AnvEnv APIKEY = new("APP__INTEGRATIONS__PROVIDERS__EMAIL_SENDERS__BREVO__APIKEY");
                        public static partial class SENDER
                        {
                            public static readonly AnvEnv EMAIL = new("APP__INTEGRATIONS__PROVIDERS__EMAIL_SENDERS__BREVO__SENDER__EMAIL");
                            public static readonly AnvEnv NAME = new("APP__INTEGRATIONS__PROVIDERS__EMAIL_SENDERS__BREVO__SENDER__NAME");
                        }
                    }
                    public static partial class RESEND
                    {
                        public static readonly AnvEnv BASEURL = new("APP__INTEGRATIONS__PROVIDERS__EMAIL_SENDERS__RESEND__BASEURL");
                        public static readonly AnvEnv APIKEY = new("APP__INTEGRATIONS__PROVIDERS__EMAIL_SENDERS__RESEND__APIKEY");
                        public static partial class SENDER
                        {
                            public static readonly AnvEnv EMAIL = new("APP__INTEGRATIONS__PROVIDERS__EMAIL_SENDERS__RESEND__SENDER__EMAIL");
                        }
                    }
                }
            }
        }
    }
}