﻿using Coreflow.Interfaces;
using Coreflow.Objects;
using MySql.Data.MySqlClient;

namespace Coreflow.Activities.MySQL
{
    [DisplayMeta("Execute Non Query", "MySQL", "fa-database")]
    public class ExecuteNonQuery : ICodeActivity
    {
        public int Execute(MySqlConnection Connection, string Query)
        {
            MySqlCommand cmd = Connection.CreateCommand();
            cmd.CommandText = Query;

            return cmd.ExecuteNonQuery();
        }
    }
}
