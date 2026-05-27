using Cysharp.Threading.Tasks;
using UnityEngine;

namespace InGame.UI
{
    public class InGameUIView : MonoBehaviour
    {
        [SerializeField] private GameObject aimPoint;

        public async UniTask Init()
        {
            aimPoint.SetActive(false);
            await UniTask.CompletedTask;
        }
        
        public void SetAimPointActive(bool isActive) => aimPoint.SetActive(isActive);
    }
}
