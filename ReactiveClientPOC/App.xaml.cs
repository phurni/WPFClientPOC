using System;
using System.Windows;
using RestSharp;
using Reactive;

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

            // Fetch and display the root window
            //var parameter = new DependencyObject();
            //BinderParameters.SetResourceUri(parameter, "/");
            //(new Binder()).Display.Execute(parameter);
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
    }
}
