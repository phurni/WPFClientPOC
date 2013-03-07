using Reactive.Extensions;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace Reactive
{
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
            var resource = parameter as IResourceInfo;

            RunRequest(resource.Uri);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        protected abstract void FillRequest(IRestRequest request);
        protected abstract void HandleResponse(IRestResponse response);

        public void RunRequest(String uri)
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

        public FormCommand(FormBinder binder)
            : base(binder)
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
            // 1. Ok created, or updated. Do nothing
            // 2. We receive back a data object indicating form errors (validation)
            // 3. Redirected, which means a Display command
            switch (response.StatusCode)
            {
                case HttpStatusCode.Created:
                case HttpStatusCode.NoContent:
                    break;

                case HttpStatusCode.OK:
                    break;

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

    public class CreateCommand : FormCommand
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
}
