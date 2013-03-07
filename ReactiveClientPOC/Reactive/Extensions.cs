using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Xaml;
using Reactive;

namespace Reactive.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider self)
            where T : class
        {
            return self.GetService(typeof(T)) as T;
        }
    }

    // Enregistement: xmlns:r="clr-namespace:Reactive.Extensions"
    public class SelfExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var obj = serviceProvider.GetService<IProvideValueTarget>();
            if (obj == null)
            {
                throw new ArgumentNullException("ISetviceProvider did not give the required IProvideValueTarget object");
            }

            return obj.TargetObject;
        }
    }

    public interface IResourceInfo
    {
        string Uri { get; set; }
        string OpenMode { get; set; }
    }

    public class ResourceExtension : MarkupExtension, IResourceInfo
    {
        public string Uri { get; set; }
        public string OpenMode { get; set; }

        public ResourceExtension(string uri)
        {
            this.Uri = uri;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this as IResourceInfo;
        }
    }

    public class FormBinderExtension : MarkupExtension
    {
        public IResourceInfo Target { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new FormBinder() { Resource = Target };
        }
    }
}
