using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SDG {

    public class ParamBase {
        public string name;
        public string password;
        public int userid;
        public string token;
    }

    public class ParamRoom:ParamBase {
        public int roomid;
    }

    public class ParamPlayMove:ParamBase
    {
        public int x;
        public int y;
    }

    public class Params
    {
    }
}
