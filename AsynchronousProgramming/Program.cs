using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsynchronousProgramming
{
    class Program
    {
        const string connectionString = "Data source = localhost; Initial catalog = Chinook; Integrated security = SSPI;";
        static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "MainThread";

            #region This will block the thread...
            //DataSet dsArtists = GetArtistsAsync().Result;
            //foreach (DataRow row in dsArtists.Tables["Artists"].Rows)
            //{
            //    foreach (DataColumn col in dsArtists.Tables[0].Columns)
            //    {
            //        Console.Write(row[col] + "\t");
            //    }
            //    Console.WriteLine();
            //}
            #endregion

            #region This won't block the main thread
            //GetArtistsAsync().ContinueWith(task =>
            //{
            //    DataSet dsArtists = task.Result;
            //    foreach (DataRow row in dsArtists.Tables["Artists"].Rows)
            //    {
            //        foreach (DataColumn col in dsArtists.Tables[0].Columns)
            //        {
            //            Console.Write(row[col] + "\t");
            //        }
            //        Console.WriteLine();
            //    }
            //});
            #endregion

            #region This won't block the thread and catches exceptions
            //GetArtistsAsync().ContinueWith(task =>
            //{
            //    DataSet dsArtists = task.Result;
            //    foreach (DataRow row in dsArtists.Tables["Artists"].Rows)
            //    {
            //        foreach (DataColumn col in dsArtists.Tables[0].Columns)
            //        {
            //            Console.Write(row[col] + "\t");
            //        }
            //        Console.WriteLine();
            //    }
            //}, TaskContinuationOptions.NotOnFaulted);

            //GetArtistsAsync().ContinueWith(task =>
            //{
            //    Console.WriteLine(task.Exception.InnerException.Message);
            //}, TaskContinuationOptions.OnlyOnFaulted);

            #endregion

            #region Task Composition - Multithreading
            //var watch = Stopwatch.StartNew();

            //Task<DataSet> artistsTask = GetArtistsAsync();
            //Task<DataSet> albumsTask = GetAlbumsAsync();

            //Task.Factory.ContinueWhenAll(new[] { artistsTask, albumsTask }, (tasks) =>
            //{
            //    foreach (var task in tasks)
            //    {
            //        if (task.Status == TaskStatus.RanToCompletion)
            //        {
            //            DataSet ds = task.Result;
            //            if (ds.Tables[0].TableName == "Artists")
            //            {
            //                foreach (DataRow row in ds.Tables["Artists"].Rows)
            //                {
            //                    foreach (DataColumn col in ds.Tables[0].Columns)
            //                    {
            //                        Console.Write(row[col] + "\t");
            //                    }
            //                    Console.WriteLine();
            //                }
            //            }
            //            else if (ds.Tables[0].TableName == "Albums")
            //            {
            //                foreach (DataRow row in ds.Tables["Albums"].Rows)
            //                {
            //                    foreach (DataColumn col in ds.Tables[0].Columns)
            //                    {
            //                        Console.Write(row[col] + "\t");
            //                    }
            //                    Console.WriteLine();
            //                }
            //            }
            //        }
            //        else
            //        {
            //            Console.WriteLine("An error has occurred..");
            //            Console.WriteLine(task.Exception.InnerException.Message);
            //        }
            //        Console.WriteLine();
            //        Console.WriteLine("------------------------------------------------");
            //        Console.WriteLine();
            //    }

            //    watch.Stop();
            //    Console.WriteLine("Time elapsed: " + watch.ElapsedMilliseconds + " milliseconds");
            //});
            #endregion

            #region Asynchronous - No multithreading
            //GetCustomersAsync().ContinueWith((task) =>
            //{
            //    DataSet dsCustomers = task.Result;
            //    foreach (DataRow row in dsCustomers.Tables["Customers"].Rows)
            //    {
            //        foreach (DataColumn col in dsCustomers.Tables[0].Columns)
            //        {
            //            Console.Write(row[col] + "\t");
            //        }
            //        Console.WriteLine();
            //    }
            //});

            #endregion


            Console.WriteLine();
            Console.WriteLine("Thread: " + Thread.CurrentThread.Name);
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        static Task<DataSet> GetArtistsAsync()
        {
            DataSet ds = new DataSet();
            return Task<DataSet>.Factory.StartNew(() =>
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string sqlSelect = @"WAITFOR DELAY '000:00:05'
                                        SELECT TOP 10 * FROM Artist";
                    SqlDataAdapter da = new SqlDataAdapter(sqlSelect, con);
                    da.Fill(ds);
                    ds.Tables[0].TableName = "Artists";
                }
                Console.WriteLine("Thread: " + Thread.CurrentThread.Name);
                return ds;
            });
        }

        static Task<DataSet> GetAlbumsAsync()
        {
            DataSet ds = new DataSet();
            return Task<DataSet>.Factory.StartNew(() =>
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string sqlSelect = @"WAITFOR DELAY '000:00:05'
                                        SELECT TOP 10 * FROM Album";
                    SqlDataAdapter da = new SqlDataAdapter(sqlSelect, con);
                    da.Fill(ds);
                    ds.Tables[0].TableName = "Albums";
                }
                Console.WriteLine("Thread: " + Thread.CurrentThread.Name);
                return ds;
            });
        }

        static Task<DataSet> GetCustomersAsync()
        {
            var tcs = new TaskCompletionSource<DataSet>();

            DataSet ds = new DataSet();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string sqlSelect = @"WAITFOR DELAY '000:00:05'
                                        SELECT TOP 10 * FROM Customer";
                SqlDataAdapter da = new SqlDataAdapter(sqlSelect, con);
                da.Fill(ds);
                ds.Tables[0].TableName = "Customers";
            }
            Console.WriteLine("Thread in GetCustomersAsync: " + Thread.CurrentThread.Name);
            tcs.SetResult(ds);
            return tcs.Task;
        }
    }
}
