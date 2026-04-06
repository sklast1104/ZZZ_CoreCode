using System;
using System.Collections.Generic;
using JM.UI.Panel;
using UnityEngine;

namespace JM
{
    public class SimpleUIMgr : UnitySingleton<SimpleUIMgr>
    {
        [SerializeField] private RectTransform hudCanvas;
        [SerializeField] private RectTransform normalCanvas;
        [SerializeField] private RectTransform topCanvas;

        [Serializable]
        public struct PrefabEntry
        {
            public string name;
            public GameObject prefab;
        }

        [SerializeField] private PrefabEntry[] prefabs;

        private Dictionary<string, GameObject> prefabMap;
        private RectTransform[] canvass;

        private List<AbstractBasePanel> hudStack;
        private List<AbstractBasePanel> normalStack;
        private List<AbstractBasePanel> topStack;
        private List<AbstractBasePanel>[] stacks;

        protected override void Awake()
        {
            base.Awake();

            canvass = new[] { hudCanvas, normalCanvas, topCanvas };

            hudStack = new List<AbstractBasePanel>();
            normalStack = new List<AbstractBasePanel>();
            topStack = new List<AbstractBasePanel>();
            stacks = new[] { hudStack, normalStack, topStack };

            prefabMap = new Dictionary<string, GameObject>();
            if (prefabs != null)
            {
                foreach (var entry in prefabs)
                {
                    if (!string.IsNullOrEmpty(entry.name) && entry.prefab != null)
                        prefabMap[entry.name] = entry.prefab;
                }
            }
        }

        private T Init<T>(GameObject go, UILayer layer) where T : AbstractBasePanel
        {
            var panel = go.GetComponent<T>();

            var stk = stacks[(int)layer];
            if (stk.Count > 0)
            {
                stk[^1].OnPause();
            }
            stk.Add(panel);

            panel.OnResume();
            panel.PlayEnter();
            return panel;
        }

        public T PushPanel<T>(UILayer layer = UILayer.Normal) where T : AbstractBasePanel
        {
            var go = LoadAssetInLayer(typeof(T).Name, layer);
            return Init<T>(go, layer);
        }

        public void PopPanel(AbstractBasePanel panel, UILayer layer = UILayer.Normal)
        {
            var stk = stacks[(int)layer];
            if (stk.Count == 0 || stk[^1] != panel)
            {
                Debug.LogError($"PopPanel: panel not at top of stack");
                return;
            }
            stk.RemoveAt(stk.Count - 1);

            panel.OnPause();
            panel.onPlayExitFinished += () =>
            {
                Destroy(panel.gameObject);
            };
            panel.PlayExit();

            if (stk.Count > 0)
            {
                stk[^1].OnResume();
            }
        }

        public T AddHUD<T>()
        {
            return AddView<T>(UILayer.HUD);
        }

        public T AddTop<T>()
        {
            return AddView<T>(UILayer.Top);
        }

        private GameObject LoadAssetInLayer(string location, UILayer layer)
        {
            if (!prefabMap.TryGetValue(location, out var prefab))
            {
                Debug.LogError($"SimpleUIMgr: prefab '{location}' not found in registry");
                return null;
            }
            return Instantiate(prefab, canvass[(int)layer]);
        }

        private T AddView<T>(UILayer layer)
        {
            string location = typeof(T).Name;
            var go = LoadAssetInLayer(location, layer);
            if (go == null) return default;
            return go.GetComponent<T>();
        }
    }
}
