using System;
using UnityEngine.EventSystems;
using Ami.BroAudio;

namespace GearEngine.Core.ViewUtility.Audio
{
    [Serializable]
    public class AudioEventTriggerEntry
    {
        public EventTriggerType eventID;
        public SoundID sound;
    }
}
