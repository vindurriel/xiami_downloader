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
        public static void RegisterContext(string contextName, IActionProvider obj, string propertyName)
        {
            Contexts[contextName] = obj;
            obj.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == propertyName)
                    Refresh();
            };
            Actions[contextName] = new List<CharmAction>();
            foreach (var item in obj.ProvideActions())
            {
                Actions[contextName].Add(item);
            }
        }
        public static void SetActionBar(IActionBar bar)
        {
            actionBar = bar;
        }
        private static string contextName;

        public static string ContextName
        {
            get { return contextName; }
            set { contextName = value; Refresh(); }
        }

        public static void Refresh()
        {
            if (actionBar == null) return;
            var validActions = new List<CharmAction>();
            if (!string.IsNullOrEmpty(ContextName) && Contexts.ContainsKey(ContextName))
            {
                validActions = Actions[ContextName].Where(x => x.Validate(Contexts[ContextName])).ToList();
                actionBar.ValidActions(validActions);
            }
            actionBar.IsOpen = validActions.Count > 0;
        }
        static Dictionary<string, object> Contexts = new Dictionary<string, object>();
        static IActionBar actionBar;

        static Dictionary<string, List<CharmAction>> Actions = new Dictionary<string, List<CharmAction>>();
    }
    public interface IActionBar
    {
        void ValidActions(IEnumerable<CharmAction> actions);
        bool IsOpen { get; set; }
    }
    public interface IActionProvider : System.ComponentModel.INotifyPropertyChanged
    {
        IEnumerable<CharmAction> ProvideActions();
    }
}
