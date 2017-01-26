using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Crawley
{
    class Program
    {
        static void Main(string[] args)
        {
            Uri seed = new Uri("https://news.google.com");
            List<Uri> link = new List<Uri>();
            HttpClient httpClient = new HttpClient();
            int crawls = 0;
            int maxCrawls = 10000;
            String data = String.Empty;
            Task<String> task = null;
            Uri uri = null;
            List<String> email = new List<string>();

            Crawl(ref seed, ref link, ref httpClient, 
                ref crawls, ref maxCrawls, ref data, ref task, ref uri, 
                ref email);
            Console.WriteLine("Crawl finished, crawled all the uris.");
            Console.WriteLine("Found " + email.Count + " email(s):");
            foreach (String eml in email)
            {
                Console.WriteLine(eml);
            }
            Console.ReadKey();
        }

        static private void Crawl(ref Uri Seed, ref List<Uri> Link, 
            ref HttpClient HttpClient, ref int Crawls, ref int MaxCrawls, 
            ref String Data, ref Task<String> Task, ref Uri Uri, 
            ref List<String> Email)
        {
            Task = HttpClient.GetStringAsync(Seed);
            
            try
            {
                Console.WriteLine("Crawling uri #" + Crawls +  ": " + Seed.AbsoluteUri);
                Data = Task.Result.ToString();
                // Optionally: Do something with the data.
                ExtractEmails(Data, ref Email);
            }
            catch (Exception)
            {
                Console.WriteLine("Error while accesing " + Seed.AbsoluteUri);
            }
                
            if (Link.Count <= MaxCrawls)
            { 
                ExtractLink(Link, Data);
            }

            if (Crawls <= MaxCrawls && Crawls < Link.Count)
            {
                Uri = Link[Crawls];
                Crawls++;
                Crawl(ref Uri, ref Link, ref HttpClient,
                    ref Crawls, ref MaxCrawls, ref Data, ref Task, ref Uri, 
                    ref Email);
            }
        }

        private static void ExtractLink(List<Uri> Link, string Data)
        {
            int cursor = 0;
            int uriLength = 0;
            Uri uri = null;
            String linkData = null;

            for (; cursor < Data.Length; cursor++)
            {
                Parse(Link, Data, ref cursor, ref uriLength, ref uri, ref linkData);
            }
        }

        private static void Parse(List<Uri> Link, string data, ref int cursor, 
            ref int uriLength, ref Uri uri, ref string linkData)
        {
            if (cursor >= 0)
            {
                cursor = data.IndexOf("a href", cursor);
                if (cursor >= 0)
                {
                    cursor = data.IndexOf("http", cursor);
                    if (cursor >= 0)
                    {
                        uriLength = GetUriLength(data, cursor); if (uriLength >= 11)
                        {
                            AddLink(Link, data, ref cursor, uriLength, ref uri, ref linkData);
                        }
                        else if (uriLength == -1)
                        {
                            cursor = data.Length;
                        }
                        else
                        {
                            cursor += uriLength;
                        }
                    }
                    else
                    {
                        cursor = data.Length;
                    }
                }
                else
                {
                    cursor = data.Length;
                }
            }
            else
            {
                cursor = data.Length;
            }
        }

        private static int GetUriLength(string data, int cursor)
        {
            return data.IndexOf(">", cursor) < data.IndexOf("\"", cursor) ?
                                        data.IndexOf(" ", cursor) - cursor :
                                        data.IndexOf("\"", cursor) - cursor;
        }

        private static void AddLink(List<Uri> Link, string data, ref int cursor, 
            int uriLength, ref Uri uri, ref string linkData)
        {
            try
            {
                linkData = data.Substring(cursor, uriLength);
                uri = new Uri(linkData);
                if (!Link.Contains(uri))
                {
                    Link.Add(uri);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                cursor += uriLength;
            }
        }

        private static void ExtractEmails(String Data, ref List<String> Email)
        {
            Regex emailRegularExpression = new Regex(@"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$");
            MatchCollection email = emailRegularExpression.Matches(Data);

            foreach(Match match in email)
            {
                Email.Add(match.Value);
                Console.WriteLine("Email: " + match.Value);
            }
        }
    }
}