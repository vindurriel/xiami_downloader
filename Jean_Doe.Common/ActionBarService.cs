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
        public static void RegisterContext(string contextName,  IActionProvider obj)
        {
            Contexts[contextName] = obj;
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
        public static string ContextName { get; set; }
        public static void Refresh()
        {
            if (string.IsNullOrEmpty(ContextName)) return;
            if (!Contexts.ContainsKey(ContextName)) return;
            if (actionBar == null) return;
            var validActions = Actions[ContextName].Where(x => x.Validate(Contexts[ContextName])).ToList();
            actionBar.ValidActions(validActions);
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
    public interface IActionProvider
    {
        IEnumerable<CharmAction> ProvideActions();
    }
}
