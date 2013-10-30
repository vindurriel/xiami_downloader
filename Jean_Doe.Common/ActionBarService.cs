using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Jean_Doe.Common
{
    public class ActionBarService
    {
        public static void RegisterContext(string contextName, IActionProvider obj, params string[] properties)
        {
            Contexts[contextName] = obj;
            obj.PropertyChanged += (s, e) =>
            {
                if (properties.Contains(e.PropertyName))
                    Refresh();
            };
            Actions[contextName] = new List<CharmAction>();
            foreach (var item in obj.ProvideActions())
            {
                Actions[contextName].Add(item);
            }
        }
        public static void RegisterActionBar(IActionBar bar, string name = "Default")
        {
            ActionBars[name] = bar;
        }
        private static string contextName;

        public static string ContextName
        {
            get { return contextName; }
            set { contextName = value; Refresh(); }
        }

        public static void Refresh()
        {
            if (ActionBars.Count == 0) return;
            if (!string.IsNullOrEmpty(ContextName) && Contexts.ContainsKey(ContextName))
            {
                var provider = Contexts[ContextName];
                foreach (var pair in ActionBars)
                {
                    var actions = provider.ProvideActions(pair.Key);
                    pair.Value.ValidActions(actions);
                }
            }
        }
        static Dictionary<string, IActionProvider> Contexts = new Dictionary<string, IActionProvider>();
        static Dictionary<string, IActionBar> ActionBars = new Dictionary<string, IActionBar>();
        static Dictionary<string, List<CharmAction>> Actions = new Dictionary<string, List<CharmAction>>();
    }
    public interface IActionBar
    {
        void ValidActions(IEnumerable<CharmAction> actions);
    }
    public interface IActionProvider : System.ComponentModel.INotifyPropertyChanged
    {
        IEnumerable<CharmAction> ProvideActions(string actionBarName="Default");
    }
}

