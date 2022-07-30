using EpicTransport;
using TMPro;
using UnityEngine;

namespace Gameplay.Controllers
{
    public class DelayedInit : MonoBehaviour
    {
        public EOSSDKComponent component;

        public TMP_InputField authField;


        public void Init()
        {
            component.devAuthToolCredentialName = authField.text;
            EOSSDKComponent.Initialize();
        }

    }
}