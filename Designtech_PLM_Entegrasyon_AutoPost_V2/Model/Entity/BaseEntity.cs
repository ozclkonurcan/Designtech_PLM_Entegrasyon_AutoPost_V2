using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Designtech_PLM_Entegrasyon_AutoPost_V2.Model.Entity
{
    public abstract class BaseEntity
    {

        public BaseEntity()
        {
            TransferID = Guid.NewGuid().ToString();
        }

        public string TransferID { get; set; }

        //private string _transferID;

        //public string TransferID { get {

        //        if (_transferID.IsNullOrEmpty())
        //        {
        //            _transferID = Guid.NewGuid().ToString();
        //        }
        //            return _transferID;

        //    }
        //    set {
        //        _transferID = value;
        //    }

        //  }
    }
}
