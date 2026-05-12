using System.Collections.Generic;
using Common.Info;
using Common.Scene;
using Common.Scene.Parameter;
using Common.Template;
using Common.Template.Interface;
using Cysharp.Threading.Tasks;
using Generated.Table;
using InGame.Component.Model;
using UnityEngine;

namespace Common
{
    public class Global : Singleton<Global>
    {
        private readonly List<IUpdateable> _updateables = new ();
        private readonly List<IFixedUpdateable> _fixedUpdateables = new ();
        
        public SceneLoader SceneLoader { get; private set; }
        public TableManager TableManager { get; private set; }
        public AssetManager AssetManager { get; private set; }

        public void Init() => Load().Forget();

        private async UniTask Load()
        {
            TableManager = InitGlobal<TableManager>();
            await TableManager.Init();
            SceneLoader = InitGlobal<SceneLoader>();
            AssetManager = InitGlobal<AssetManager>();
            
            //메인으로 바로 넘어가게 => 테스트 코드임
            List<WeaponStatusData> weapons = new();
            weapons.Add(Instance.TableManager.WeaponStatusRecord.GetRecord(1001));
            CharacterModel characterModel = new(Instance.TableManager.CharacterRecord.GetRecord(1001), weapons);
            var playerInfo = new PlayerInfo(characterModel);
            var stageData = Instance.TableManager.StageRecord.GetRecord(0101);
            var mainParameter = new SceneParameterMain(stageData, playerInfo);
            SceneLoader.LoadScene(GameDefine.SceneType.Main, mainParameter);
        }

        private T InitGlobal<T>() where T : MonoBehaviour
        {
            GameObject dontDestroyObject = new GameObject(typeof(T).Name);
            T component = dontDestroyObject.AddComponent<T>();
            dontDestroyObject.transform.SetParent(Instance.transform);
            return component;
        }

        #region LifeCycle
        private void Update()
        {
            for (int i = _updateables.Count - 1; i >= 0; i--)
            {
                _updateables[i].OnUpdate();
            }
        }

        private void FixedUpdate()
        {
            for(int i = _fixedUpdateables.Count - 1; i >= 0; i--)
            {
                _fixedUpdateables[i].OnFixedUpdate();
            }
        }
        #endregion

        #region Bind Events
        public void BindUpdate(IUpdateable updateable) => _updateables.Add(updateable);
        public void UnBindUpdate(IUpdateable updateable) => _updateables.Remove(updateable);
        public void BindFixedUpdate(IFixedUpdateable updateable) => _fixedUpdateables.Add(updateable);
        public void UnBindFixedUpdate(IFixedUpdateable updateable) => _fixedUpdateables.Remove(updateable);
        #endregion
    }
}
