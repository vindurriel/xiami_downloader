using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jean_Doe.MusicControl
{
    public class CollectViewModel : MusicViewModel, IHasCollection
    {
        Collection collect;
        public CollectViewModel(Collection a)
            : base(a)
        {
            collect = a;
            ImageSource = "/Jean_Doe.MusicControl;component/Resources/collect.png";
            typecolor = "#588ae4";
        }
        public string CollectionId
        {
            get { return collect.Id; }
        }

        public string CollectionName
        {
            get { return collect.Name; }
        }
    }
}
