using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.IO;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Net;
using System.Threading.Tasks;

/*
 * text/plain formatter for use with DebugOutput
 */

namespace LiiteriStatisticsProxiedAPI.Formatters
{
    /*
     * http://stackoverflow.com/questions/11581697/is-there-a-way-to-force-asp-net-web-api-to-return-plain-text
     */
    public class TextPlainFormatter : MediaTypeFormatter
    {

        public TextPlainFormatter()
        {
            this.SupportedMediaTypes.Add(
                new MediaTypeHeaderValue("text/plain"));
        }

        public override bool CanWriteType(Type type)
        {
            return type == typeof(string);
        }

        public override bool CanReadType(Type type)
        {
            return type == typeof(string);
        }

        public override Task WriteToStreamAsync(
            Type type,
            object value,
            Stream writeStream,
            System.Net.Http.HttpContent content,
            TransportContext transportContext)
        {
            return Task.Factory.StartNew(() => {
                StreamWriter writer = new StreamWriter(writeStream);
                writer.Write(value);
                writer.Flush();
            });
        }

        public override Task<object> ReadFromStreamAsync(
            Type type,
            Stream readStream,
            System.Net.Http.HttpContent content,
            IFormatterLogger formatterLogger)
        {
            return Task.Factory.StartNew(() => {
                StreamReader reader = new StreamReader(readStream);
                return (object) reader.ReadToEnd();
            });
        }
    }
}
