using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Gameplay.Util
{
    [DisplayName("EditorSignalEmitter")]
    [CustomStyle("SignalEmitter")]
    public class EditorMarker : Marker, INotification, INotificationOptionProvider
    {
        [SerializeField] SignalAsset m_Asset;
        [SerializeField] public bool emitOnce;
        [SerializeField] public bool emitInEditor;
        
        
        /// <summary>
        /// Asset representing the signal being emitted.
        /// </summary>
        public SignalAsset asset
        {
            get => m_Asset;
            set => m_Asset = value;
        }

        public PropertyName id
        {
            get
            {
                if (m_Asset != null)
                {
                    return new PropertyName(m_Asset.name);
                }
                return new PropertyName(string.Empty);
            }
        }
        
        NotificationFlags INotificationOptionProvider.flags =>
            (emitOnce ? NotificationFlags.TriggerOnce : default) |
            (emitInEditor ? NotificationFlags.TriggerInEditMode : default); 
    }
}