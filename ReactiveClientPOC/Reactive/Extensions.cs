using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Xaml;
using Reactive;

namespace Reactive
{
    public interface ICommandArguments
    {
        string Uri { get; set; }
        string OpenMode { get; set; }
    }

    public class CommandArguments : ICommandArguments
    {
        public string Uri { get; set; }
        public string OpenMode { get; set; }
    }

    public class CommandExtension : MarkupExtension, ICommandArguments
    {
        protected CommandArguments args = new CommandArguments();

        public string Uri      { get { return args.Uri;      } set { args.Uri      = value; } }
        public string OpenMode { get { return args.OpenMode; } set { args.OpenMode = value; } }

        public CommandExtension(string uri)
        {
            args.Uri = uri;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return args;
        }
    }

    public class FormExtension : MarkupExtension
    {
        protected string uri;

        public FormExtension(string uri)
        {
            this.uri = uri;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new FormBinder(uri);
        }
    }

}
