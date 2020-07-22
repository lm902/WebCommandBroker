using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security;

namespace WebCommandBroker
{
    class Program
    {
        static void Main(string[] args)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add(ConfigurationManager.AppSettings["uriPrefix"]);
            listener.Start();
            while (true)
            {
                var context = listener.GetContext();
                try
                {
                    IncomingRequest(context);
                } catch (Exception e)
                {
                    Console.Error.WriteLine(e.StackTrace);
                }
            }
        }

        static void IncomingRequest(HttpListenerContext context)
        {
            context.Response.StatusCode = 204;
            context.Response.ContentType = "text/html; charset=utf-8";
            var writer = new StreamWriter(context.Response.OutputStream);
            try
            {
                Authenticate(context);
                RunCmd(context, writer);
            } catch (Exception e)
            {
                context.Response.StatusCode = 500;
                writer.Write(e.GetType().FullName + ": " + e.Message);
            } finally
            {
                writer.Close();
            }
        }

        static void Authenticate(HttpListenerContext context)
        {
            if (string.IsNullOrEmpty(context.Request.QueryString["key"]) ||
                ConfigurationManager.AppSettings["key"] != context.Request.QueryString["key"])
            {
                throw new SecurityException("你黑你妈呢");
            }
        }

        static void RunCmd(HttpListenerContext context, StreamWriter writer)
        {
            if (!string.IsNullOrEmpty(context.Request.QueryString["cmd"])) {
                var startInfo = new ProcessStartInfo("wt.exe");
                startInfo.Arguments = context.Request.QueryString["cmd"];
                startInfo.UseShellExecute = true;
                Process.Start(startInfo);
            }
        }
    }
}
