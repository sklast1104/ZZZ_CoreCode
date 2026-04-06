using System;
using UnityEngine.Serialization;

namespace JM.TimelineAction
{
    public enum ECameraFollowForwardMode
    {
        Disabled = 0,
        Tracking = 1,  // 매 프레임 캐릭터 forward 추적
        SnapLock = 2,  // 시전 시점에 백뷰 스냅 + 카메라 완전 고정
    }

    [Serializable]
    public class ActionArgs
    {
        public bool enableRotation = false;
        public bool enableRecenter = false;
        [FormerlySerializedAs("lookAtMonster")] public bool enableLookAtMonster = false;
        [FormerlySerializedAs("enableCameraFollowForward")]
        public ECameraFollowForwardMode cameraFollowForwardMode = ECameraFollowForwardMode.Disabled;
        public bool disableMonsterPush = false;
        public bool ultimateMode = false;
        public ERoleShowState roleShowState = ERoleShowState.Front;

        // public static ActionParams GetStateDefault(EActionState state)
        // {
        //     var ret = new ActionParams();
        //     switch (state)
        //     {
        //         case EActionState.Idle:
        //         {
        //             ret.enableRotation = true;
        //             break;
        //         }
        //         case EActionState.Dodge_Front:
        //         {
        //             ret.enableRotation = true;
        //             break;
        //         }
        //         case EActionState.Walk:
        //         {
        //             ret.enableRotation = true;
        //             ret.enableRecenter = true;
        //             break;
        //         }
        //         case EActionState.Run:
        //         {
        //             ret.enableRotation = true;
        //             ret.enableRecenter = true;
        //             break;
        //         }
        //         case EActionState.Attack_Normal:
        //         {
        //             ret.lookAtMonster = true;
        //             break;
        //         }
        //         case EActionState.Attack_Special:
        //         {
        //             ret.lookAtMonster = true;
        //             break;
        //         }
        //         case EActionState.Attack_Ex_Special:
        //         {
        //             ret.lookAtMonster = true;
        //             break;
        //         }
        //     }
        //     return ret;
        // }
    }
}