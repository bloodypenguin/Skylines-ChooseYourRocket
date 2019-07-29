using System;
using System.Linq;
using ColossalFramework;
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

                PrefabLister.ListRockets().ForEach(r =>
                {
                    _rocketDropDown.AddItem(r.name);
                });
                _rocketDropDown.eventSelectedIndexChanged += IndexChangeHandler;
                _rocketLabel = UiUtil.CreateLabel("Rocket", _chirpXPanel.component, new Vector3());
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
            String name = _rocketDropDown.items[value];
            if (name == null)
            {
                return;
            }
            Configuration.SetRocket(eventId, name);
        }

        private void SetUpRocketDropDown()
        {
            var eventId = (ushort) Util.GetInstanceField(typeof(ChirpXPanel), _chirpXPanel, "m_currentEventID");
            VehicleInfo rocket = Configuration.Get(eventId).Rocket;
            int index = _rocketDropDown.items.ToList().IndexOf(rocket.name);
            _rocketDropDown.selectedIndex = index;

        }
    }
}
