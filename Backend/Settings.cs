﻿using System;
using System.Collections.Generic;
using LinqToDB.Configuration;

namespace Backend
{
    public class Settings : ILinqToDBSettings
    {
        public string BaseUrl { get; set; } =
#if DEBUG
            "http://localhost:4201";
#else
            "http://gis.hahn-kev.com";
#endif
        public string ConnectionString { get; set; }

        public string MailgunApiKey { get; set; }
        public string MailgunDomain { get; set; }
        public string Environment { get; set; }
        public string SentryDsn { get; set; }
        public string SendGridAPIKey { get; set; }
        public string GoogleAPIKey { get; set; }

        public IEnumerable<IDataProviderSettings> DataProviders
        {
            get { yield break; }
        }

        public string DefaultConfiguration => "PostGreSQL";
        public string DefaultDataProvider => "PostgreSQL";

        public IEnumerable<IConnectionStringSettings> ConnectionStrings
        {
            get
            {
                yield return new ConnectionStringSettings
                {
                    Name = DefaultConfiguration,
                    ProviderName = DefaultDataProvider,
                    ConnectionString = FormatConnectionString(ConnectionString)
                };
            }
        }

        public static string FormatConnectionString(string connectionString)
        {
            try
            {
                var uri = new Uri(connectionString);
                var db = uri.AbsolutePath.Trim('/');
                var user = uri.UserInfo.Split(':')[0];
                var passwd = uri.UserInfo.Split(':')[1];
                var port = uri.Port > 0 ? uri.Port : 5432;
                return $"Server={uri.Host};Database={db};User Id={user};Password={passwd};Port={port}";
            }
            catch (UriFormatException)
            {
                return connectionString;
            }
        }
    }

    public class ConnectionStringSettings : IConnectionStringSettings
    {
        public string ConnectionString { get; set; }
        public string Name { get; set; }
        public string ProviderName { get; set; }
        public bool IsGlobal => false;
    }

    public class TemplateSettings
    {
        public string NotifyLeaveRequest { get; set; }
        public string RequestLeaveApproval { get; set; }
        public string NotifyHrLeaveRequest { get; set; }
        public string NotifyLeaveApproved { get; set; }
    }
}