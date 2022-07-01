using System.Text;
using Yarp.ReverseProxy.Forwarder;

namespace Yarp.Demo.ExternalApi;

public class Utf8VowelTransformer : HttpTransformer
{
    private char[] _vowels = { 'a', 'e', 'i', 'o', 'u', 'y' };
    
    public override async  ValueTask TransformRequestAsync(HttpContext httpContext, HttpRequestMessage proxyRequest, string destinationPrefix)
    {
        // copy all request headers
        await base.TransformRequestAsync(httpContext, proxyRequest, destinationPrefix);
        
        // update path
        var proxyPath = new PathString(httpContext.Request.Path.Value.Replace("internal/", ""));
        proxyRequest.RequestUri = RequestUtilities.MakeDestinationAddress(destinationPrefix, proxyPath, QueryString.Empty);


        // custom manipulation logic
        var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8);
        var content = await reader.ReadToEndAsync(); // bad practice
        
        if (!string.IsNullOrWhiteSpace(content))
        {
            var proxyContent = new StringBuilder(content.Length);

            foreach (var character in content)
            {
                if (_vowels.Contains(character))
                {
                    proxyContent.Append(char.ToUpper(character));
                }
                else
                {
                    proxyContent.Append(character);
                }
            }
            proxyRequest.Content = JsonContent.Create(proxyContent.ToString());
        }


        
        
        
        // Suppress the original request header, use the one from the destination Uri.
        proxyRequest.Headers.Host = null;
    }
}