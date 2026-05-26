using System;
using System.Collections.Generic;
using Common;
using Cysharp.Threading.Tasks;
using InGame.Component.Hub;
using InGame.Component.Model;
using InGame.Component.Module;
using InGame.Model;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using static Common.AssetKeys;

namespace InGame.Component
{
    public class ComponentBank : MonoBehaviour
    {
        private InGameModel _inGameModel;
        public GameObject Model { get; private set; }
        public AnimationPlayer AnimationPlayer { get; private set; }
        public CameraController CameraController { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        public Collider Collider { get; private set; }
        public InputHub InputHub { get; private set; }
        public CharacterModel CharacterModel { get; private set; }
        
        private readonly HashSet<Collider> _groundContacts = new();
        public bool HasGroundContact => _groundContacts.Count > 0;
        private IDisposable _collisionEnterDisposable;
        private IDisposable _collisionExitDisposable;

        public async UniTask Init(InGameModel inGameModel, InputHub inputHub, CharacterModel characterModel)
        {
            _inGameModel = inGameModel;
            CharacterModel = characterModel;
            InputHub = inputHub;

            var assetManager = Global.Instance.AssetManager;
            var characterData = characterModel.CharacterData;
            var model = await assetManager.LoadAssetAsync<GameObject>(LoadTarget.Model, characterData.Id.ToString());
            if (model == null)
            {
                Debug.LogWarning("Model not found: " + CharacterModel.CharacterData.Id);
                return;
            }
            Model = Instantiate(model, transform);
            Model.transform.position = Vector3.zero;
            Model.transform.rotation = Quaternion.identity;
            
            AnimationPlayer = gameObject.AddComponent<AnimationPlayer>();
            var animator = Model.GetComponent<Animator>();
            await AnimationPlayer.Init(_inGameModel, animator == null ? Model.AddComponent<Animator>() : animator, characterModel);

            CameraController = gameObject.AddComponent<CameraController>();
            await CameraController.Init(_inGameModel, inputHub, characterModel);
            
            Rigidbody = gameObject.AddComponent<Rigidbody>();
            Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            
            var defaultCollider = gameObject.AddComponent<CapsuleCollider>();
            defaultCollider.center = GameDefine.DefaultColliderCenter;
            defaultCollider.radius = GameDefine.DefaultColliderRadius;
            defaultCollider.height = GameDefine.DefaultColliderHeight;
            Collider = defaultCollider;
            _collisionEnterDisposable = Collider.OnCollisionEnterAsObservable().Subscribe(NotifyOnCollisionEnter);
            _collisionExitDisposable = Collider.OnCollisionExitAsObservable().Subscribe(NotifyOnCollisionExit);
        }
        
        #region Events
        private void NotifyOnCollisionEnter(Collision collision)
        {
            foreach (var contact in collision.contacts)
            {
                if (contact.normal.y > 0.7f)
                {
                    _groundContacts.Add(collision.collider);
                    CharacterModel.IsLand.Value = true;
                    return;
                }
            }
        }

        private void NotifyOnCollisionExit(Collision collision)
        {
            _groundContacts.Remove(collision.collider);
            if (_groundContacts.Count == 0)
                CharacterModel.IsLand.Value = false;
        }
        #endregion
        

        public void Dispose()
        {
            _collisionEnterDisposable?.Dispose();
            _collisionExitDisposable?.Dispose();
            var assetManager = Global.Instance?.AssetManager;
            if (assetManager == null) return;
            assetManager.ReleaseAsset<GameObject>(LoadTarget.Model, CharacterModel.CharacterData.Id.ToString());
        }
    }
}
