using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AdjustEquip
{
    class EquipData
    {
        public Vector3 Pos;
        public Vector3 Sca;
        public Vector3 Rot;
        public EquipData()
        {
            Pos = new Vector3(0,0,0);
            Sca = new Vector3(1, 1, 1);
            Rot = new Vector3(0, 0, 0);
        }
        public EquipData(Vector3 p, Vector3 s, Vector3 r)
        {
            Pos = p;
            Sca = s;
            Rot = r;
        }
    }
}
