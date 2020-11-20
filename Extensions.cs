using CefSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Viewer3D
{
    public static class CefExample
    {
        //TODO: Revert after https://bitbucket.org/chromiumembedded/cef/issues/2685/networkservice-custom-scheme-unable-to
        //has been fixed.
        public const string ExampleDomain = "cefsharp.example";
        public const string BaseUrl = "https://" + ExampleDomain;
        public const string DefaultUrl = BaseUrl + "/home.html";
        public const string BindingTestUrl = BaseUrl + "/BindingTest.html";
        public const string BindingTestNetCoreUrl = BaseUrl + "/BindingTestNetCore.html";
        public const string BindingTestSingleUrl = BaseUrl + "/BindingTestSingle.html";
        public const string BindingTestsAsyncTaskUrl = BaseUrl + "/BindingTestsAsyncTask.html";
        public const string LegacyBindingTestUrl = BaseUrl + "/LegacyBindingTest.html";
        public const string PostMessageTestUrl = BaseUrl + "/PostMessageTest.html";
        public const string PluginsTestUrl = BaseUrl + "/plugins.html";
        public const string PopupTestUrl = BaseUrl + "/PopupTest.html";
        public const string TooltipTestUrl = BaseUrl + "/TooltipTest.html";
        public const string BasicSchemeTestUrl = BaseUrl + "/SchemeTest.html";
        public const string ResponseFilterTestUrl = BaseUrl + "/ResponseFilterTest.html";
        public const string DraggableRegionTestUrl = BaseUrl + "/DraggableRegionTest.html";
        public const string DragDropCursorsTestUrl = BaseUrl + "/DragDropCursorsTest.html";
        public const string CssAnimationTestUrl = BaseUrl + "/CssAnimationTest.html";
        public const string CdmSupportTestUrl = BaseUrl + "/CdmSupportTest.html";
        public const string BindingApiCustomObjectNameTestUrl = BaseUrl + "/BindingApiCustomObjectNameTest.html";
        public const string TestResourceUrl = "http://test/resource/load";
        public const string RenderProcessCrashedUrl = "http://processcrashed";
        public const string TestUnicodeResourceUrl = "http://test/resource/loadUnicode";
        public const string PopupParentUrl = "http://www.w3schools.com/jsref/tryit.asp?filename=tryjsref_win_close";
        public const string ChromeInternalUrls = "chrome://chrome-urls";
        public const string ChromeNetInternalUrls = "chrome://net-internals";
        public const string ChromeProcessInternalUrls = "chrome://process-internals";
    }

    public class PostMessageExample
    {
        public string Type { get; set; }
        public PostMessageExampleData Data { get; set; }
        public IJavascriptCallback Callback { get; set; }
    }

    public class PostMessageExampleData
    {
        public string Property { get; set; }
    }

    public interface IJavascriptCallback : IDisposable
    {
        /// <summary>
        /// Callback Id
        /// </summary>
        Int64 Id { get; }

        /// <summary>
        /// Execute the javascript callback
        /// </summary>
        /// <param name="parms">param array of objects</param>
        /// <returns>JavascriptResponse</returns>
        Task<JavascriptResponse> ExecuteAsync(params object[] parms);

        /// <summary>
        /// Execute the javascript callback
        /// </summary>
        /// <param name="timeout">timeout</param>
        /// <param name="parms">param array of objects</param>
        /// <returns>JavascriptResponse</returns>
        Task<JavascriptResponse> ExecuteWithTimeoutAsync(TimeSpan? timeout, params object[] parms);

        /// <summary>
        /// Check to see if the underlying resource are still available to execute the callback
        /// </summary>
        bool CanExecute { get; }

        /// <summary>
        /// Gets a value indicating whether the callback has been disposed of.
        /// </summary>
        bool IsDisposed { get; }
    }
    public class CefSharpSchemeHandlerFactory : ISchemeHandlerFactory
    {
        public const string SchemeName = "custom";
        public const string SchemeNameTest = "test";

        private static readonly IDictionary<string, string> ResourceDictionary;

        static CefSharpSchemeHandlerFactory()
        {
            ResourceDictionary = new Dictionary<string, string>
            {
                //{ "/home.html", Resources.home_html },

                //{ "/assets/css/shCore.css", Resources.assets_css },
                //{ "/assets/css/shCoreDefault.css", Resources.assets_css_shCoreDefault_css },
                //{ "/assets/css/docs.css", Resources.docs_css },
                //{ "/assets/js/application.js", Resources.assets_js_application_js },
                //{ "/assets/js/jquery.js", Resources.assets_js_jquery_js },
                //{ "/assets/js/shBrushCSharp.js", Resources.assets_js_shBrushCSharp_js },
                //{ "/assets/js/shBrushJScript.js", Resources.assets_js_shBrushJScript_js },
                //{ "/assets/js/shCore.js", Resources.assets_js_shCore_js },

                //{ "/bootstrap/bootstrap-theme.min.css", Resources.bootstrap_theme_min_css },
                //{ "/bootstrap/bootstrap.min.css", Resources.bootstrap_min_css },
                //{ "/bootstrap/bootstrap.min.js", Resources.bootstrap_min_js },

                //{ "/BindingTest.html", Resources.BindingTest },
                //{ "/BindingTestNetCore.html", Resources.BindingTestNetCore },
                //{ "/BindingTestAsync.js", Resources.BindingTestAsync },
                //{ "/BindingTestSync.js", Resources.BindingTestSync },
                //{ "/BindingTestSingle.html", Resources.BindingTestSingle },
                //{ "/LegacyBindingTest.html", Resources.LegacyBindingTest },
                //{ "/PostMessageTest.html", Resources.PostMessageTest },
                //{ "/ExceptionTest.html", Resources.ExceptionTest },
                //{ "/PopupTest.html", Resources.PopupTest },
                //{ "/SchemeTest.html", Resources.SchemeTest },
                //{ "/TooltipTest.html", Resources.TooltipTest },
                //{ "/FramedWebGLTest.html", Resources.FramedWebGLTest },
                //{ "/MultiBindingTest.html", Resources.MultiBindingTest },
                //{ "/ScriptedMethodsTest.html", Resources.ScriptedMethodsTest },
                //{ "/ResponseFilterTest.html", Resources.ResponseFilterTest },
                //{ "/DraggableRegionTest.html", Resources.DraggableRegionTest },
                //{ "/DragDropCursorsTest.html", Resources.DragDropCursorsTest },
                //{ "/CssAnimationTest.html", Resources.CssAnimation },
                //{ "/CdmSupportTest.html", Resources.CdmSupportTest },
                //{ "/Recaptcha.html", Resources.Recaptcha },
                //{ "/UnicodeExampleGreaterThan32kb.html", Resources.UnicodeExampleGreaterThan32kb },
                //{ "/UnocodeExampleEqualTo32kb.html", Resources.UnocodeExampleEqualTo32kb },
                //{ "/JavascriptCallbackTest.html", Resources.JavascriptCallbackTest },
                //{ "/BindingTestsAsyncTask.html", Resources.BindingTestsAsyncTask },
                //{ "/BindingApiCustomObjectNameTest.html", Resources.BindingApiCustomObjectNameTest }
            };
        }

        public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
        {
            //Notes:
            // - The 'host' portion is entirely ignored by this scheme handler.
            // - If you register a ISchemeHandlerFactory for http/https schemes you should also specify a domain name
            // - Avoid doing lots of processing in this method as it will affect performance.
            // - Use the Default ResourceHandler implementation

            var uri = new Uri(request.Url);
            var fileName = uri.AbsolutePath;

            //Load a file directly from Disk
            if (fileName.EndsWith("CefSharp.Core.xml", StringComparison.OrdinalIgnoreCase))
            {
                //Convenient helper method to lookup the mimeType
                var mimeType = Cef.GetMimeType("xml");
                //Load a resource handler for CefSharp.Core.xml
                //mimeType is optional and will default to text/html
                return ResourceHandler.FromFilePath("CefSharp.Core.xml", mimeType, autoDisposeStream: true);
            }

            if (fileName.EndsWith("Logo.png", StringComparison.OrdinalIgnoreCase))
            {
                //Convenient helper method to lookup the mimeType
                var mimeType = Cef.GetMimeType("png");
                //Load a resource handler for Logo.png
                //mimeType is optional and will default to text/html
                return ResourceHandler.FromFilePath("..\\..\\..\\..\\CefSharp.WinForms.Example\\Resources\\chromium-256.png", mimeType, autoDisposeStream: true);
            }

            if (uri.Host == "cefsharp.com" && schemeName == "https" && (string.Equals(fileName, "/PostDataTest.html", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(fileName, "/PostDataAjaxTest.html", StringComparison.OrdinalIgnoreCase)))
            {
                return new CefSharpSchemeHandler();
            }

            if (string.Equals(fileName, "/EmptyResponseFilterTest.html", StringComparison.OrdinalIgnoreCase))
            {
                return ResourceHandler.FromString("", mimeType: ResourceHandler.DefaultMimeType);
            }

            string resource;
            if (ResourceDictionary.TryGetValue(fileName, out resource) && !string.IsNullOrEmpty(resource))
            {
                var fileExtension = Path.GetExtension(fileName);
                return ResourceHandler.FromString(resource, includePreamble: true, mimeType: Cef.GetMimeType(fileExtension));
            }

            return null;
        }
    }

    public struct JsSerializableStruct
    {
        public string Value;
    }

    public class JsSerializableClass
    {
        public string Value { get; set; }
    }

    public class SimpleClass
    {
        public IJavascriptCallback Callback { get; set; }
        public string TestString { get; set; }

        public IList<SimpleSubClass> SubClasses { get; set; }
    }

    public class SimpleSubClass
    {
        public string PropertyOne { get; set; }
        public int[] Numbers { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = true)]
    public class JavascriptIgnoreAttribute : Attribute
    {
    }

    internal class CefSharpSchemeHandler : ResourceHandler
    {
        public override CefReturnValue ProcessRequestAsync(IRequest request, ICallback callback)
        {
            var uri = new Uri(request.Url);
            var fileName = uri.AbsolutePath;

            Task.Run(() =>
            {
                using (callback)
                {
                    Stream stream = null;

                    if (string.Equals(fileName, "/PostDataTest.html", StringComparison.OrdinalIgnoreCase))
                    {
                        var postDataElement = request.PostData.Elements.FirstOrDefault();
                        stream = ResourceHandler.GetMemoryStream("Post Data: " + (postDataElement == null ? "null" : postDataElement.GetBody()), Encoding.UTF8);
                    }

                    if (string.Equals(fileName, "/PostDataAjaxTest.html", StringComparison.OrdinalIgnoreCase))
                    {
                        var postData = request.PostData;
                        if (postData == null)
                        {
                            stream = ResourceHandler.GetMemoryStream("Post Data: null", Encoding.UTF8);
                        }
                        else
                        {
                            var postDataElement = postData.Elements.FirstOrDefault();
                            stream = ResourceHandler.GetMemoryStream("Post Data: " + (postDataElement == null ? "null" : postDataElement.GetBody()), Encoding.UTF8);
                        }
                    }

                    if (stream == null)
                    {
                        callback.Cancel();
                    }
                    else
                    {
                        //Reset the stream position to 0 so the stream can be copied into the underlying unmanaged buffer
                        stream.Position = 0;
                        //Populate the response values - No longer need to implement GetResponseHeaders (unless you need to perform a redirect)
                        ResponseLength = stream.Length;
                        MimeType = "text/html";
                        StatusCode = (int)HttpStatusCode.OK;
                        Stream = stream;

                        callback.Continue();
                    }
                }
            });

            return CefReturnValue.ContinueAsync;
        }
    }

    public class ExceptionTestBoundObject
    {
        [DebuggerStepThrough]
        private double DivisionByZero(int zero)
        {
            return 10 / zero;
        }

        [DebuggerStepThrough]
        public double TriggerNestedExceptions()
        {
            try
            {
                try
                {
                    return DivisionByZero(0);
                }
                catch (Exception innerException)
                {
                    throw new InvalidOperationException("Nested Exception Invalid", innerException);
                }
            }
            catch (Exception e)
            {
                throw new OperationCanceledException("Nested Exception Canceled", e);
            }
        }

        [DebuggerStepThrough]
        public int TriggerParameterException(int parameter)
        {
            return parameter;
        }

        public void TestCallbackException(IJavascriptCallback errorCallback, IJavascriptCallback errorCallbackResult)
        {
            const int taskDelay = 500;

            Task.Run(async () =>
            {
                await Task.Delay(taskDelay);

                using (errorCallback)
                {
                    JavascriptResponse result = await errorCallback.ExecuteAsync("This callback from C# was delayed " + taskDelay + "ms");
                    string resultMessage;
                    if (result.Success)
                    {
                        resultMessage = "Fatal: No Exception thrown in error callback";
                    }
                    else
                    {
                        resultMessage = "Exception Thrown: " + result.Message;
                    }
                    await errorCallbackResult.ExecuteAsync(resultMessage);
                }
            });
        }
    }

    public struct CallbackResponseStruct
    {
        public string Response;

        public CallbackResponseStruct(string response)
        {
            Response = response;
        }
    }

    public class AppendResponseFilter : IResponseFilter
    {
        private static Encoding encoding = Encoding.UTF8;

        /// <summary>
        /// Overflow from the output buffer.
        /// </summary>
        private List<byte> overflow = new List<byte>();

        public AppendResponseFilter(string stringToAppend)
        {
            //Add the encoded string into the overflow.
            overflow.AddRange(encoding.GetBytes(stringToAppend));
        }

        bool IResponseFilter.InitFilter()
        {
            return true;
        }

        FilterStatus IResponseFilter.Filter(Stream dataIn, out long dataInRead, Stream dataOut, out long dataOutWritten)
        {
            if (dataIn == null)
            {
                dataInRead = 0;
                dataOutWritten = 0;

                return FilterStatus.Done;
            }

            //We'll read all the data
            dataInRead = dataIn.Length;
            dataOutWritten = Math.Min(dataInRead, dataOut.Length);

            if (dataIn.Length > 0)
            {
                //Copy all the existing data first
                dataIn.CopyTo(dataOut);
            }

            // If we have overflow data and remaining space in the buffer then write the overflow.
            if (overflow.Count > 0)
            {
                // Number of bytes remaining in the output buffer.
                var remainingSpace = dataOut.Length - dataOutWritten;
                // Maximum number of bytes we can write into the output buffer.
                var maxWrite = Math.Min(overflow.Count, remainingSpace);

                // Write the maximum portion that fits in the output buffer.
                if (maxWrite > 0)
                {
                    dataOut.Write(overflow.ToArray(), 0, (int)maxWrite);
                    dataOutWritten += maxWrite;
                }

                if (maxWrite == 0 && overflow.Count > 0)
                {
                    //We haven't yet got space to append our data
                    return FilterStatus.NeedMoreData;
                }

                if (maxWrite < overflow.Count)
                {
                    // Need to write more bytes than will fit in the output buffer. 
                    // Remove the bytes that were written already
                    overflow.RemoveRange(0, (int)(maxWrite - 1));
                }
                else
                {
                    overflow.Clear();
                }
            }

            if (overflow.Count > 0)
            {
                return FilterStatus.NeedMoreData;
            }

            return FilterStatus.Done;
        }

        public void Dispose()
        {

        }
    }
}
