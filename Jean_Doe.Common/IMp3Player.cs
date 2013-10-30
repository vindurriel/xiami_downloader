using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
namespace Jean_Doe.Common
{
    [ServiceContract]
    public interface IMp3Player
    {
        [OperationContract]
        double Play();
        [OperationContract]
        void Pause();
        [OperationContract]
        void Exit();
        [OperationContract]
        void SetCurrentTime(double t);
        [OperationContract]
        double GetCurrentTime();
        [OperationContract]
        bool Initialize(string fileName);

    }
}
