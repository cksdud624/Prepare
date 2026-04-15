using Common;
using InGame.Controller;
using UnityEngine;
using static Common.GameDefine;

namespace InGame.Object
{
    public class ObjectHub
    {
        public ObjectState State { get; set; }
        public GameObject Model { get; set; }
        public Rigidbody Rigidbody { get; set; }
        public Collider Collider { get; set; }
        public ControllerBase Controller { get; set; }
        public bool isPlayer;
    }
}
