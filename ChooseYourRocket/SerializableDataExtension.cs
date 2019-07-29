using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ICities;

namespace ChooseYourRocket
{
    public class SerializableDataExtension : SerializableDataExtensionBase
    {

        public static Dictionary<ushort, RawItem> RawState;
        private const string DataId = "ChooseYourRocket";

        public override void OnLoadData()
        {
            var data = serializableDataManager.LoadData(DataId);
            if (data == null)
            {
                RawState = null;
                return;
            }
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream(data);
            RawState = (Dictionary<ushort,RawItem>)binFormatter.Deserialize(mStream);
        }

        public override void OnSaveData()
        {
            var binFormatter = new BinaryFormatter();
            var mStream = new MemoryStream();
            binFormatter.Serialize(mStream, Configuration.ToRawConfig());
            serializableDataManager.SaveData(DataId, mStream.ToArray());
        }

        [Serializable]
        public struct RawItem
        {
            public string Rocket { get; set; }
            public string Crawler { get; set; }
        }
    }
}
