using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Dynamic;
using System.Linq;
using System.Net;
using RestSharp;
using System.Diagnostics;

namespace Reactive
{
    using Reactive.Extensions;

    public class Binder : DynamicObject
    {
        protected Command _fetch;
        protected Command _destroy;
        protected Command _display;

        public ICommand Fetch { get { return _fetch; } }
        public ICommand Destroy { get { return _destroy; } }
        public ICommand Display { get { return _display; } }

        public IResourceInfo Resource { get; set; }

        public Binder()
        {
            _fetch = new FetchCommand(this);
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

        public ICommand Create { get { return _create; } }
        public ICommand Update { get { return _update; } }

        public FormBinder()
        {
            _create = new CreateCommand(this);
            _update = new UpdateCommand(this);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var name = binder.Name;
            result = "Some text";

            return true;
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

}