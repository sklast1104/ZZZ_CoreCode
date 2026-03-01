using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace JM.TimelineAction
{
    // 액션 목록
    // 일반적으로 액션 목록은 특정 캐릭터에 속함
    // 한 캐릭터에 많은 액션이 있으므로 한 곳에서 편집
    [MovedFrom(true, sourceClassName: "ActionListSO")]
    [CreateAssetMenu(fileName = "ActionList", menuName = "TimelineAction/Action List")]
    public class ActionListSO : ScriptableObject
    {
        // 액션 이름에 캐릭터명 접두사가 포함되어 파일 구분에 사용되지만, 재생 시에는 접두사 없이 사용
        public string namePrefix;
        public List<ActionSO> actions;

        public string RemoveNamePrefix(string actionFullName)
        {
            if (!string.IsNullOrEmpty(namePrefix))
            {
                if (actionFullName.StartsWith(namePrefix))
                {
                    return actionFullName[namePrefix.Length..];
                }
                Debug.LogError($"접두사 제거 불가 - 전체 이름: {actionFullName}, 접두사: {namePrefix}");
            }
            else
            {
                Debug.Log($"{name} 접두사가 비어있음");
            }
            return actionFullName;
        }

        public Dictionary<string, ActionSO> ActionDict
        {
            get
            {
                if (actions == null) return new Dictionary<string, ActionSO>();
                var dict = actions
                    .Where(x => x != null)
                    .ToDictionary(x => RemoveNamePrefix(x.name));
                return dict;
            }
        }

        public string ToJson()
        {
            return "";
        }
    }
}