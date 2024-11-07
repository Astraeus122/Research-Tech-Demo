using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.BossRoom.Gameplay.AI
{
    [Serializable]
    public class SerializableQTable
    {
        public Dictionary<string, Dictionary<string, float>> QTableData = new Dictionary<string, Dictionary<string, float>>();
    }
}
