using System;
using System.IO;
using System.Text;
using System.Text.Json;
using EventStore.ClientAPI;

namespace EventStore.Shell.EventStore
{
    public static class ResolvedEventExtensions
    {
        public static string DataAsString(this ResolvedEvent evt) => evt.Event.IsJson ? Deserialise(evt.Event.Data) : null;

        public static string MetaAsString(this ResolvedEvent evt) => evt.Event.IsJson ? Deserialise(evt.Event.Metadata) : null;

        static string Deserialise(byte[] data)
        {
            try
            {
                var document = JsonDocument.Parse(data);

                using var stream = new MemoryStream();
                using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions {Indented = true});
                document.WriteTo(writer);
                writer.Flush();
                
                return Encoding.UTF8.GetString(stream.ToArray());
            }
            catch (Exception)
            {
                return "Not a valid JSON document";
            }
        }
    }
}
