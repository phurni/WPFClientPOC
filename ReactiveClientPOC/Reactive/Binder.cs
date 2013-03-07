using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Dynamic;
using System.Linq;
using System.Net;
using RestSharp;

namespace Reactive
{
    public class Binder : DynamicObject
    {
        protected Command _fetch;
        protected Command _destroy;
        protected Command _display;

        public ICommand Fetch   { get { return _fetch; } }
        public ICommand Destroy { get { return _destroy; } }
        public ICommand Display { get { return _display; } }

        public Binder()
        {
            _fetch   = new FetchCommand(this);
            _destroy = new DestroyCommand(this);
            _display = new DisplayCommand(this);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {

            var name = binder.Name;
            result = "Some text";
 
            return true;
        }
    }

    public class FormBinder : Binder
    {
        protected Command _create;
        protected Command _update;

        public ICommand Create  { get { return _create; } }
        public ICommand Update  { get { return _update; } }

        public FormBinder()
        {
            _create  = new CreateCommand(this);
            _update  = new UpdateCommand(this);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {

            var name = binder.Name;
            result = "Some text";
 
            return true;
        }
    }

    public abstract class Command : ICommand
    {
        protected Binder binder;
        protected RestClient rest;

        protected string openMode;

        public Command(Binder binder)
        {
            this.binder = binder;
            rest = (RestClient)Application.Current.Properties["restClient"];
        }

        public Command(Command source)
        {
            binder = source.binder;
            rest = source.rest;
            openMode = source.openMode;
        }

        public void Execute(object parameter)
        {
            // explode parameter and run the request, subclasses customizing the request
            var source = (DependencyObject)parameter;
            if (source == null)
            {
                NotifyError(new Exception("Invalid arguments")); // TODO: declare explicit exception class 
                return;
            }

            string uri = BinderParameters.GetResourceUri(source);
            openMode = BinderParameters.GetOpenMode(source);

            RunRequest(uri);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        protected abstract void FillRequest(IRestRequest request);
        protected abstract void HandleResponse(IRestResponse response);

        public void RunRequest(string uri)
        {
            var request = new RestRequest(uri);
            FillRequest(request);

            /* Can't currently use ExecuteAsync() because the response will be handled
             * in another thread and accessing the UI from another thread is forbidden!
             * When times will come to drop .Net 4.0 compatibility and go with 4.5, we'll
             * be able to use async/await for this.
            rest.ExecuteAsync(request, response => {
                if (response.ErrorException == null)
                {
                    try
                    {
                        HandleResponse(response);
                    }
                    catch (Exception e) {
                        NotifyError(e);
                    }
                }
                else
                    NotifyError(response.ErrorException);
            });
            */
            var response = rest.Execute(request);
            if (response.ErrorException == null)
            {
                try
                {
                    HandleResponse(response);
                }
                catch (Exception e)
                {
                    NotifyError(e);
                }
            }
            else
                NotifyError(response.ErrorException);
        }

        protected void NotifyError(Exception exception)
        {
            // FIXME: Ugly way to show the issue, only for POC
            System.Windows.MessageBox.Show(exception.Message);
        }
    }

    public class FormCommand : Command
    {
        protected new FormBinder binder;

        public FormCommand(FormBinder binder) : base(binder)
        {
            this.binder = binder;
        }

        public FormCommand(Command source) : base(source) { }

        protected override void FillRequest(IRestRequest request)
        {
            request.AddHeader("Accept", "application/xml;application/json;text/xml;text/json");

            // iterate over the binder properties to add them as parameters
            //binder.GetDynamicMemberNames().
        }

        protected override void HandleResponse(IRestResponse response)
        {
            // We have three kind of responses,
            switch (response.StatusCode)
            {
                // 1. Ok created, or updated. Do nothing
                case HttpStatusCode.Created:
                case HttpStatusCode.NoContent:
                    break;

                // 2. We receive back a data object indicating form errors (validation)
                case HttpStatusCode.OK:
                    break;

                // 3. Redirected, which means a Display command
                case HttpStatusCode.Found:
                case HttpStatusCode.Moved:
                // We should only have to handle SeeOther but in some circomstances servers may respond with 301 or 302.
                case HttpStatusCode.SeeOther:
                    var displayCommand = new DisplayCommand(this);
                    displayCommand.RunRequest((string)response.Headers.Where(header => header.Name == "Location").First().Value);
                    break;

                default:
                    //throw new UnknownResponse();
                    break;
            }
        }
    }

    public class CreateCommand :FormCommand
    {
        public CreateCommand(FormBinder binder) : base(binder) { }
        public CreateCommand(Command source) : base(source) { }

        protected override void FillRequest(IRestRequest request)
        {
            base.FillRequest(request);
            request.Method = Method.POST;
        }
    }

    public class UpdateCommand : FormCommand
    {
        public UpdateCommand(FormBinder binder) : base(binder) { }
        public UpdateCommand(Command source) : base(source) { }

        protected override void FillRequest(IRestRequest request)
        {
            base.FillRequest(request);
            request.Method = Method.PUT;
        }
    }

    public class FetchCommand : Command
    {
        public FetchCommand(Binder binder) : base(binder) { }
        public FetchCommand(Command source) : base(source) { }

        protected override void FillRequest(IRestRequest request)
        {
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/xml;application/json;text/xml;text/json");
        }

        protected override void HandleResponse(IRestResponse response)
        {
        }
    }

    public class DestroyCommand : Command
    {
        public DestroyCommand(Binder binder) : base(binder) { }
        public DestroyCommand(Command source) : base(source) { }

        protected override void FillRequest(IRestRequest request)
        {
            request.Method = Method.DELETE;
        }

        protected override void HandleResponse(IRestResponse response)
        {
        }
    }

    public class DisplayCommand : Command
    {
        public DisplayCommand(Binder binder) : base(binder) { }
        public DisplayCommand(Command source) : base(source) { }

        protected override void FillRequest(IRestRequest request)
        {
            request.Method = Method.GET;
            request.AddHeader("Accept", "application/xaml+xml");
        }

        protected override void HandleResponse(IRestResponse response)
        {
            switch (openMode)
            {
                case "newPane":
                    break;

                // default is "newWindow"
                default:
                    ((Window)XamlReader.Parse(response.Content)).Show();
                    break;
            }
        }
    }

    public static class BinderParameters
    {
        private static readonly DependencyProperty ResourceUriProperty =
            DependencyProperty.RegisterAttached("ResourceUri",
                                                typeof(string),
                                                typeof(BinderParameters));

        private static readonly DependencyProperty OpenModeProperty =
            DependencyProperty.RegisterAttached("OpenMode",
                                                typeof(string),
                                                typeof(BinderParameters));

        public static void SetResourceUri(DependencyObject element, string value)
        {
            element.SetValue(ResourceUriProperty, value);
        }
        public static string GetResourceUri(DependencyObject element)
        {
            return (string)element.GetValue(ResourceUriProperty);
        }

        public static void SetOpenMode(DependencyObject element, string value)
        {
            element.SetValue(OpenModeProperty, value);
        }
        public static string GetOpenMode(DependencyObject element)
        {
            return (string)element.GetValue(OpenModeProperty);
        }
    }

    // Extension to shorten the CommandParamter content to CommandParamter="{reactive:Self}"
    // instead of CommandParamter="{Binding RelativeSource={RelativeSource Self}}"
    // To enable add this namespace to the XAML document: xmlns:reactive="clr-namespace:Reactive"
    public class SelfExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var obj = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (obj == null)
            {
                throw new ArgumentNullException("IServiceProvider did not give the required IProvideValueTarget object");
            }

            return obj.TargetObject;
        }
    }
}
