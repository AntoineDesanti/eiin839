using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;

namespace BasicServerHTTPlistener
{
    /*
     Urls to tests
    - http://localhost:8080/404 
    - http://localhost:8080/hello?param1=whatever

     */


    public class MyReflectionClass
    {
        public string Hello(object[] uriParams)
        {
            HttpListenerRequest request =  (HttpListenerRequest) uriParams[0];
            string responseString = "<HTML><BODY> Welcome in hello method!";
            responseString += "<p> Parameter 1 is ";
            responseString += HttpUtility.ParseQueryString(request.Url.Query).Get("param1") + "</p>";
            responseString += "<p> Parameter 2 is ";
            responseString += HttpUtility.ParseQueryString(request.Url.Query).Get("param2") + "</p>";
            responseString += "<p> Parameter 3 is ";
            responseString += HttpUtility.ParseQueryString(request.Url.Query).Get("param3") + "</p>";
            responseString += "<p> Parameter 4 is ";
            responseString += HttpUtility.ParseQueryString(request.Url.Query).Get("param4") + "</p>";

            responseString += " </BODY></HTML>";

            return responseString;
        }

        public string About(object[] uriParams)
        {
            HttpListenerRequest request = (HttpListenerRequest)uriParams[0];
            string responseString = "<HTML><BODY> This is the about page ";
            responseString += "<p> Parameter 1 is ";
            responseString += HttpUtility.ParseQueryString(request.Url.Query).Get("param1") + "</p>";
            responseString += "<p> Parameter 2 is ";
            responseString += HttpUtility.ParseQueryString(request.Url.Query).Get("param2") + "</p>";
            responseString += "<p> Parameter 3 is ";
            responseString += HttpUtility.ParseQueryString(request.Url.Query).Get("param3") + "</p>";
            responseString += "<p> Parameter 4 is ";
            responseString += HttpUtility.ParseQueryString(request.Url.Query).Get("param4") + "</p>";


            responseString += " </BODY></HTML>";

            return responseString;
        }

        public string ErrorPage(object[] uriParams)
        {
            HttpListenerRequest request = (HttpListenerRequest)uriParams[0];
            string responseString = "<HTML><BODY> The page you are looking for doesn't exist";
            responseString += "<p> You can try the following links";
            responseString += "<p> <a href='http://localhost:8080/Hello?param1=hello&param2=world&param3=!&param4=!'> Hello page</a></p>";
            responseString += "<p> <a href='http://localhost:8080/About?param1=1&param2=2&param3=3&param4=100'>About page</a> </p>";
            responseString += " </BODY></HTML>";

            return responseString;
        }
    }

    internal class Program
    {

        private static void Main(string[] args)
        {

            //if HttpListener is not supported by the Framework
            if (!HttpListener.IsSupported)
            {
                Console.WriteLine("A more recent Windows version is required to use the HttpListener class.");
                return;
            }

        string[] authorizedUri = { "Hello", "About" };

        // Create a listener.
        HttpListener listener = new HttpListener();

            // Add the prefixes.
            if (args.Length != 0)
            {
                foreach (string s in args)
                {

                    listener.Prefixes.Add(s);
                    // don't forget to authorize access to the TCP/IP addresses localhost:xxxx and localhost:yyyy 
                    // with netsh http add urlacl url=http://localhost:xxxx/ user="Tout le monde"
                    // and netsh http add urlacl url=http://localhost:yyyy/ user="Tout le monde"
                    // user="Tout le monde" is language dependent, use user=Everyone in english 
                }
            }
            else
            {
                Console.WriteLine("Syntax error: the call must contain at least one web server url as argument");
            }
            listener.Start();

            // get args 
            foreach (string s in args)
            {
                Console.WriteLine("Listening for connections on " + s);
            }

            // Trap Ctrl-C on console to exit 
            Console.CancelKeyPress += delegate {
                // call methods to close socket and exit
                listener.Stop();
                listener.Close();
                Environment.Exit(0);
            };


            while (true)
            {
                // Note: The GetContext method blocks while waiting for a request.
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;

                string uri = request.Url.LocalPath.ToString().Substring(1);

                string result;

                if (authorizedUri.Any(x => x.Contains(uri)))
                {
                    Type type = typeof(MyReflectionClass);
                    MethodInfo method = type.GetMethod(uri);
                    MyReflectionClass myReflectionClass = new MyReflectionClass();
                    var uriParams = new object[2];
                    uriParams[0] = request;
                    result = (string)method.Invoke(myReflectionClass, new[] { uriParams });
                }
                else
                {
                    Type type = typeof(MyReflectionClass);
                    MethodInfo method = type.GetMethod("ErrorPage" );
                    MyReflectionClass myReflectionClass = new MyReflectionClass();
                    var uriParams = new object[2];
                    uriParams[0] = request;
                    result = (string)method.Invoke(myReflectionClass, new[] { uriParams });
                }
                


               

               // Console.WriteLine( ((HttpListenerRequest) uriParams[0]).Url);
              
                //Console.WriteLine(result);
                //Console.ReadLine();


                string documentContents;
                using (Stream receiveStream = request.InputStream)
                {
                    using (StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8))
                    {
                        documentContents = readStream.ReadToEnd();
                    }
                }

                // get url 
                Console.WriteLine($"Received request for {request.Url}");

                // parse path in url 
                foreach (string str in request.Url.Segments)
                {
                    Console.WriteLine(str);
                }

                //get params un url. After ? and between &

                Console.WriteLine(request.Url.Query);


                //
                Console.WriteLine(documentContents);

                // Obtain a response object.
                HttpListenerResponse response = context.Response;

                // Construct a response.
                string responseString = result;
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                // Get a response stream and write the response to it.
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // You must close the output stream.
                output.Close();
            }
            // Httplistener neither stop ... But Ctrl-C do that ...
            // listener.Stop();
        }
    }
}