﻿using _203.UMS.Data.Contracts;
using _203.UMS.Models.Config;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Web.Configuration;

namespace _203.UMS.Web.Config
{
    public class Connections
    {
        private readonly IRepoUow _repo;
        private readonly Configuration _conf;

        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        public Connections(IRepoUow uow)
        {
            _repo = uow;
            _conf = WebConfigurationManager.OpenWebConfiguration("~");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Config"/> class.
        /// </summary>
        /// <param name="path">The path to the configuration file.</param>
        public Connections(string path)
        {
            _conf = File.Exists(path)
                ? ConfigurationManager.OpenExeConfiguration(path)
                : WebConfigurationManager.OpenWebConfiguration(path);
        }

        public DatabaseSetting GetDatabaseSettings()
        {
            var conf = new SqlConnectionStringBuilder { ConnectionString = _conf.ConnectionStrings.ConnectionStrings["AppDb"].ConnectionString };

            return new DatabaseSetting
                         {
                             Server = conf.DataSource,
                             Catalog = conf.InitialCatalog,
                             IntegratedSecurity = conf.IntegratedSecurity,
                             Username = conf.UserID,
                             Password = conf.Password
                         };
        }

        public bool SetDatabaseSettings(DatabaseSetting db)
        {
            var conf = new SqlConnectionStringBuilder
                           {
                               DataSource = db.Server,
                               InitialCatalog = db.Catalog,
                               IntegratedSecurity = db.IntegratedSecurity
                           };

            if (!db.IntegratedSecurity)
            {
                conf.UserID = db.Username;
                conf.Password = db.Password;
            }

            try
            {
                _conf.ConnectionStrings.ConnectionStrings["AppDb"].ConnectionString = conf.ConnectionString;

                SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public DirectorySetting GetDirectorySettings()
        {
            return GetDirectorySettings(false);
        }

        public DirectorySetting GetDirectorySettings(bool pass)
        {
            var s = new Settings(_repo);
            return new DirectorySetting
                       {
                           Directory = s.Get<string>("LdapPath"),
                           Username = s.Get<string>("LdapUser"),
                           Password = pass ? s.Get<string>("LdapPass") : ""
                       };
        }

        public bool SetDirectorySettings(DirectorySetting dir)
        {
            var s = new Settings(_repo);
            s.Put("LdapUser", dir.Username);
            s.Put("LdapPass", dir.Password, true);
            return true;
        }

        public void SaveChanges()
        {
            _conf.Save();
        }
    }
}