using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;

namespace ChooseYourRocket
{
    public class GamePanelExtender : MonoBehaviour
    {
        private bool _initialized;
        private ChirpXPanel _chirpXPanel;
        private UIDropDown _rocketDropDown;
        private UILabel _rocketLabel;
        private List<string> _assetNames;

        public void OnDestroy()
        {
            if (_rocketDropDown != null)
            {
                if (_chirpXPanel != null)
                {
                    _chirpXPanel.component.RemoveUIComponent(_rocketDropDown);
                }
                Destroy(_rocketDropDown.gameObject);
                _rocketDropDown = null;
            }
            if (_rocketLabel != null)
            {
                if (_chirpXPanel != null)
                {
                    _chirpXPanel.component.RemoveUIComponent(_rocketLabel);
                }
                Destroy(_rocketLabel.gameObject);
                _rocketLabel = null;
            }
            _initialized = false;
        }

        public void Update()
        {

            if (!_initialized)
            {
                var go = GameObject.Find("(Library) ChirpXPanel");
                if (go == null)
                {
                    return;
                }
                var infoPanel = go.GetComponent<ChirpXPanel>();
                if (infoPanel == null)
                {
                    return;
                }
                _chirpXPanel = infoPanel;
                _rocketDropDown = UiUtil.CreateDropDown(_chirpXPanel.component);
                _rocketDropDown.width = 250;
                _rocketDropDown.relativePosition = new Vector3(16, 345);
                _assetNames = new List<string>();
                PrefabLister.ListRockets().ForEach(r =>
                {
                    _assetNames.Add(r.name);
                    _rocketDropDown.AddItem(Util.CleanName(r.name));
                });
                _rocketDropDown.eventSelectedIndexChanged += IndexChangeHandler;
                _rocketLabel = UiUtil.CreateLabel("Rocket type", _chirpXPanel.component, new Vector3(16,325));
                _initialized = true;
            }
            if (!_chirpXPanel.component.isVisible)
            {
                return;
            }
            SetUpRocketDropDown();
        }

        private void IndexChangeHandler(UIComponent component, int value)
        {
            if (value < 0)
            {
                return;
            }
            var eventId = (ushort)Util.GetInstanceField(typeof(ChirpXPanel), _chirpXPanel, "m_currentEventID");
            var assetName = _assetNames[value];
            if (assetName == null)
            {
                return;
            }
            Configuration.SetRocket(eventId, assetName);
        }

        private void SetUpRocketDropDown()
        {
            var eventId = (ushort) Util.GetInstanceField(typeof(ChirpXPanel), _chirpXPanel, "m_currentEventID");
            var rocket = Configuration.Get(eventId).Rocket;
            var index = _assetNames.IndexOf(rocket.name);
            _rocketDropDown.selectedIndex = index;

        }
    }
}
