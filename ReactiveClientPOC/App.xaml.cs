using System;
using System.Windows;
using System.Collections.Generic;
using System.Dynamic;

using RestSharp;
using Reactive;

using JsonFx.Serialization;
using JsonFx.Serialization.Providers;
using JsonFx.Serialization.Resolvers;

namespace ReactiveClientPOC
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Create the application wide REST client
            Properties.Add("restClient", CreateRestClient(ReactiveClientPOC.Properties.Settings.Default.BaseURL));

            // Create the application wide Deserializer Provider (based on content-type)
            Properties.Add("restReaderProvider", CreateRestReaderProvider());

            // Create the application wide Serializer Provider (based on accept and content-type)
            Properties.Add("restWriterProvider", CreateRestWriterProvider());

            // Fetch and display the root window
            //new Binder().Fetch.Execute(new CommandArguments() { Uri = "/" });
            new MainWindow().Show();
        }

        protected RestClient CreateRestClient(string baseUrl)
        {
            RestClient rest = new RestClient(baseUrl);

            // Setup the user defined authenticator, which let him choose between Basic authentication (only over HTTPS, please)
            // or with the ParamsAuthentication based on the authlogic_api protocol
            // rest.Authenticator = RestAuthenticator.create(app_config.rest_authenticator);

            // configure requests to not follow redirects, we handle them ourself
            rest.FollowRedirects = false;

            return rest;
        }

        protected IDataReaderProvider CreateRestReaderProvider()
        {
            var readerSettings = new DataReaderSettings();
            readerSettings.UntypedResolverStrategy = new ConventionResolverStrategy(ConventionResolverStrategy.WordCasing.PascalCase);

            return new DataReaderProvider(new List<IDataReader> { new JsonFx.Json.JsonReader(readerSettings), new JsonFx.Xml.XmlReader(readerSettings) });
        }

        protected IDataWriterProvider CreateRestWriterProvider()
        {
            var writerSettings = new DataWriterSettings();

            return new DataWriterProvider(new List<IDataWriter> { new JsonFx.Json.JsonWriter(writerSettings), new JsonFx.Xml.XmlWriter(writerSettings) });
        }
    }
}
