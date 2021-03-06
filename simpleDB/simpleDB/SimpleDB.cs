﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;

namespace SimpleDB
{

    public class DbContext
    {
        public string ConnectionString { get; set; }
        public string SQL { get; set; }
        public MyDictionary<string, string> Params { get; set; }
        public bool isStoredProc { get; set; }

        public DbContext()
        {
        }
    }

    public class SearchResults
    {
        public int interval { get; set; }
        public int assigned_page { get; set; }
        public string orderby { get; set; }
        public int totalRecords { get; set; }
        public int pages { get; set; }
        public int nextPage { get; set; }
        public int prevPage { get; set; }
        public string pagination { get; set; }
        public string rows { get; set; }
        public string query { get; set; }

        public SearchResults()
        {
            assigned_page = 0;
            totalRecords = 0;
            interval = 25;
        }
        
    }

    public class MySqlDatabase
    {
        /* ----------------------------------------------------------------------------  
                Description: Get 1 row of data back in a Name Value Pairs MyDictionary...
                Author: Mark Higbee 
                Usage: 
            
                MyDictionary<string, string> Params = new MyDictionary<string, string>();
                Params.Add("Name", "Value");

                MyDictionary<string, string> Results = new MyDictionary<string, string>();
                Results = Database.query(SQL, Params, true);
          
            ---------------------------------------------------------------------------------*/
        public static MyDictionary<string, string> query(DbContext context)
        {
            MyDictionary<string, string> Results = new MyDictionary<string, string>();

            using (MySqlConnection conn = new MySqlConnection(context.ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    if (context.isStoredProc)
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                    }

                    cmd.CommandText = context.SQL;
                    cmd.Connection = conn;

                    foreach (var pair in context.Params)
                    {
                        if (pair.Value != null && pair.Value.StartsWith("@"))
                        {
                            cmd.Parameters.AddWithValue("@" + pair.Key, pair.Value).Direction = ParameterDirection.Output;
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@" + pair.Key, pair.Value);
                        }
                    }

                    try
                    {
                        if (conn.State != ConnectionState.Open)
                        {
                            cmd.Connection.Open();
                        }
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                for (var i = 0; i < reader.FieldCount; i++)
                                {
                                    if (!reader.IsDBNull(i))
                                    {
                                        Results.Add(reader.GetName(i), reader.GetValue(i).ToString());
                                    }
                                    else
                                    {
                                        Results.Add(reader.GetName(i), "N/A");
                                    }
                                }
                            }
                            reader.Close();
                        }
                    }
                    catch (Exception error)
                    {
                        Results["ErrorMsg"] = error.ToString();
                        throw new Exception(error.ToString());
                    }

                }
            }

            return Results;
        }

        /* ---------------------------------------------------------------------------- 
           Description: get multiple rows of data back in a List of Name Value Pairs ...
           Author: Mark Higbee
           Usage: 
            
           MyDictionary<string, string> Params = new MyDictionary<string, string>();
           Params.Add("Name", "Value");

           List<MyDictionary<string, string>> Results = new List<MyDictionary<string, string>>();
           Results = Database.whileQuery(SQL,Params, true);
          
         --------------------------------------------------------------------------------*/
        public static List<MyDictionary<string, string>> whileQuery(DbContext context)
        {
            List<MyDictionary<string, string>> Results = new List<MyDictionary<string, string>>();

            using (MySqlConnection conn = new MySqlConnection(context.ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    if (context.isStoredProc)
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                    }

                    cmd.CommandText = context.SQL;
                    cmd.Connection = conn;

                    foreach (var pair in context.Params)
                    {
                        cmd.Parameters.AddWithValue("@" + pair.Key, pair.Value);
                    }

                    if (conn.State != ConnectionState.Open)
                    {
                        cmd.Connection.Open();
                    }
                    try
                    {

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MyDictionary<string, string> Row = new MyDictionary<string, string>();

                                //if (reader.Read())
                                //{
                                for (var i = 0; i < reader.FieldCount; i++)
                                {
                                    if (!reader.IsDBNull(i))
                                    {
                                        Row.Add(reader.GetName(i), reader.GetValue(i).ToString());
                                    }
                                    else
                                    {
                                        Row.Add(reader.GetName(i), "N/A");
                                    }
                                }
                                // }

                                Results.Add(Row);
                            }
                            reader.Close();
                        }
                    }
                    catch (Exception error)
                    {
                        MyDictionary<string, string> Row = new MyDictionary<string, string>();
                        Row.Add("ErrorMsg", error.ToString());
                        Results.Add(Row);
                        throw new Exception(error.ToString());
                    }

                }
            }
            return Results;
        }

        /* ---------------------------------------------------------------------------- 
          Description: 
          Author: Mark Higbee 
          Usage: 
         
          Use for Inserts,Updates, Deletes
         
          MyDictionary<string, string> Params = new MyDictionary<string, string>();
          Params.Add("Name", "Value");

          List<MyDictionary<string, string>> Results = new List<MyDictionary<string, string>>();
          Results = Database.insert(SQL,Params,true);
          
        --------------------------------------------------------------------------------*/
        public static MyDictionary<string, string> executeCmd(DbContext context)
        {
            MyDictionary<string, string> Results = new MyDictionary<string, string>();

            using (MySqlConnection conn = new MySqlConnection(context.ConnectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    if (context.isStoredProc)
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                    }

                    cmd.CommandText = context.SQL;
                    cmd.Connection = conn;
                    cmd.CommandTimeout = 0;

                    foreach (var pair in context.Params)
                    {
                        if (pair.Value != null && pair.Value.StartsWith("@"))
                        {
                            cmd.Parameters.AddWithValue("@" + pair.Key, pair.Value).Direction = ParameterDirection.Output;
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@" + pair.Key, pair.Value);
                        }
                    }

                    try
                    {
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        try
                        {
                          cmd.ExecuteNonQuery();
                        }
                        catch (MySqlException ex)
                        {
                            Results["ErrorMsg"] = ex.Message.ToString();
                        }
                    }
                    catch (Exception error)
                    {
                        Results["ErrorMsg"] = error.ToString();
                    }
                }
            }

            return Results;
        }

        public static MyDictionary<string, string> save(DbContext context, string table)
        {
            var d = new DbContext(context.ConnectionString);
            d.SQL = "DESC " + table;
            var results = whileQuery(d);

            string placeHolders = "";
            string primaryKey = "";
            string primaryValue = "";
            string primary = "";
            string update = "";
            bool hasPrimary = false;

            List<string> keys = new List<string>();
            List<string> values = new List<string>();

            foreach ( var item in results )
            {
                if(item["Key"].Equals("PRI"))
                {
                    primaryKey = item["Field"];
                    primaryValue = context.Params[item["Field"]];
                    primary = " "+ primaryKey + "='" + primaryValue + "' AND";

                    if (!primaryValue.Equals(""))
                    {
                        hasPrimary = true;
                    }
                }

                if (!context.Params.ContainsKey(item["Field"]))
                {
                    continue;
                }
                if(item["Type"].Equals("date") || item["Type"].Equals("timestamp"))
                {
                    continue;
                }

                if( (item["Type"].Equals("date") || (item["Type"].Equals("timesamp")) && !context.Params.ContainsKey(item["Field"]) ))
                {
                    context.Params.Add( item["Field"], "now()");
                    placeHolders += "now(),";
                    update = item["Field"] + "= now(),";
                }
                else if((item["Type"].Equals("date") || (item["Type"].Equals("timesamp")) && item["Field"].ToLower().Equals("now")) )
                {
                    context.Params.Add(item["Field"], "now()");
                    placeHolders += "now(),";
                    update = item["Field"] + "= now(),";
                }
                else if(item["Type"].Equals("created") && !context.Params.ContainsKey(item["Field"]))
                {
                    context.Params.Add(item["Field"], "now()");
                    placeHolders += "now(),";
                    update = item["Field"] + "= now(),";
                }
                else
                {
                        placeHolders += "?,";
                        update += " " + item["Field"] + " = ?,";
                }

                keys.Add(item["Field"]);
                
                if( !context.Params[item["Field"]].Equals("now()") )
                {
                    values.Add(context.Params[item["Field"]]);
                }

            }

            update = update.TrimEnd(',');
            placeHolders = placeHolders.TrimEnd(',');
            primary = Regex.Replace(primary, @"AND$", "");

            bool recordExists = false;

            if( hasPrimary)
            {
                var d2 = new DbContext(context.ConnectionString);
                d2.SQL = "SELECT * FROM " + table + " WHERE " + primary;
                var record = MySqlDatabase.query(d2);

                if (record.Count > 0)
                {
                    recordExists = true;
                }
            }

            string query = "";

            if ( !primaryKey.Equals("") && !primaryValue.Equals("") && recordExists)
            {
                query = " UPDATE " + table + " SET " + update + " WHERE " + primary;
            }
            else
            {
                string keyList = string.Join(",", keys.ToArray());
                query = "INSERT INTO " + table + "(" + keyList + ") VALUES(" + placeHolders + ")";
            }

            var d3 = new DbContext(context.ConnectionString);
            d3.SQL = query;
            d3.Params = context.Params;
            
            return MySqlDatabase.query( d3);
        }

        public SearchResults getSearchResults(DbContext context, SearchResults sr)
        {

            if (sr.assigned_page <= 0)
            {
                sr.assigned_page = 1;
            }

            context.SQL = Regex.Replace(sr.query,"SELECT(.*?)FROM", "SELECT count(*) as count FROM" );
            context.isStoredProc = false;

            var results = MySqlDatabase.query(context);

            if( results["rows"].Equals("1") )
            {
                if (results["count"] != null && Int32.Parse(results["count"]) > 0)
                {
                    sr.totalRecords = Int32.Parse(results["count"]);
                }
            }

            if( sr.totalRecords == 0 )
            {
                var results2 = MySqlDatabase.whileQuery(context);
                sr.totalRecords = results2.Count;
            }

            sr.pages = (int)(sr.totalRecords / sr.interval + .99);


            return sr;
        }


    }

    public class Database
    {

        /* ----------------------------------------------------------------------------  
                Description: Get 1 row of data back in a Name Value Pairs MyDictionary...
                Author: Mark Higbee 
                Usage: 
            
                MyDictionary<string, string> Params = new MyDictionary<string, string>();
                Params.Add("Name", "Value");

                MyDictionary<string, string> Results = new MyDictionary<string, string>();
                Results = Database.query(SQL, Params, true);
          
            ---------------------------------------------------------------------------------*/
        public static MyDictionary<string, string> query(DbContext context)
        {
            MyDictionary<string, string> Results = new MyDictionary<string, string>();

            using (SqlConnection conn = new SqlConnection(context.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    if (context.isStoredProc)
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                    }

                    cmd.CommandText = context.SQL;
                    cmd.Connection = conn;

                    foreach (var pair in context.Params)
                    {
                        if (pair.Value != null && pair.Value.StartsWith("@"))
                        {
                            cmd.Parameters.AddWithValue("@" + pair.Key, pair.Value).Direction = ParameterDirection.Output;
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@" + pair.Key, pair.Value);
                        }
                    }

                    try
                    {
                        if (conn.State != ConnectionState.Open)
                        {
                            cmd.Connection.Open();
                        }
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                for (var i = 0; i < reader.FieldCount; i++)
                                {
                                    if (!reader.IsDBNull(i))
                                    {
                                        Results.Add(reader.GetName(i), reader.GetValue(i).ToString());
                                    }
                                    else
                                    {
                                        Results.Add(reader.GetName(i), "N/A");
                                    }
                                }
                            }
                            reader.Close();
                        }
                    }
                    catch (Exception error)
                    {
                        Results["ErrorMsg"] = error.ToString();
                    }

                }
            }

            return Results;
        }

        /* ---------------------------------------------------------------------------- 
           Description: get multiple rows of data back in a List of Name Value Pairs ...
           Author: Mark Higbee
           Usage: 
            
           MyDictionary<string, string> Params = new MyDictionary<string, string>();
           Params.Add("Name", "Value");

           List<MyDictionary<string, string>> Results = new List<MyDictionary<string, string>>();
           Results = Database.whileQuery(SQL,Params, true);
          
         --------------------------------------------------------------------------------*/
        public static List<MyDictionary<string, string>> whileQuery(DbContext context)
        {
            List<MyDictionary<string, string>> Results = new List<MyDictionary<string, string>>();

            using (SqlConnection conn = new SqlConnection(context.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    if (context.isStoredProc)
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                    }

                    cmd.CommandText = context.SQL;
                    cmd.Connection = conn;

                    foreach (var pair in context.Params)
                    {
                        cmd.Parameters.AddWithValue("@" + pair.Key, pair.Value);
                    }

                    if (conn.State != ConnectionState.Open)
                    {
                        cmd.Connection.Open();
                    }
                    try
                    {

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                MyDictionary<string, string> Row = new MyDictionary<string, string>();

                                //if (reader.Read())
                                //{
                                for (var i = 0; i < reader.FieldCount; i++)
                                {
                                    if (!reader.IsDBNull(i))
                                    {
                                        Row.Add(reader.GetName(i), reader.GetValue(i).ToString());
                                    }
                                    else
                                    {
                                        Row.Add(reader.GetName(i), "N/A");
                                    }
                                }
                                // }

                                Results.Add(Row);
                            }
                            reader.Close();
                        }
                    }
                    catch (Exception error)
                    {
                        MyDictionary<string, string> Row = new MyDictionary<string, string>();
                        Row.Add("ErrorMsg", error.ToString());
                        Results.Add(Row);
                    }

                }
            }
            return Results;
        }

        /* ---------------------------------------------------------------------------- 
          Description: 
          Author: Mark Higbee 
          Usage: 
         
          Use for Inserts,Updates, Deletes
         
          MyDictionary<string, string> Params = new MyDictionary<string, string>();
          Params.Add("Name", "Value");

          List<MyDictionary<string, string>> Results = new List<MyDictionary<string, string>>();
          Results = Database.insert(SQL,Params,true);
          
        --------------------------------------------------------------------------------*/
        public static MyDictionary<string, string> executeCmd(DbContext context)
        {
            MyDictionary<string, string> Results = new MyDictionary<string, string>();

            using (SqlConnection conn = new SqlConnection(context.ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    if (context.isStoredProc)
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                    }

                    cmd.CommandText = context.SQL;
                    cmd.Connection = conn;
                    cmd.CommandTimeout = 0;

                    foreach (var pair in context.Params)
                    {
                        cmd.Parameters.AddWithValue("@" + pair.Key, pair.Value);
                    }

                    try
                    {
                        if (conn.State != ConnectionState.Open)
                        {
                            conn.Open();
                        }
                        try
                        {
                            cmd.ExecuteNonQuery();
                        }
                        catch (SqlException ex)
                        {
                            Results["ErrorMsg"] = ex.Message.ToString();
                        }
                    }
                    catch (Exception error)
                    {
                        Results["ErrorMsg"] = error.ToString();
                    }
                }
            }

            return Results;
        }


    }
}
